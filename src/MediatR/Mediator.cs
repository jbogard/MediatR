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
    private static readonly ConcurrentDictionary<(Type Request, Type Response), RequestResponseHandler> _requestResponseHandlers = new();
    private static readonly ConcurrentDictionary<(Type Request, Type Response), StreamRequestHandler> _streamRequestHandlers = new();

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

    public ValueTask<TResponse> SendAsync<TResponse>(IRequest<TResponse>? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _requestResponseHandlers.GetOrAdd((request!.GetType(), typeof(TResponse)), SubscriptionFactory.CreateRequestResponseHandler)
            .HandleAsync(request, _serviceProvider, cancellationToken);
    }

    public ValueTask SendAsync<TRequest>(TRequest? request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _requestHandlers.GetOrAdd(typeof(TRequest), SubscriptionFactory.CreateRequestHandler)
            .HandleAsync(request!, _serviceProvider, cancellationToken);
    }

    public void Publish<TNotification>(TNotification? notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is null)
        {
            ThrowArgumentNull(nameof(notification));
        }

        var notificationHandler = _notificationHandlers.GetOrAdd(typeof(TNotification), SubscriptionFactory.CreateNotificationHandler);
        _notificationPublisher.Publish(notificationHandler, notification!, _serviceProvider, _notificationPublisher, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStreamAsync<TResponse>(IStreamRequest<TResponse>? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            ThrowArgumentNull(nameof(request));
        }

        return _streamRequestHandlers.GetOrAdd((request!.GetType(), typeof(TResponse)), SubscriptionFactory.CreateStreamRequestHandler)
            .HandleAsync(request, _serviceProvider, cancellationToken);
    }

    private static void ThrowArgumentNull(string paramName) => throw new ArgumentNullException(paramName);
}
