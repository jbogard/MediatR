using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class RequestResponseHandler :
    IRequestHandler<RequestResponse, Response>,
    IRequestHandler<ThrowingExceptionRequest, Response>,
    IRequestHandler<AccessViolationRequest, Response>
{
    public int Calls { get; private set; }

    public ValueTask<Response> Handle(RequestResponse request, CancellationToken cancellationToken)
    {
        Calls++;

        if (request is RootRequestResponse rootRequestResponse)
        {
            return ValueTask.FromResult(rootRequestResponse.Response);
        }
        
        return ValueTask.FromResult(new Response());
    }

    public ValueTask<Response> Handle(ThrowingExceptionRequest request, CancellationToken cancellationToken) => throw request.Exception;

    public ValueTask<Response> Handle(AccessViolationRequest request, CancellationToken cancellationToken) => throw request.AccessViolationException;
}