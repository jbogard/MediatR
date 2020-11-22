using System.IO;
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

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("- Stream Starting Up");
        }
    }
}