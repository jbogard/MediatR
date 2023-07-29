using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Handlers;

internal sealed class RequestHandler :
    IRequestHandler<Request>,
    IRequestHandler<ThrowingExceptionRequest>,
    IRequestHandler<AccessViolationRequest>
{
    public int Calls { get; private set; }

    public ValueTask Handle(Request request, CancellationToken cancellationToken)
    {
        Calls++;

        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(ThrowingExceptionRequest request, CancellationToken cancellationToken) => throw request.Exception;

    public ValueTask Handle(AccessViolationRequest request, CancellationToken cancellationToken) => throw request.AccessViolationException;
}