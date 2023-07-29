using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Handlers;

internal sealed class BaseRequestHandler : IRequestHandler<RequestBase>
{
    public int Calls { get; set; }

    public ValueTask Handle(RequestBase request, CancellationToken cancellationToken)
    {
        Calls++;
        return ValueTask.CompletedTask;
    }
}