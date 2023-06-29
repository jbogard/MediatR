using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions;

internal sealed class RequestHandler<TRequest> : RequestHandler
    where TRequest : IRequest
{
    private readonly IServiceProvider _serviceProvider;

    private IRequestHandler<TRequest>? _cachedHandler;

    public RequestHandler(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;
    
    public override ValueTask HandleAsync(IRequest request, CancellationToken cancellationToken) =>
        GetHandler().Handle((TRequest)request, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestHandler<TRequest> GetHandler()
    {
        return _cachedHandler ??= _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
    }
}