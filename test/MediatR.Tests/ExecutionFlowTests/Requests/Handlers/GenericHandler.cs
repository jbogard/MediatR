using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Handlers;

internal sealed class GenericHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : GenericHandableRequest, IRequest
{
    public ValueTask Handle(TRequest request, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;
}