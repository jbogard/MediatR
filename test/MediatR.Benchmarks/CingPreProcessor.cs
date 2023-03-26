using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Benchmarks
{
    public class CingPreProcessor: IRequestPreProcessor<Cing>
    {
        public Task Process(Cing request, CancellationToken cancellationToken)
        {
            request.Message = "I'm ";
            return Task.CompletedTask;
        }
    }
}