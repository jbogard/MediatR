namespace MediatR.Internal
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class RequestHandlerBase
    {
        public abstract Task<object?> HandleAsync(object request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);

        protected static THandler GetHandler<THandler>(ServiceFactory factory)
        {
            THandler handler;

            try
            {
                handler = factory.GetInstance<THandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
            }

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
            }

            return handler;
        }
    }

    internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        public abstract Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<object?> HandleAsync(object request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            return HandleAsync((IRequest<TResponse>)request, cancellationToken, serviceFactory)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        ExceptionDispatchInfo.Capture(t.Exception.InnerException).Throw();
                    }
                    return (object?)t.Result;
                }, cancellationToken);
        }

        public override Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).HandleAsync((TRequest) request, cancellationToken);

            return serviceFactory
                .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.HandleAsync((TRequest)request, cancellationToken, next))();
        }
    }
}
