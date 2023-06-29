using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling;

public abstract class RequestResponseExceptionRequestHandler
{
    public abstract Task Handle(IBaseRequest baseRequest, Exception exception, RequestResponseExceptionHandlerState state, CancellationToken cancellationToken);
}