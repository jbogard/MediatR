using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

namespace MediatR.ExecutionFlowTests.StreamRequest.Handlers;

internal sealed class StreamRequestHandler :
    IStreamRequestHandler<StreamRequestMessage, StreamResponse>,
    IStreamRequestHandler<RootStreamRequestMessage, StreamResponse>
{
    public int Calls { get; private set; }

    public async IAsyncEnumerable<StreamResponse> Handle(StreamRequestMessage requestMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Calls++;

        await Task.Yield();
        yield return new StreamResponse();
    }

    public async IAsyncEnumerable<StreamResponse> Handle(RootStreamRequestMessage request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Calls++;
        await Task.Yield();
        yield return request.StreamResponse;
    }
}