using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Requests;

internal sealed class CachedRequestResponseHandler<TRequest, TResponse> : RequestResponseHandler
    where TRequest : IRequest<TResponse>
{
    private RequestHandlerDelegate<TRequest, TResponse>? _cachedHandler;

    public override ValueTask<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), $"Response '{typeof(TResponse)}' and method response '{typeof(TMethodResponse)}' must always be the same type.");

        if (_cachedHandler is not null)
        {
            return Unsafe.As<ValueTask<TResponse>, ValueTask<TMethodResponse>>(
                ref Unsafe.AsRef(_cachedHandler(
                    Unsafe.As<IRequest<TMethodResponse>, TRequest>(ref request), 
                    cancellationToken)));
        }
        
        var behaviors = GetBehaviors(serviceProvider);
        RequestHandlerDelegate<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        _cachedHandler = handler;

        return Unsafe.As<ValueTask<TResponse>, ValueTask<TMethodResponse>>(
            ref Unsafe.AsRef(_cachedHandler(
                Unsafe.As<IRequest<TMethodResponse>, TRequest>(ref request),
                cancellationToken)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IPipelineBehavior<TRequest, TResponse>[] GetBehaviors(IServiceProvider serviceProvider) =>
        serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
}