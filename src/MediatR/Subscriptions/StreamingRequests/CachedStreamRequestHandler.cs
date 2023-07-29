using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.StreamingRequests;

internal sealed class CachedStreamRequestHandler<TRequest, TResponse> : StreamRequestHandler
    where TRequest : IStreamRequest<TResponse>
{
    private StreamHandlerDelegate<TRequest, TResponse>? _cachedHandler;

    public override IAsyncEnumerable<TMethodResponse> Handle<TMethodResponse>(IStreamRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), $"Response '{typeof(TResponse)}' and method response '{typeof(TMethodResponse)}' must always be the same type.");

        if (_cachedHandler is not null)
        {
            _cachedHandler((TRequest)request, cancellationToken);
        }
        
        var behaviors = GetBehaviors(serviceProvider);
        StreamHandlerDelegate<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        _cachedHandler = handler;

        return (IAsyncEnumerable<TMethodResponse>) handler((TRequest) request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IStreamPipelineBehavior<TRequest, TResponse>[] GetBehaviors(IServiceProvider serviceProvider) =>
        serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
}