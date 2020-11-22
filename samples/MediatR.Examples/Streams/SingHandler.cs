using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples
{
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
            yield return await Task.Run(() => new Song { Message = request.Message + "ing do re mi" });
            yield return await Task.Run(() => new Song { Message = request.Message + "ing fa so la" });
            yield return await Task.Run(() => new Song { Message = request.Message + "ing ti do" });
        }
    }
}
