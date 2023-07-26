using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling.RequestResponse.Subscription;

internal sealed class CachedRequestResponseExceptionRequestHandler<TRequest, TResponse, TException> : RequestResponseExceptionRequestHandler
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    private IRequestResponseExceptionHandler<TRequest, TResponse, TException>? _cachedHandler;

    public override Task Handle<TMethodRequest, TMethodResponse>(
        TMethodRequest baseRequest,
        Exception exception,
        RequestResponseExceptionHandlerState<TMethodResponse> state,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");
        Debug.Assert(typeof(TResponse) == typeof(TMethodResponse), "response type and method response type must be the same type.");
        
        var possibleHandler = (IRequestResponseExceptionHandler<TMethodRequest, TMethodResponse, TException>?) GetHandler(serviceProvider);

        return possibleHandler is null ?
            Task.CompletedTask :
            possibleHandler.Handle(baseRequest, (TException) exception, state, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestResponseExceptionHandler<TRequest, TResponse, TException>? GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetService<IRequestResponseExceptionHandler<TRequest, TResponse, TException>>();
}