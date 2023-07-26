using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.Examples.Streams;

public class GenericStreamPipelineBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly TextWriter _writer;

    public GenericStreamPipelineBehavior(TextWriter writer)
    {
        _writer = writer;
    }

    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerNext<TRequest, TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("-- Handling StreamRequest");
        await foreach (var response in next(request, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return response;
        }
        await _writer.WriteLineAsync("-- Finished StreamRequest");
    }
}