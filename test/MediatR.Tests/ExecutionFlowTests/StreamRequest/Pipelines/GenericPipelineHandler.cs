using System;
using System.Collections.Generic;
using System.Threading;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExecutionFlowTests.StreamRequest.Pipelines;

internal sealed class GenericPipelineHandler<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : notnull
{
    public int Calls { get; set; }
    public Action InvocationValidation { get; set; } = () => { };

    public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        InvocationValidation();
        Calls++;
        return next(request, cancellationToken);
    }
}