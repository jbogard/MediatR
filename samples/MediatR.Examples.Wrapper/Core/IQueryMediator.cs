using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.Wrapper.Core
{
    public interface IQueryMediator
    {
        Task<TResponse> Handle<TQuery, TResponse>(TQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
            where TQuery : IQuery<TResponse>;
    }
}