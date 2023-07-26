using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Requests;

internal sealed class CachedRequestHandler<TRequest> : RequestHandler
    where TRequest : IRequest
{
    private IRequestHandler<TRequest>? _cachedHandler;
    private IPipelineBehavior<TRequest>[]? _cachedBehaviors;

    public override ValueTask HandleAsync<TMethodRequest>(TMethodRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var behaviors = (IPipelineBehavior<TMethodRequest>[])GetBehaviors(serviceProvider);
        RequestHandlerDelegate<TMethodRequest> handler = ((IRequestHandler<TMethodRequest>)GetHandler(serviceProvider)).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler(request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestHandler<TRequest> GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IPipelineBehavior<TRequest>[] GetBehaviors(IServiceProvider serviceProvider) =>
        _cachedBehaviors ??= serviceProvider.GetServices<IPipelineBehavior<TRequest>>();
}