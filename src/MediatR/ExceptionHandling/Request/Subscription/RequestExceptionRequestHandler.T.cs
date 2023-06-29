using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling;

internal sealed class RequestExceptionRequestHandler<TRequest, TException> : RequestExceptionRequestHandler
    where TRequest : IRequest
    where TException : Exception
{
    private readonly IServiceProvider _serviceProvider;

    private IRequestExceptionHandler<TRequest, TException>? _cachedHandler;

    public RequestExceptionRequestHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public override Task HandleAsync(IBaseRequest request, Exception exception, RequestExceptionHandlerState state, CancellationToken cancellationToken)
    {
        var possibleHandler = GetHandler();
        return possibleHandler is null ?
            Task.CompletedTask :
            possibleHandler.Handle((TRequest) request, (TException) exception, state, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestExceptionHandler<TRequest, TException>? GetHandler() =>
        _cachedHandler ??= _serviceProvider.GetService<IRequestExceptionHandler<TRequest, TException>>();
}