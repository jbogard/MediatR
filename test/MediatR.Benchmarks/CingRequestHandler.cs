using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Benchmarks
{
    public class CingRequestHandler : IRequestHandler<Cing, Cong>
    {
        public Task<Cong> Handle(Cing request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Cong { Message = $"{request.Message} cining " });
        }
    }
}