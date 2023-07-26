using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Subscriptions.Requests;

internal abstract class RequestResponseHandler
{
    public abstract Task<TMethodResponse> HandleAsync<TMethodResponse>(IRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}