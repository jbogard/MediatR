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

    public Task<Response> Handle(RequestResponse request, CancellationToken cancellationToken)
    {
        Calls++;

        if (request is RootRequestResponse rootRequestResponse)
        {
            return Task.FromResult(rootRequestResponse.Response);
        }
        
        return Task.FromResult(new Response());
    }

    public Task<Response> Handle(ThrowingExceptionRequest request, CancellationToken cancellationToken) => throw request.Exception;

    public Task<Response> Handle(AccessViolationRequest request, CancellationToken cancellationToken) => throw request.AccessViolationException;
}