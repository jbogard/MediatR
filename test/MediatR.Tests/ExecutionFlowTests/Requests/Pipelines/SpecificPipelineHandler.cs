using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Pipelines;

internal sealed class SpecificPipelineHandler : IPipelineBehavior<Request>
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };
    public ValueTask Handle(Request request, RequestHandlerDelegate<Request> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(request, cancellationToken);
    }
}