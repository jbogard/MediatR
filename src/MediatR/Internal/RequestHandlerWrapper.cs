namespace MediatR.Internal
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class RequestHandlerBase
    {
        public abstract Task<object> Handle(
            object request,
            CancellationToken cancellationToken,
            ServiceFactory serviceFactory);

        protected static IEnumerable<THandler> GetHandlers<THandler>(ServiceFactory factory)
        {
            IEnumerable<THandler> handlers;

            try
            {
                handlers = factory.GetInstances<THandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
            }

            if (handlers?.Any() ?? false)
            {
                return handlers;
            }

            throw new InvalidOperationException(
                    $"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
        }

        protected static THandler GetHandler<THandler>(ServiceFactory factory)
        {
            var handlers = GetHandlers<THandler>(factory);

            if (handlers.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"There are more than one handler for request of type {typeof(THandler)}. Use SendToMany or remove unnessesary handlers.");
            }

            return handlers.FirstOrDefault();
        }
    }

    internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);

        public abstract Task<TResponse[]> HandleMany(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<object> Handle(object request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            return Handle((IRequest<TResponse>)request, cancellationToken, serviceFactory)
                .ContinueWith(t => (object) t.Result);
        }

        public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            Task<TResponse> Handler() =>
                GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest)request, cancellationToken);

            return serviceFactory
                  .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
                  .Reverse()
                  .Aggregate((RequestHandlerDelegate<TResponse>)Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
        }

        public override Task<TResponse[]> HandleMany(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory)
        {
            Task<TResponse[]> Handler()
            {
                var handlers = GetHandlers<IRequestHandler<TRequest, TResponse>>(serviceFactory);
                var _result = handlers.Select(h => h.Handle((TRequest)request, cancellationToken));
                return Task.WhenAll(_result);
            }

            //var instances = serviceFactory.GetInstances<IPipelineBehavior<TRequest, TResponse[]>>();
            //var result = (RequestHandlerDelegate<TResponse[]>)Handler;
            //foreach (var behavior in instances.Reverse())
            //{
            //    var prevResult = result;
            //    result = () => behavior.Handle((TRequest)request, cancellationToken, prevResult);
            //}

            //return result();

            return serviceFactory
                   .GetInstances<IPipelineBehavior<TRequest, TResponse[]>>()
                   .Reverse()
                   .Aggregate((RequestHandlerDelegate<TResponse[]>)Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
        }
    }
}
