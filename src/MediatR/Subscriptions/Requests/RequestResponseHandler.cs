using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Subscriptions;

internal abstract class RequestResponseHandler
{
    public abstract ValueTask<TMethodResponse> HandleAsync<TMethodRequest, TMethodResponse>(TMethodRequest request, CancellationToken cancellationToken)
        where TMethodRequest : IRequest<TMethodResponse>;
}