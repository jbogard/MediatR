namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingAsyncHandler : IAsyncRequestHandler<PingAsync, Pong>
    {
        public async Task<Pong> Handle(PingAsync message)
        {
            return await Task.Factory.StartNew(() => new Pong { Message = message.Message + " Pong" });
        }
    }
}