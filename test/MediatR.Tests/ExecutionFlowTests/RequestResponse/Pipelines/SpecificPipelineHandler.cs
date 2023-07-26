using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.RequestResponse.Pipelines;

internal sealed class SpecificPipelineHandler : IPipelineBehavior<RequestResponse, Response>
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };
    public ValueTask<Response> Handle(RequestResponse request, RequestHandlerDelegate<RequestResponse, Response> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(request, cancellationToken);
    }
}