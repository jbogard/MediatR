using System;
using System.Collections.Generic;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

namespace MediatR.ExecutionFlowTests.StreamRequest.Pipelines;

internal sealed class SpecificPipelineHandler : IStreamPipelineBehavior<RootStreamRequestMessage, StreamResponse>
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };
    public IAsyncEnumerable<StreamResponse> Handle(RootStreamRequestMessage requestMessage, StreamHandlerDelegate<RootStreamRequestMessage, StreamResponse> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(requestMessage, cancellationToken);
    }
}