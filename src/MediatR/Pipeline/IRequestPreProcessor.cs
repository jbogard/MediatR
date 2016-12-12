namespace MediatR.Pipeline
{
    using System.Threading.Tasks;

    public interface IRequestPreProcessor<in TRequest>
    {
        Task Process(TRequest request);
    }
}