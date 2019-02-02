using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.Wrapper.Core
{
    public abstract class QueryHandler<TQuery, TResponse> : IRequestHandler<QueryWrapper<TQuery, TResponse>, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public abstract Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);

        public Task<TResponse> Handle(QueryWrapper<TQuery, TResponse> request, CancellationToken cancellationToken)
        {
            return Handle(request.Query, cancellationToken);
        }
    }
}