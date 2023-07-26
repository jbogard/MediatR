using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.Benchmarks.MockServices.Request;

internal sealed class OpenGenericPipeline<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken) =>
        next(request, cancellationToken);
}