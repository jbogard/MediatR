using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling.Request.Subscription;

internal sealed class CachedRequestExceptionActionHandler<TRequest, TException> : RequestExceptionActionHandler
    where TRequest : IRequest
    where TException : Exception
{
    private IRequestExceptionAction<TRequest, TException>[]? _cachedHandler;

    public override Task HandleAsync<TMethodRequest>(TMethodRequest request, Exception exception, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Debug.Assert(typeof(TRequest).IsAssignableFrom(typeof(TMethodRequest)), "request type must be an inherited type of method request type.");

        var handlers = (IRequestExceptionAction<TMethodRequest, TException>[])GetHandler(serviceProvider);
        var tasks = new Task[handlers.Length];

        for (var i = 0; i < handlers.Length; i++)
        {
            tasks[i] = handlers[i].Execute(request, (TException) exception, cancellationToken);
        }

        return Task.WhenAll(tasks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestExceptionAction<TRequest, TException>[] GetHandler(IServiceProvider serviceProvider) =>
        _cachedHandler ??= serviceProvider.GetServices<IRequestExceptionAction<TRequest, TException>>();
}