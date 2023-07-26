using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Subscriptions.Requests;

internal abstract class RequestResponseHandler
{
    public abstract ValueTask<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}