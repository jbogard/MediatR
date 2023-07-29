using System.Collections.Generic;
using System.Threading;
using MediatR.Abstraction.Behaviors;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

namespace MediatR.ExecutionFlowTests.StreamRequest.Pipelines;

internal sealed class RestrictedGenericPipelineHandler<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : StreamRequestBase, IStreamRequest<TResponse>
    where TResponse : notnull
{
    public int Calls { get; set; }

    public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        Calls++;
        return next(request, cancellationToken);
    }
}