namespace MediatR.Examples
{
    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Pong Handle(Ping message)
        {
            return new Pong {Message = message.Message + " Pong"};
        }
    }
}