namespace MediatR.Examples.Streams;

public class Sing : IStreamRequest<Song>
{
    public required string Message { get; set; }
}