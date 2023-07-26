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
    public override ValueTask HandleAsync<TMethodRequest>(TMethodRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var behaviors = (IPipelineBehavior<TMethodRequest>[]) serviceProvider.GetServices<IPipelineBehavior<TRequest>>();
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
    private static IRequestHandler<TRequest> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
}