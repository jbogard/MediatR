using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline.Streams;

namespace MediatR.Examples
{
    public class GenericStreamRequestPostProcessor<TRequest, TResponse> : IStreamRequestPostProcessor<TRequest, TResponse>
    {
        private readonly TextWriter _writer;

        public GenericStreamRequestPostProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            await Task.Run(() => _writer.WriteLineAsync("- Stream PostProcessing"));
        }
    }
}