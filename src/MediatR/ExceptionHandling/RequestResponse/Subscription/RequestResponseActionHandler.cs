using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ExceptionHandling;

public abstract class RequestResponseExceptionActionHandler
{
    public abstract Task Handle(IBaseRequest request, Exception exception, CancellationToken cancellationToken);
}