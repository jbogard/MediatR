namespace MediatR.Examples
{
    public class PingAsync : IRequest<Pong>
    {
        public string Message { get; set; }
    }
}