using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Pipelines;

internal sealed class RequestChangingPipelineHandler : IPipelineBehavior<Request>
{
    public int Calls { get; set; }
    
    public ValueTask Handle(Request request, RequestHandlerDelegate<Request> next, CancellationToken cancellationToken)
    {
        Calls++;
        
        if (request.ShouldChangeRequest)
        {
            return next(new RootRequest(), cancellationToken);
        }

        return next(request, cancellationToken);
    }
}