using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.RequestResponse.Pipelines;

internal sealed class RestrictedGenericPipelineHandler<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ThrowingExceptionRequest, IRequest<TResponse>
{
    public int Calls { get; set; }

    public ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        Calls++;
        return next(request, cancellationToken);
    }
}