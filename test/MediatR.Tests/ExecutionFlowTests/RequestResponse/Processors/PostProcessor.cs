using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Processors;

namespace MediatR.ExecutionFlowTests.RequestResponse.Processors;

internal sealed class PostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : RequestResponse, IRequest<TResponse>
{
    public int Calls { get; set; }
    public Action InvocationCheck { get; set; } = () => { };
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        InvocationCheck();
        Calls++;
        return Task.CompletedTask;
    }
}