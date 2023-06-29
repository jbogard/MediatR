using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Pipeline;
using MediatR.Pipeline;

namespace MediatR.Benchmarks
{
    public class GenericRequestPreProcessor<TRequest, TResponse> : IRequestPreProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericRequestPreProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("- Starting Up");
        }
    }
}