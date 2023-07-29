using System.Collections.Generic;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

namespace MediatR.ExecutionFlowTests.StreamRequest.Pipelines;

internal sealed class RequestChangingPipelineHandler : IStreamPipelineBehavior<StreamRequestMessage, StreamResponse>
{
    public int Calls { get; set; }
    
    public IAsyncEnumerable<StreamResponse> Handle(StreamRequestMessage requestMessage, StreamHandlerDelegate<StreamRequestMessage, StreamResponse> next, CancellationToken cancellationToken)
    {
        Calls++;
        
        if (requestMessage.ShouldChangeRequest)
        {
            return next(new RootStreamRequestMessage
            {
                StreamResponse = new RootStreamResponse()
            }, cancellationToken);
        }

        return next(requestMessage, cancellationToken);
    }
}