namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingAsyncHandler : IAsyncRequestHandler<PingAsync, Pong>
    {
        public Task<Pong> Handle(PingAsync message)
        {
            return Task.Factory.StartNew(() => new Pong { Message = message.Message + " Pong" });
        }
    }
}