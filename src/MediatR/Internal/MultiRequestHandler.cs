using System.Collections.ObjectModel;

namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class MultiRequestHandlerBase
    {
        protected static IEnumerable<THandler> GetHandlers<THandler>(MultiInstanceFactory factory, ref Collection<Exception> resolveExceptions)
        {
            try
            {
                return factory(typeof(THandler)).Cast<THandler>();
            }
            catch (Exception e)
            {
                resolveExceptions?.Add(e);
                return null;
            }
        }

        protected static IEnumerable<THandler> GetHandlers<THandler>(MultiInstanceFactory factory)
        {
            Collection<Exception> swallowedExceptions = null;
            return GetHandlers<THandler>(factory, ref swallowedExceptions);
        }

        protected static InvalidOperationException BuildException(object message, Collection<Exception> resolveExceptions)
        {
            Exception innerException = null;
            if (resolveExceptions.Count == 1)
                innerException = resolveExceptions.First();
            else if (resolveExceptions.Count > 1)
                innerException = new AggregateException("Errors were encountered while resolving handlers", resolveExceptions);

            return new InvalidOperationException("Handlers were not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", innerException);
        }
    }

    internal abstract class MultiRequestHandler<TResponse> : MultiRequestHandlerBase
    {
        public abstract Task<TResponse[]> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory, Func<IEnumerable<Task<TResponse>>, Task<TResponse[]>> publish);
    }

    internal class MultiRequestHandlerImpl<TRequest, TResponse> : MultiRequestHandler<TResponse>
        where TRequest : IRequest<TResponse>
    {
        private Func<TRequest, CancellationToken, MultiInstanceFactory, IEnumerable<RequestHandlerDelegate<TResponse>>> _handlerFactory;
        private object _syncLock = new object();
        private bool _initialized = false;

        public override Task<TResponse[]> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory, Func<IEnumerable<Task<TResponse>>, Task<TResponse[]>> publish)
        {
            var handlers = GetHandlers((TRequest)request, cancellationToken, multiFactory);

            var pipeline = GetPipeline((TRequest)request, handlers, multiFactory);

            return publish(pipeline);
        }

        private IEnumerable<RequestHandlerDelegate<TResponse>> GetHandlers(TRequest request, CancellationToken cancellationToken, MultiInstanceFactory factory)
        {
            var resolveExceptions = new Collection<Exception>();
            LazyInitializer.EnsureInitialized(ref _handlerFactory, ref _initialized, ref _syncLock,
                () => GetHandlerFactories(factory, ref resolveExceptions));

            if (!_initialized || _handlerFactory == null)
                throw BuildException(request, resolveExceptions);

            return _handlerFactory(request, cancellationToken, factory);
        }

        private static Func<TRequest, CancellationToken, MultiInstanceFactory, IEnumerable<RequestHandlerDelegate<TResponse>>>
            GetHandlerFactories(MultiInstanceFactory factory, ref Collection<Exception> resolveExceptions)
        {
            if (GetHandlers<IRequestHandler<TRequest, TResponse>>(factory, ref resolveExceptions).Any())
            {
                return (request, token, fac) =>
                {
                    var handlers = GetHandlers<IRequestHandler<TRequest, TResponse>>(fac);
                    return handlers.Select(item => (RequestHandlerDelegate<TResponse>)(() => Task.FromResult(item.Handle(request))));
                };
            }

            if (GetHandlers<IAsyncRequestHandler<TRequest, TResponse>>(factory, ref resolveExceptions).Any())
            {
                return (request, token, fac) =>
                {
                    var handlers = GetHandlers<IAsyncRequestHandler<TRequest, TResponse>>(fac);
                    return handlers.Select(item => (RequestHandlerDelegate<TResponse>)(() => item.Handle(request)));
                };
            }

            if (GetHandlers<ICancellableAsyncRequestHandler<TRequest, TResponse>>(factory, ref resolveExceptions).Any())
            {
                return (request, token, fac) =>
                {
                    var handlers = GetHandlers<ICancellableAsyncRequestHandler<TRequest, TResponse>>(fac);
                    return handlers.Select(item => (RequestHandlerDelegate<TResponse>)(() => item.Handle(request, token)));
                };
            }

            return null;
        }

        private static IEnumerable<Task<TResponse>> GetPipeline(TRequest request, IEnumerable<RequestHandlerDelegate<TResponse>> invokeHandlers, MultiInstanceFactory factory)
        {
            var behaviors = factory(typeof(IPipelineBehavior<TRequest, TResponse>))
                .Cast<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse();

            var result = invokeHandlers.Select(item => behaviors.Aggregate(item, (next, pipeline) => () => pipeline.Handle(request, next)));

            return result.Select(p => p());
        }
    }
}
