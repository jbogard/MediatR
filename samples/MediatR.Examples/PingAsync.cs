namespace MediatR.Examples
{
    public class PingAsync : IAsyncRequest<Pong>
    {
        public string Message { get; set; }
    }
}