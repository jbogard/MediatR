using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Benchmarks.MockServices.StreamRequest;

internal sealed class StreamHandler : IStreamRequestHandler<SingStream, Note>
{
    private static readonly IAsyncEnumerable<Note> Response = GetResponse();
    public IAsyncEnumerable<Note> Handle(SingStream request, CancellationToken cancellationToken) => Response;

    private static async IAsyncEnumerable<Note> GetResponse()
    {
        await Task.CompletedTask;
        yield return new Note();
    }
}