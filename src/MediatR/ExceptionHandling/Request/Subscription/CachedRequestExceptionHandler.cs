using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling.Request.Subscription;

internal sealed class CachedRequestExceptionHandler<TRequest, TException> : RequestExceptionRequestHandler
    where TRequest : IRequest
    where TException : Exception
{
    private IRequestExceptionHandler<TRequest, TException>? _cachedHandler;

    public override Task HandleAsync<TMethodRequest>(TMethodRequest request, Exception exception, RequestExceptionHandlerState state, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var possibleHandler = (IRequestExceptionHandler<TMethodRequest, TException>?)GetHandler(serviceProvider);
        return possibleHandler is null ?
            Task.CompletedTask :
            possibleHandler.Handle(request, (TException) exception, state, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestExceptionHandler<TRequest, TException>? GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetService<IRequestExceptionHandler<TRequest, TException>>();
}
