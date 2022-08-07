namespace MediatR.Wrappers;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class RequestHandlerBase : HandlerBase
{
    public abstract Task<object?> Handle(object request, ServiceFactory serviceFactory,
        CancellationToken cancellationToken);

}

public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, ServiceFactory serviceFactory,
        CancellationToken cancellationToken);
}

public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, ServiceFactory serviceFactory,
        CancellationToken cancellationToken) =>
        await Handle((IRequest<TResponse>)request, serviceFactory, cancellationToken).ConfigureAwait(false);

    public override Task<TResponse> Handle(IRequest<TResponse> request, ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);

        return serviceFactory
            .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, next, cancellationToken))();
    }
}