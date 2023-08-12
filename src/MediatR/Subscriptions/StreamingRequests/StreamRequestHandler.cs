using System;
using System.Collections.Generic;
using System.Threading;

namespace MediatR.Subscriptions.StreamingRequests;

internal abstract class StreamRequestHandler
{
    public abstract IAsyncEnumerable<TMethodResponse> HandleAsync<TMethodResponse>(IStreamRequest<TMethodResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    public abstract IAsyncEnumerable<object?> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}