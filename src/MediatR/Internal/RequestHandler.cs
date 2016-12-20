namespace MediatR.Internal
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class RequestHandlerBase
    {
        protected static object GetHandler(Type requestType, SingleInstanceFactory singleInstanceFactory)
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

        protected static THandler GetHandler<THandler>(SingleInstanceFactory factory)
        {
            return (THandler) GetHandler(typeof(THandler), factory);
        }

        protected static InvalidOperationException BuildException(object message)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.");
        }
    }

    internal abstract class RequestHandler : RequestHandlerBase
    {
        public abstract Task Handle(IRequest request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory);
    }

    internal abstract class RequestHandler<TResponse> : RequestHandlerBase
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory);
    }

    internal class RequestHandlerImpl<TRequest, TResponse> : RequestHandler<TResponse>
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
                () => GetHandlerFactory(t => GetHandler(t, factory)));

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

    internal class RequestHandlerImpl<TRequest> : RequestHandler
        where TRequest : IRequest
    {
        private Func<TRequest, CancellationToken, SingleInstanceFactory, RequestHandlerDelegate<Unit>> _handlerFactory;
        private object _syncLock = new object();

        public override Task Handle(IRequest request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory)
        {
            var handler = GetHandler((TRequest)request, cancellationToken, singleFactory);

            var pipeline = GetPipeline((TRequest)request, handler, multiFactory);

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
}