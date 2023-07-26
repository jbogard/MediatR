using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class BaseRequestResponseHandler : IRequestHandler<RequestResponse, BaseResponse>
{
    public int Calls { get; set; }

    public Task<BaseResponse> Handle(RequestResponse request, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult<BaseResponse>(new Response());
    }
}