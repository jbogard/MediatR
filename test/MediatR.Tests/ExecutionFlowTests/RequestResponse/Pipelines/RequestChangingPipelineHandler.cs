using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.RequestResponse.Pipelines;

internal sealed class RequestChangingPipelineHandler : IPipelineBehavior<RequestResponse, Response>
{
    public int Calls { get; set; }
    
    public ValueTask<Response> Handle(RequestResponse request, RequestHandlerDelegate<RequestResponse, Response> next, CancellationToken cancellationToken)
    {
        Calls++;
        
        if (request.ShouldChangeRequest)
        {
            return next(new RootRequestResponse
                {
                    Response = new RootResponse()
                },
                cancellationToken);
        }

        return next(request, cancellationToken);
    }
}