using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling.RequestResponse.Subscription;

internal abstract class RequestResponseExceptionActionHandler
{
    public abstract Task Handle<TMethodRequest, TMethodResponse>(TMethodRequest request, Exception exception, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TMethodRequest : IRequest<TMethodResponse>
        where TMethodResponse : notnull;
}