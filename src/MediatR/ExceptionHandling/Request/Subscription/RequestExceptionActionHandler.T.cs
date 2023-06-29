using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling;

public sealed class RequestExceptionActionHandler<TRequest, TException> : RequestExceptionActionHandler
    where TRequest : IRequest
    where TException : Exception
{
    private readonly IServiceProvider _serviceProvider;
    
    private IRequestExceptionAction<TRequest, TException>[]? _cachedHandler;

    public RequestExceptionActionHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


    public override Task HandleAsync(IRequest request, Exception exception, CancellationToken cancellationToken)
    {
        var handlers = GetHandler();
        if (handlers.Length < 1)
        {
            return Task.CompletedTask;
        }

        Parallel.ForEach(
            handlers,
            (action, _) => action.Execute((TRequest) request, (TException) exception, cancellationToken).Wait(cancellationToken));

        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestExceptionAction<TRequest, TException>[] GetHandler() =>
        _cachedHandler ??= _serviceProvider.GetServices<IRequestExceptionAction<TRequest, TException>>();
}