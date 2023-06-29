using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions;

internal sealed class RequestResponseHandler<TRequest, TResponse> : RequestResponseHandler
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    private IRequestHandler<TRequest, TResponse>? _cachedHandler;

    public RequestResponseHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public override ValueTask<TMethodResponse> HandleAsync<TMethodRequest, TMethodResponse>(TMethodRequest request, CancellationToken cancellationToken) =>
        ((IRequestHandler<TMethodRequest, TMethodResponse>) GetHandler()).Handle(request, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IRequestHandler<TRequest, TResponse> GetHandler() => 
        _cachedHandler ??= _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
}