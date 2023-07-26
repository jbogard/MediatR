using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class BaseRequestResponseHandler : IRequestHandler<RequestResponse, BaseResponse>
{
    public int Calls { get; set; }

    public ValueTask<BaseResponse> Handle(RequestResponse request, CancellationToken cancellationToken)
    {
        Calls++;
        return ValueTask.FromResult<BaseResponse>(new Response());
    }
}