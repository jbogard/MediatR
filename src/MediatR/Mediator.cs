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
        private static readonly ConcurrentDictionary<Type, Delegate> _requestHandlerFactories = new ConcurrentDictionary<Type, Delegate>();
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
            var handler = GetHandler(request, cancellationToken);

            var pipeline = GetPipeline(request, handler);

            return pipeline;
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


        private RequestHandlerDelegate<TResponse> GetHandler<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var requestType = request.GetType();

            var handlerFactory = (Func<IRequest<TResponse>, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<TResponse>>)
                _requestHandlerFactories.GetOrAdd(requestType, GetHandlerFactory<TResponse>(requestType, GetHandler));

            if (handlerFactory == null)
            {
                throw BuildException(request);
            }

            return handlerFactory(request, cancellationToken, _singleInstanceFactory);
        }

        private object GetHandler(Type requestType)
        {
            try
            {
                return _singleInstanceFactory(requestType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Func<IRequest<TResponse>, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<TResponse>>
            GetHandlerFactory<TResponse>(Type requestType, SingleInstanceFactory factory)
        {
            var responseType = typeof(TResponse);
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            if (factory(handlerType) != null)
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                var wrapper = (RequestHandlerWrapper<TResponse>)Activator.CreateInstance(wrapperType);
                return (request, token, fac) => () =>
                {
                    var handler = fac(handlerType);
                    return Task.FromResult(wrapper.Handle(request, handler));
                };
            }
            handlerType = typeof(IAsyncRequestHandler<,>).MakeGenericType(requestType, responseType);
            if (factory(handlerType) != null)
            {
                var wrapperType = typeof(AsyncRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                var wrapper = (AsyncRequestHandlerWrapper<TResponse>)Activator.CreateInstance(wrapperType);
                return (request, token, fac) =>
                {
                    var handler = fac(handlerType);
                    return () => wrapper.Handle(request, handler);
                };
            }
            handlerType = typeof(ICancellableAsyncRequestHandler<,>).MakeGenericType(requestType, responseType);
            if (factory(handlerType) != null)
            {
                var wrapperType = typeof(CancellableAsyncRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                var wrapper = (CancellableAsyncRequestHandlerWrapper<TResponse>)Activator.CreateInstance(wrapperType);
                return (request, token, fac) =>
                {
                    var handler = fac(handlerType);
                    return () => wrapper.Handle(request, token, handler);
                };
            }
            return null;
        }

        private Task<TResponse> GetPipeline<TResponse>(object request, RequestHandlerDelegate<TResponse> invokeHandler)
        {
            var requestType = request.GetType();

            var wrapper = (PipelineBehaviorWrapper<TResponse>) _pipelineWrappers.GetOrAdd(requestType, t =>
            {
                var wrapperType = typeof(PipelineBehaviorWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
                return Activator.CreateInstance(wrapperType);
            });

            return wrapper.CreatePipeline(request, invokeHandler, _multiInstanceFactory);
        }

        private abstract class RequestHandler
        {
            public abstract Task Handle(object request, CancellationToken cancellationToken,
                SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory);
        }

        private class RequestHandlerImpl<TRequest> : RequestHandler
            where TRequest : IRequest
        {
            private Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<Unit>> _handlerFactory;
            private object _syncLock = new object();

            public override Task Handle(object request, CancellationToken cancellationToken, 
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
                    () => GetHandlerFactory(t => GetHandler(t, singleInstanceFactory)));

                if (!initialized || _handlerFactory == null)
                {
                    throw BuildException(request);
                }

                return _handlerFactory(request, cancellationToken, singleInstanceFactory);
            }

            private static Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<Unit>> 
                GetHandlerFactory(SingleInstanceFactory factory)
            {
                var handlerType = typeof(IRequestHandler<TRequest>);
                if (factory(handlerType) != null)
                {
                    return (request, token, fac) => () =>
                    {
                        var handler = (IRequestHandler<TRequest>)fac(handlerType);
                        handler.Handle(request);
                        return Task.FromResult(Unit.Value);
                    };
                }
                handlerType = typeof(IAsyncRequestHandler<TRequest>);
                if (factory(handlerType) != null)
                {
                    return (request, token, fac) => async () =>
                    {
                        var handler = (IAsyncRequestHandler<TRequest>)fac(handlerType);
                        await handler.Handle(request);
                        return Unit.Value;
                    };
                }
                handlerType = typeof(ICancellableAsyncRequestHandler<TRequest>);
                if (factory(handlerType) != null)
                {
                    return (request, token, fac) => async () =>
                    {
                        var handler = (ICancellableAsyncRequestHandler<TRequest>)fac(handlerType);
                        await handler.Handle(request, token);
                        return Unit.Value;
                    };
                }
                return null;
            }

            private object GetHandler(Type requestType, SingleInstanceFactory singleInstanceFactory)
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

            private Task<Unit> GetPipeline(TRequest request, RequestHandlerDelegate<Unit> invokeHandler, MultiInstanceFactory factory)
            {
                var behaviors = factory(typeof(IPipelineBehavior<TRequest, Unit>))
                    .Cast<IPipelineBehavior<TRequest, Unit>>()
                    .Reverse();

                var aggregate = behaviors.Aggregate(invokeHandler, (next, pipeline) => () => pipeline.Handle(request, next));

                return aggregate();
            }

        }

        private static InvalidOperationException BuildException(object message)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.");
        }
    }
}
