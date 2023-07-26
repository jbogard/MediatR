using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Subscriptions;
using MediatR.Subscriptions.Notifications;
using MediatR.Subscriptions.Requests;
using MediatR.Subscriptions.StreamingRequests;

namespace MediatR;

/// <summary>
/// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
/// </summary>
internal sealed class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, NotificationHandler> _notificationHandlers = new();
    private static readonly ConcurrentDictionary<Type, RequestHandler> _requestHandlers = new();
    private static readonly ConcurrentDictionary<Type, RequestResponseHandler> _requestResponseHandlers = new();
    private static readonly ConcurrentDictionary<Type, StreamRequestHandler> _streamRequestHandlers = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationPublisher _notificationPublisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
    /// <param name="notificationPublisher">Notification publisher.</param>
    public Mediator(IServiceProvider serviceProvider, INotificationPublisher notificationPublisher)
    {
        _serviceProvider = serviceProvider;
        _notificationPublisher = notificationPublisher;
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse>? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _requestResponseHandlers.GetOrAdd(request!.GetType(),static req => SubscriptionFactory.CreateRequestResponseHandler(req, typeof(TResponse)))
            .HandleAsync(request, _serviceProvider, cancellationToken);
    }

    public Task SendAsync<TRequest>(TRequest? request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _requestHandlers.GetOrAdd(request!.GetType(), SubscriptionFactory.CreateRequestHandler)
            .HandleAsync(request, _serviceProvider, cancellationToken);
    }

    public void Publish<TNotification>(TNotification? notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is null)
        {
            ThrowArgumentNull(nameof(notification));
        }

        _notificationHandlers.GetOrAdd(notification!.GetType(), SubscriptionFactory.CreateNotificationHandler)
            .Handle(notification, _serviceProvider, _notificationPublisher, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse>? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _streamRequestHandlers.GetOrAdd(request!.GetType(), static req => SubscriptionFactory.CreateStreamRequestHandler(req, typeof(TResponse)))
            .Handle(request, _serviceProvider, cancellationToken);
    }

    private static void ThrowArgumentNull(string message) => throw new ArgumentNullException(message);
}
