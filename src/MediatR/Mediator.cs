using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.Subscriptions;

namespace MediatR;

/// <summary>
/// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
/// </summary>
public sealed class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, NotificationHandler> _notificationHandlers = new();
    private static readonly ConcurrentDictionary<Type, RequestHandler> _requestHandlers = new();
    private static readonly ConcurrentDictionary<Type, RequestResponseHandler> _requestResponseHandlers = new();
    private static readonly ConcurrentDictionary<Type, StreamRequestHandler> _streamRequestHandlers = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Type, NotificationHandler> _notificationFactory;
    private readonly Func<Type, RequestHandler> _requestFactory;
    private readonly Func<Type, Type, RequestResponseHandler> _requestResponseFactory;
    private readonly Func<Type, Type, StreamRequestHandler> _streamRequestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var factory = serviceProvider.GetRequiredService<SubscriptionFactory>();
        _notificationFactory = factory.CreateNotificationHandler;
        _requestFactory = factory.CreateRequestHandler;
        _requestResponseFactory = factory.CreateRequestResponseHandler;
        _streamRequestFactory = factory.CreateStreamRequestHandler;
    }

    public ValueTask<TResponse> Send<TRequest, TResponse>(TRequest? request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        if (request is null)
        {
            return ThrowArgumentNullRef<ValueTask<TResponse>>(nameof(request));
        }

        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        RequestHandlerDelegate<TRequest, TResponse> handler = Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler(request, cancellationToken);

        ValueTask<TResponse> Handle(TRequest handlerRequest, CancellationToken token)
        {
            return _requestResponseHandlers.GetOrAdd(typeof(TRequest), requestType => _requestResponseFactory(requestType, typeof(TResponse)))
                .HandleAsync<TRequest, TResponse>(handlerRequest, token);
        }
    }

    public ValueTask Send<TRequest>(TRequest? request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        if (request is null)
        {
            return ThrowArgumentNullRef<ValueTask>(nameof(request));
        }

        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest>>();
        RequestHandlerDelegate<TRequest> handler = Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler(request, cancellationToken);

        ValueTask Handle(TRequest handlerRequest, CancellationToken token)
        {
            return _requestHandlers.GetOrAdd(typeof(TRequest), _requestFactory)
                .HandleAsync(handlerRequest, token);
        }
    }

    public void Publish<TNotification>(TNotification? notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is null)
        {
            ThrowArgumentNullRef<int>(nameof(notification));
            return;
        }

        _notificationHandlers.GetOrAdd(typeof(TNotification), _notificationFactory)
            .Handle(notification, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest? request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        if (request is null)
        {
            return ThrowArgumentNullRef<IAsyncEnumerable<TResponse>>(nameof(request));
        }

        var behaviors = _serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();
        StreamHandlerNext<TRequest, TResponse> handler = Handle;
        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var next = handler;
            var behavior = behaviors[i];
            handler = (behaviorRequest, token) => behavior.Handle(behaviorRequest, next, token);
        }

        return handler(request, cancellationToken);

        IAsyncEnumerable<TResponse> Handle(TRequest handlerRequest, CancellationToken token)
        {
            return _streamRequestHandlers.GetOrAdd(typeof(TRequest), (requestType) => _streamRequestFactory(requestType, typeof(TResponse)))
                .Handle<TRequest, TResponse>(handlerRequest, token);
        }
    }

    // The exception throwing was moved to this function to reduce the JIT output per function that would throw an exception.
    // This reduces the JIT output which then reduces the instruction that needs to be loaded which makes the function faster.
    // And because throwing an exception is really slow one more method call or less doesn't make much of a difference.
    private static T ThrowArgumentNullRef<T>(string message) => throw new ArgumentNullException(message);
}
