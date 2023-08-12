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
    private RequestHandlerDelegate<TRequest>? _cachedHandler;

    public override ValueTask HandleAsync<TMethodRequest>(TMethodRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var handler = GetHandler(serviceProvider);

        var methodHandler = Unsafe.As<RequestHandlerDelegate<TRequest>, RequestHandlerDelegate<TMethodRequest>>(ref handler);
        return methodHandler(request, cancellationToken);
    }

    public override ValueTask HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(request.GetType() == typeof(TRequest), "The request type must be the same.");

        var handler = GetHandler(serviceProvider);

        return handler((TRequest) request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RequestHandlerDelegate<TRequest> GetHandler(IServiceProvider serviceProvider)
    {
        if (_cachedHandler is not null)
        {
            return _cachedHandler;
        }

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest>>();
        RequestHandlerDelegate<TRequest> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>().Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return _cachedHandler = handler;
    }
}