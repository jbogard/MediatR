using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Processors;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;

namespace MediatR.ExecutionFlowTests.Requests.Processors;

internal sealed class PreRegisteredPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : Request, IRequest
{
    public int Calls { get; set; }
    public Action InvocationCheck { get; set; } = () => { };

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        InvocationCheck();
        Calls++;
        return Task.CompletedTask;
    }
}