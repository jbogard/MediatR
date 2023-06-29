using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling;

internal sealed class RequestResponseExceptionActionHandler<TRequest, TResponse, TException> : RequestResponseExceptionActionHandler
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
    where TException : Exception
{
    private readonly IServiceProvider _serviceProvider;

    private IRequestResponseExceptionAction<TRequest, TResponse, TException>[]? _cachedHandler;

    public RequestResponseExceptionActionHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public override Task Handle(IBaseRequest request, Exception exception, CancellationToken cancellationToken)
    {
        var handlers = GetHandler();
        if (handlers.Length < 1)
        {
            return Task.CompletedTask;
        }

        Parallel.ForEach(
            handlers,
            (action, _) => action.Execute((TRequest) request, (TException) exception, cancellationToken));
        
        return Task.CompletedTask;
    }

    private IRequestResponseExceptionAction<TRequest, TResponse, TException>[] GetHandler() =>
        _cachedHandler ??= _serviceProvider.GetServices<IRequestResponseExceptionAction<TRequest, TResponse, TException>>();
}