namespace MediatR.Examples;

public class Jing : IRequest
{
    public required string Message { get; set; }
}