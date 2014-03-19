namespace MediatR.Examples
{
    public class PingPongHandler : IPostRequestHandler<Ping, Pong>
    {
        public void Handle(Ping request, Pong response)
        {
            response.Message = response.Message + " Pung";
        }
    }
}