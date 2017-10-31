using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Examples
{
    public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly TextWriter _writer;

        public GenericRequestPreProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Process(TRequest request, IMediatorContext context)
        {
            _writer.WriteLine("- Starting Up");
            return Task.FromResult(0);
        }
    }
}