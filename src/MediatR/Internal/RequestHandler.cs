namespace MediatR.Internal
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class RequestHandlerBase
    {
        protected static THandler GetHandler<THandler>(SingleInstanceFactory factory)
        {
            THandler handler;

            try
            {
                handler = (THandler)factory(typeof(THandler));
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
        public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory)
        {
            Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(singleFactory).Handle((TRequest) request, cancellationToken);

            return multiFactory(typeof(IPipelineBehavior<TRequest, TResponse>))
                .Cast<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
        }
    }

    internal class RequestHandlerImpl<TRequest> : RequestHandler
        where TRequest : IRequest
    {
        public override Task Handle(IRequest request, CancellationToken cancellationToken,
            SingleInstanceFactory singleFactory, MultiInstanceFactory multiFactory)
        {
            async Task<Unit> Handler()
            {
                await GetHandler<IRequestHandler<TRequest>>(singleFactory).Handle((TRequest) request, cancellationToken).ConfigureAwait(false);
                return Unit.Value;
            }

            return multiFactory(typeof(IPipelineBehavior<TRequest, Unit>))
                .Cast<IPipelineBehavior<TRequest, Unit>>()
                .Reverse()
                .Aggregate((RequestHandlerDelegate<Unit>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
        }
    }
}
