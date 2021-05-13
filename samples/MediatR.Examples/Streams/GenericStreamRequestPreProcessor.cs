using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline.Streams;

namespace MediatR.Examples
{
    public class GenericStreamRequestPreProcessor<TRequest> : IStreamRequestPreProcessor<TRequest>
    {
        private readonly TextWriter _writer;

        public GenericStreamRequestPreProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            await Task.Run(() => { _writer.WriteLineAsync("- Stream PreProcessing"); });
        }
    }
}