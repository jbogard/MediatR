using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

namespace MediatR.ExecutionFlowTests.StreamRequest.Handlers;

internal sealed class BaseStreamRequestHandler : IStreamRequestHandler<StreamRequestMessage, BaseStreamResponse>
{
    public int Calls { get; set; }

    public async IAsyncEnumerable<BaseStreamResponse> Handle(StreamRequestMessage request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Calls++;
        await Task.Yield();
        yield return new StreamResponse();
    }
}