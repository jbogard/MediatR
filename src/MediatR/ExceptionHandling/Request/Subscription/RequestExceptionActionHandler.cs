using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling.Request.Subscription;

internal abstract class RequestExceptionActionHandler
{
    public abstract Task HandleAsync<TMethodRequest>(TMethodRequest request, Exception exception, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TMethodRequest : IRequest;
}