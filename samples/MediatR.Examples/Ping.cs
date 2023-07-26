namespace MediatR.Examples;

public class Ping : IRequest<Pong>
{
    public required string Message { get; set; }
}