namespace MediatR
{
    using Internal;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly SingleInstanceFactory _singleInstanceFactory;
        private readonly MultiInstanceFactory _multiInstanceFactory;
        private static readonly ConcurrentDictionary<Type, RequestHandler> _voidRequestHandlers = new ConcurrentDictionary<Type, RequestHandler>();
        private static readonly ConcurrentDictionary<Type, object> _requestHandlers = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> _pipelineWrappers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="singleInstanceFactory">The single instance factory.</param>
        /// <param name="multiInstanceFactory">The multi instance factory.</param>
        public Mediator(SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
            _multiInstanceFactory = multiInstanceFactory;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestType = request.GetType();

            var handler = (RequestHandler<TResponse>)_requestHandlers.GetOrAdd(requestType,
                t => Activator.CreateInstance(typeof(RequestHandlerImpl<,>).MakeGenericType(requestType, typeof(TResponse))));

            return handler.Handle(request, cancellationToken, _singleInstanceFactory, _multiInstanceFactory);
        }

        public Task SendAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestType = request.GetType();

            var handler = _voidRequestHandlers.GetOrAdd(requestType,
                t => (RequestHandler) Activator.CreateInstance(typeof(RequestHandlerImpl<>).MakeGenericType(requestType)));

            return handler.Handle(request, cancellationToken, _singleInstanceFactory, _multiInstanceFactory);
        }

        public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken))
            where TNotification : INotification
        {
            var notificationType = typeof(TNotification);
            var notificationHandlers = _multiInstanceFactory(typeof(INotificationHandler<>).MakeGenericType(notificationType))
                .Cast<INotificationHandler<TNotification>>()
                .Select(handler =>
                {
                    handler.Handle(notification);
                    return Unit.Task;
                });
            var asyncNotificationHandlers = _multiInstanceFactory(typeof(IAsyncNotificationHandler<>).MakeGenericType(notificationType))
                .Cast<IAsyncNotificationHandler<TNotification>>()
                .Select(handler => handler.Handle(notification));
            var cancellableAsyncNotificationHandlers = _multiInstanceFactory(typeof(ICancellableAsyncNotificationHandler<>).MakeGenericType(notificationType))
                .Cast<ICancellableAsyncNotificationHandler<TNotification>>()
                .Select(handler => handler.Handle(notification, cancellationToken));

            var allHandlers = notificationHandlers
                .Concat(asyncNotificationHandlers)
                .Concat(cancellableAsyncNotificationHandlers);

            return Task.WhenAll(allHandlers);
        }

        private abstract class RequestHandler
        {
            public abstract Task Handle(IRequest request, CancellationToken cancellationToken,
                SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory);
        }

        private abstract class RequestHandler<TResponse>
        {
            public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
                SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory);
        }

        private class RequestHandlerImpl<TRequest, TResponse> : RequestHandler<TResponse>
            where TRequest : IRequest<TResponse>
        {
            private Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<TResponse>> _handlerFactory;
            private object _syncLock = new object();

            public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
                SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory)
            {
                var handler = GetHandler((TRequest)request, cancellationToken, singleFactory);

                var pipeline = GetPipeline((TRequest)request, handler, multiFactory);

                return pipeline;
            }

            private RequestHandlerDelegate<TResponse> GetHandler(TRequest request, CancellationToken cancellationToken, SingleInstanceFactory factory)
            {
                var initialized = false;

                LazyInitializer.EnsureInitialized(ref _handlerFactory, ref initialized, ref _syncLock,
                    () => GetHandlerFactory(t => Mediator.GetHandler(t, factory)));

                if (!initialized || _handlerFactory == null)
                {
                    throw BuildException(request);
                }

                return _handlerFactory(request, cancellationToken, factory);
            }

            private static Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<TResponse>>
                GetHandlerFactory(SingleInstanceFactory factory)
            {
                if (GetHandler<IRequestHandler<TRequest, TResponse>>(factory) != null)
                {
                    return (request, token, fac) => () =>
                    {
                        var handler = GetHandler<IRequestHandler<TRequest, TResponse>>(fac);
                        return Task.FromResult(handler.Handle(request));
                    };
                }
                if (GetHandler<IAsyncRequestHandler<TRequest, TResponse>>(factory) != null)
                {
                    return (request, token, fac) =>
                    {
                        var handler = GetHandler<IAsyncRequestHandler<TRequest, TResponse>>(fac);
                        return () => handler.Handle(request);
                    };
                }
                if (GetHandler<ICancellableAsyncRequestHandler<TRequest, TResponse>>(factory) != null)
                {
                    return (request, token, fac) =>
                    {
                        var handler = GetHandler<ICancellableAsyncRequestHandler<TRequest, TResponse>>(fac);
                        return () => handler.Handle(request, token);
                    };
                }
                return null;
            }

            private static Task<TResponse> GetPipeline(TRequest request, RequestHandlerDelegate<TResponse> invokeHandler, MultiInstanceFactory factory)
            {
                var behaviors = factory(typeof(IPipelineBehavior<TRequest, TResponse>))
                    .Cast<IPipelineBehavior<TRequest, TResponse>>()
                    .Reverse();

                var aggregate = behaviors.Aggregate(invokeHandler, (next, pipeline) => () => pipeline.Handle(request, next));

                return aggregate();
            }
        }

        private class RequestHandlerImpl<TRequest> : RequestHandler
            where TRequest : IRequest
        {
            private Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<Unit>> _handlerFactory;
            private object _syncLock = new object();

            public override Task Handle(IRequest request, CancellationToken cancellationToken, 
                SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory)
            {
                var handler = GetHandler((TRequest) request, cancellationToken, singleFactory);

                var pipeline = GetPipeline((TRequest) request, handler, multiFactory);

                return pipeline;
            }

            private RequestHandlerDelegate<Unit> GetHandler(TRequest request, CancellationToken cancellationToken, SingleInstanceFactory singleInstanceFactory)
            {
                var initialized = false;

                LazyInitializer.EnsureInitialized(ref _handlerFactory, ref initialized, ref _syncLock,
                    () => GetHandlerFactory(t => Mediator.GetHandler(t, singleInstanceFactory)));

                if (!initialized || _handlerFactory == null)
                {
                    throw BuildException(request);
                }

                return _handlerFactory(request, cancellationToken, singleInstanceFactory);
            }

            private static Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<Unit>> 
                GetHandlerFactory(SingleInstanceFactory factory)
            {
                if (GetHandler<IRequestHandler<TRequest>>(factory) != null)
                {
                    return (request, token, fac) => () =>
                    {
                        var handler = GetHandler<IRequestHandler<TRequest>>(fac);
                        handler.Handle(request);
                        return Task.FromResult(Unit.Value);
                    };
                }
                if (GetHandler<IAsyncRequestHandler<TRequest>>(factory) != null)
                {
                    return (request, token, fac) => async () =>
                    {
                        var handler = GetHandler<IAsyncRequestHandler<TRequest>>(fac);
                        await handler.Handle(request);
                        return Unit.Value;
                    };
                }
                if (GetHandler<ICancellableAsyncRequestHandler<TRequest>>(factory) != null)
                {
                    return (request, token, fac) => async () =>
                    {
                        var handler = GetHandler<ICancellableAsyncRequestHandler<TRequest>>(fac);
                        await handler.Handle(request, token);
                        return Unit.Value;
                    };
                }
                return null;
            }

            private static Task<Unit> GetPipeline(TRequest request, RequestHandlerDelegate<Unit> invokeHandler, MultiInstanceFactory factory)
            {
                var behaviors = factory(typeof(IPipelineBehavior<TRequest, Unit>))
                    .Cast<IPipelineBehavior<TRequest, Unit>>()
                    .Reverse();

                var aggregate = behaviors.Aggregate(invokeHandler, (next, pipeline) => () => pipeline.Handle(request, next));

                return aggregate();
            }
        }

        private static object GetHandler(Type requestType, SingleInstanceFactory singleInstanceFactory)
        {
            try
            {
                return singleInstanceFactory(requestType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static THandler GetHandler<THandler>(SingleInstanceFactory factory)
        {
            return (THandler)GetHandler(typeof(THandler), factory);
        }

        private static InvalidOperationException BuildException(object message)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.");
        }
    }
}
