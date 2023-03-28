namespace MediatR.Tests.CompiledPipeline
{
    public class Cing : IRequest<Cong>
    {
        public string? Message { get; set; }
    }
}