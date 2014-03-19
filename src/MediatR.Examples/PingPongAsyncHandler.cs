namespace MediatR.Examples
{
    using System.Threading.Tasks;

    public class PingPongAsyncHandler : IAsyncPostRequestHandler<PingAsync, Pong>
    {
        public async Task Handle(PingAsync request, Pong response)
        {
            await Task.Factory.StartNew(() => response.Message = response.Message + " Pung");
        }
    }
}