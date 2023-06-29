using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR.Abstraction.Handlers;

namespace MediatR.Subscriptions;

internal sealed class StreamRequestHandler<TRequest, TResponse> : StreamRequestHandler
    where TRequest : IStreamRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    private IStreamRequestHandler<TRequest, TResponse>? _cachedHandler;

    public StreamRequestHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public override IAsyncEnumerable<TMethodResponse> Handle<TMethodRequest, TMethodResponse>(TMethodRequest request, CancellationToken cancellationToken) =>
        ((IStreamRequestHandler<TMethodRequest, TMethodResponse>) GetHandler()).Handle(request, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IStreamRequestHandler<TRequest, TResponse> GetHandler() =>
        _cachedHandler ??= _serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
}