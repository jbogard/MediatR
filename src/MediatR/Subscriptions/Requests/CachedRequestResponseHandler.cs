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

    public override ValueTask<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> methodRequest, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(methodRequest is TRequest, "request type must be an inherited type of method request type.");
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), $"Response '{typeof(TResponse)}' and method response '{typeof(TMethodResponse)}' must always be the same type.");

        var request = Unsafe.As<IRequest<TMethodResponse>, TRequest>(ref methodRequest);
        var handler = GetHandler(serviceProvider);
        var response = handler(request, cancellationToken);

        return Unsafe.As<ValueTask<TResponse>, ValueTask<TMethodResponse>>(ref response);
    }

    public override async ValueTask<object?> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(request.GetType() == typeof(TRequest), "Request types must be the same.");

        var handler = GetHandler(serviceProvider);
        var response = await handler((TRequest) request, cancellationToken);

        return response;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RequestHandlerDelegate<TRequest, TResponse> GetHandler(IServiceProvider serviceProvider)
    {
        if (_cachedHandler is not null)
        {
            return _cachedHandler;
        }

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        RequestHandlerDelegate<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return _cachedHandler = handler;
    }
}