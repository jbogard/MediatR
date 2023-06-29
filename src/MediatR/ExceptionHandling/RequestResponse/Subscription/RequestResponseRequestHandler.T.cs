using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling;

internal sealed class RequestResponseExceptionRequestHandler<TRequest, TResponse, TException> : RequestResponseExceptionRequestHandler
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    private readonly IServiceProvider _serviceProvider;

    private IRequestResponseExceptionHandler<TRequest, TResponse, TException>? _cachedHandler;

    public RequestResponseExceptionRequestHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public override Task Handle(IBaseRequest baseRequest, Exception exception, RequestResponseExceptionHandlerState state, CancellationToken cancellationToken)
    {
        var possibleHandler = GetHandler();

        return possibleHandler is null ?
            Task.CompletedTask :
            possibleHandler.Handle((TRequest) baseRequest, (TException) exception, (RequestResponseExceptionHandlerState<TResponse>)state, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestResponseExceptionHandler<TRequest, TResponse, TException>? GetHandler() =>
        _cachedHandler ??= _serviceProvider.GetService<IRequestResponseExceptionHandler<TRequest, TResponse, TException>>();
}