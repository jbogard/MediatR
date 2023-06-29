using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling;

public abstract class RequestExceptionRequestHandler
{
    public abstract Task HandleAsync(IBaseRequest request, Exception exception, RequestExceptionHandlerState state, CancellationToken cancellationToken);
}