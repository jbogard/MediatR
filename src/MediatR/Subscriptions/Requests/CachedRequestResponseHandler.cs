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
    private IRequestHandler<TRequest, TResponse>? _cachedHandler;
    private IPipelineBehavior<TRequest, TResponse>[]? _cachedBehaviors;

    public override Task<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), "Response and method response must always be the same type.");

        var behaviors = GetBehaviors(serviceProvider);
        RequestHandlerDelegate<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return Unsafe.As<Task<TMethodResponse>>(handler((TRequest) request, cancellationToken));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IPipelineBehavior<TRequest, TResponse>[] GetBehaviors(IServiceProvider serviceProvider) =>
        _cachedBehaviors ??= serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
}