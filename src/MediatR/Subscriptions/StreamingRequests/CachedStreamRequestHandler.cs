using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.StreamingRequests;

internal sealed class CachedStreamRequestHandler<TRequest, TResponse> : StreamRequestHandler
    where TRequest : IStreamRequest<TResponse>
{
    private IStreamRequestHandler<TRequest, TResponse>? _cachedHandler;
    private IStreamPipelineBehavior<TRequest, TResponse>[]? _cachedBehaviors;

    public override IAsyncEnumerable<TMethodResponse> Handle<TMethodResponse>(IStreamRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var behaviors = GetBehaviors(serviceProvider);
        StreamHandlerNext<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return (IAsyncEnumerable<TMethodResponse>) handler((TRequest) request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IStreamPipelineBehavior<TRequest, TResponse>[] GetBehaviors(IServiceProvider serviceProvider) =>
        _cachedBehaviors ??= serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
}