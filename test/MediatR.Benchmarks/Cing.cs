namespace MediatR.Benchmarks
{
    public class Cing : IRequest<Cong>
    {
        public string Message { get; set; }
    }
}