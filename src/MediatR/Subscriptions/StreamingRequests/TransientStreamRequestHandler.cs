using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.StreamingRequests;

internal sealed class TransientStreamRequestHandler<TRequest, TResponse> : StreamRequestHandler
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<TMethodResponse> HandleAsync<TMethodResponse>(IStreamRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), $"Response '{typeof(TResponse)}' and method response '{typeof(TMethodResponse)}' must always be the same type.");

        var handler = GetHandler(serviceProvider);

        return (IAsyncEnumerable<TMethodResponse>) handler((TRequest) request, cancellationToken);
    }

    public override async IAsyncEnumerable<object?> HandleAsync(object request, IServiceProvider serviceProvider, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var handler = GetHandler(serviceProvider);

        await foreach (var response in handler((TRequest)request, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return response;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StreamHandlerDelegate<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider)
    {
        var behaviors = serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
        StreamHandlerDelegate<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>().Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler;
    }
}