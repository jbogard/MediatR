using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions.Requests;

internal sealed class TransientRequestHandler<TRequest> : RequestHandler
    where TRequest : IRequest
{
    public override ValueTask HandleAsync<TMethodRequest>(TMethodRequest methodRequest, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var request = Unsafe.As<TMethodRequest, TRequest>(ref methodRequest);
        var handler = GetHandler(serviceProvider);

        return handler(request, cancellationToken);
    }

    public override ValueTask HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(request.GetType() == typeof(TRequest), "The request type must be the same.");

        var handler = GetHandler(serviceProvider);

        return handler((TRequest) request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RequestHandlerDelegate<TRequest> GetHandler(IServiceProvider serviceProvider)
    {
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest>>();
        RequestHandlerDelegate<TRequest> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>().Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler;
    }
}