using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Subscriptions;

internal abstract class RequestHandler
{
    public abstract ValueTask HandleAsync(IRequest request, CancellationToken cancellationToken);
}