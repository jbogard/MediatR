using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling;

public abstract class RequestExceptionActionHandler
{
    public abstract Task HandleAsync(IRequest request, Exception exception, CancellationToken cancellationToken);
}