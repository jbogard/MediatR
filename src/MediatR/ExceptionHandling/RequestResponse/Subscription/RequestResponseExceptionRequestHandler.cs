using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling.RequestResponse.Subscription;

internal abstract class RequestResponseExceptionRequestHandler
{
    public abstract Task Handle<TMethodRequest, TMethodResponse>(
        TMethodRequest baseRequest,
        Exception exception,
        RequestResponseExceptionHandlerState<TMethodResponse> state,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    where TMethodRequest : IRequest<TMethodResponse>
    where TMethodResponse : notnull;
}