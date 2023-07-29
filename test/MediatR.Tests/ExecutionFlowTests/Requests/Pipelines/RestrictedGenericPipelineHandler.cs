using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Pipelines;

internal sealed class RestrictedGenericPipelineHandler<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : ThrowingExceptionRequest, IRequest
{
    public int Calls { get; set; }

    public ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        Calls++;
        return next(request, cancellationToken);
    }
}