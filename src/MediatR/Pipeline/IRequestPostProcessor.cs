namespace MediatR.Pipeline
{
    using System.Threading.Tasks;

    public interface IRequestPostProcessor<in TRequest, in TResponse>
    {
        Task Process(TRequest request, TResponse response);
    }
}