using System.IO;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly TextWriter _writer;

        public GenericPipelineBehavior(TextWriter writer)
        {
            _writer = writer;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
        {
            _writer.WriteLine("-- Handling Request");
            var response = next();
            _writer.WriteLine("-- Finished Request");
            return response;
        }
    }
}