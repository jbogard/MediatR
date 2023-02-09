using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Wrappers;

public abstract class RequestHandlerBase
{
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

public abstract class RequestHandlerWrapper : RequestHandlerBase
{
    public abstract Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken) =>
        await Handle((IRequest<TResponse>) request, serviceProvider, cancellationToken).ConfigureAwait(false);

    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Task<TResponse> Handler() => serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
            .Handle((TRequest) request, cancellationToken);

        return serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>) Handler,
                (next, pipeline) => () => pipeline.Handle((TRequest) request, next, cancellationToken))();
    }
}

public class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken) =>
        await Handle((IRequest) request, serviceProvider, cancellationToken).ConfigureAwait(false);

    public override Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        async Task<Unit> Handler()
        {
            await serviceProvider.GetRequiredService<IRequestHandler<TRequest>>()
                .Handle((TRequest) request, cancellationToken);

            return Unit.Value;
        }

        return serviceProvider
            .GetServices<IPipelineBehavior<TRequest, Unit>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<Unit>) Handler,
                (next, pipeline) => () => pipeline.Handle((TRequest) request, next, cancellationToken))();
    }
}