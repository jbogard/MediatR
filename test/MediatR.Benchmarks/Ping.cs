namespace MediatR.Benchmarks
{
    public class Ping : IRequest
    {
        public string Message { get; set; }
    }
}