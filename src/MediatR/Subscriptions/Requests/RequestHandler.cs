using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Subscriptions.Requests;

internal abstract class RequestHandler
{
    public abstract ValueTask HandleAsync<TMethodRequest>(TMethodRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TMethodRequest : IRequest;

    public abstract ValueTask HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}