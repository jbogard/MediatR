using System.Collections.Generic;

namespace MediatR.Wrappers
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class RequestHandlerBase
    {
        public abstract Task<object?> Handle(object request, CancellationToken cancellationToken,
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

    public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override async Task<object?> Handle(object request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory) =>
            await Handle((IRequest<TResponse>) request, cancellationToken, serviceFactory);

        public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);

            IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelineBehaviors;

            try
            {
                pipelineBehaviors = ((IBehaviorOrder) serviceFactory(typeof(IBehaviorOrder))).GetPipelineBehaviors<TRequest, TResponse>(serviceFactory);
            }
            catch (Exception)
            {
                pipelineBehaviors = (IEnumerable<IPipelineBehavior<TRequest, TResponse>>) serviceFactory(typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>));
            }

            return pipelineBehaviors
                .Reverse()
                .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest) request, cancellationToken, next))();
        }
    }
}
