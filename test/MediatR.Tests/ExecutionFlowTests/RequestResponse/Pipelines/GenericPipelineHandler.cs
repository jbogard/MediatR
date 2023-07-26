using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.RequestResponse.Pipelines;

internal sealed class GenericPipelineHandler<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(request, cancellationToken);
    }
}