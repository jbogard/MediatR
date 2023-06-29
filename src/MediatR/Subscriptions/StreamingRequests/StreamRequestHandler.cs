using System.Collections.Generic;
using System.Threading;

namespace MediatR.Subscriptions;

internal abstract class StreamRequestHandler
{
    public abstract IAsyncEnumerable<TMethodResponse> Handle<TMethodRequest, TMethodResponse>(TMethodRequest request, CancellationToken cancellationToken)
        where TMethodRequest : IStreamRequest<TMethodResponse>;
}