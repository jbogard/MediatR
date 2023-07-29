using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.Requests.Pipelines;

internal sealed class GenericPipelineHandler<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };
    public ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(request, cancellationToken);
    }
}