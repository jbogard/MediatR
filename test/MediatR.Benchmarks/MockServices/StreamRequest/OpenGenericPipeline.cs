using System.Collections.Generic;
using System.Threading;
using MediatR.Abstraction.Behaviors;

namespace MediatR.Benchmarks.MockServices.StreamRequest;

internal sealed class OpenGenericPipeline<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerNext<TRequest, TResponse> next, CancellationToken cancellationToken) =>
        next(request, cancellationToken);
}