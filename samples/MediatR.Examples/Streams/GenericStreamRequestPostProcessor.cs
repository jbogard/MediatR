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

        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("- All Streaming Done");
        }
    }
}