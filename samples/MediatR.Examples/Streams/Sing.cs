namespace MediatR.Examples
{
    public class Sing : IRequest<Song>
    {
        public string Message { get; set; }
    }
}
