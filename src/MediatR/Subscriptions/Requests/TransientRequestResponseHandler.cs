using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Requests;

internal sealed class TransientRequestResponseHandler<TRequest, TResponse> : RequestResponseHandler
    where TRequest : IRequest<TResponse>
{
    public override ValueTask<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), "Response and method response must always be the same type.");

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        RequestHandlerDelegate<TRequest, TResponse> handler = GetHandler(serviceProvider).Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return Unsafe.As<ValueTask<TResponse>, ValueTask<TMethodResponse>>(ref Unsafe.AsRef(handler((TRequest) request, cancellationToken)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
}