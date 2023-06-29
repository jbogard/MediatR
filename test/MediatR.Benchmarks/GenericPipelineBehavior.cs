using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.Benchmarks
{
    public class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<>
        where TRequest : notnull
    {
        private readonly TextWriter _writer;

        public GenericPipelineBehavior(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync("-- Handling Request");
            var response = await next();
            await _writer.WriteLineAsync("-- Finished Request");
            return response;
        }
    }
}