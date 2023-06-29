using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction.Handlers;

namespace MediatR.Examples;

public class SingHandler : IStreamRequestHandler<Sing, Song>
{
    private readonly TextWriter _writer;

    public SingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Sing: {request.Message}, Song");
        yield return new Song { Message = request.Message + "ing do" };
        yield return new Song { Message = request.Message + "ing re" };
        yield return new Song { Message = request.Message + "ing mi" };
        yield return new Song { Message = request.Message + "ing fa" };
        yield return new Song { Message = request.Message + "ing so" };
        yield return new Song { Message = request.Message + "ing la" };
        yield return new Song { Message = request.Message + "ing ti" };
        yield return new Song { Message = request.Message + "ing do" };
    }
}