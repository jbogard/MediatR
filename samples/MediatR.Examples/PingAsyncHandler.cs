using System.Threading;

namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingAsyncHandler : IAsyncRequestHandler<PingAsync, Pong>
    {
        public async Task<Pong> Handle(PingAsync message, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => new Pong { Message = message.Message + " Pong" });
        }
    }
}