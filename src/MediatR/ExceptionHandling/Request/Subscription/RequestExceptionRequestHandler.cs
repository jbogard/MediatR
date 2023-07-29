using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.ExceptionHandling.Request.Subscription;

internal abstract class RequestExceptionRequestHandler
{
    public abstract Task HandleAsync<TMethodRequest>(TMethodRequest request, Exception exception, RequestExceptionHandlerState state, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TMethodRequest : IRequest;
}