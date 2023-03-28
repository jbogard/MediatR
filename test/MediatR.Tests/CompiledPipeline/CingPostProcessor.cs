using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Tests.CompiledPipeline
{
    public class CingPostProcessor: IRequestPostProcessor<Cing, Cong>
    {
        public Task Process(Cing request, Cong response, CancellationToken cancellationToken)
        {
            response.Message += "in the rain!";
            return Task.CompletedTask;
        }
    }
}