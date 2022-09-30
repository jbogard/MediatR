namespace MediatR;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wrappers;

/// <summary>
/// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
/// </summary>
public class Mediator : IMediator
{
    private readonly ServiceFactory _serviceFactory;
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> _requestHandlers = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _notificationHandlers = new();
    private static readonly ConcurrentDictionary<Type, StreamRequestHandlerBase> _streamRequestHandlers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceFactory">The single instance factory.</param>
    public Mediator(ServiceFactory serviceFactory) 
        => _serviceFactory = serviceFactory;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(requestType,
            static t => (RequestHandlerBase)(Activator.CreateInstance(typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(t, typeof(TResponse)))
                                             ?? throw new InvalidOperationException($"Could not create wrapper type for {t}")));

        return handler.Handle(request, _serviceFactory, cancellationToken);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        var requestType = request.GetType();
        var handler = _requestHandlers.GetOrAdd(requestType,
            static requestTypeKey =>
            {
                var requestInterfaceType = requestTypeKey
                    .GetInterfaces()
                    .FirstOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

                if (requestInterfaceType is null)
                {
                    throw new ArgumentException($"{requestTypeKey.Name} does not implement {nameof(IRequest)}", nameof(request));
                }

                var responseType = requestInterfaceType.GetGenericArguments()[0];
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestTypeKey, responseType);

                return (RequestHandlerBase)(Activator.CreateInstance(wrapperType) 
                                            ?? throw new InvalidOperationException($"Could not create wrapper for type {wrapperType}"));
            });

        // call via dynamic dispatch to avoid calling through reflection for performance reasons
        return handler.Handle(request, _serviceFactory, cancellationToken);
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification == null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        return PublishNotification(notification, cancellationToken);
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default) =>
        notification switch
        {
            null => throw new ArgumentNullException(nameof(notification)),
            INotification instance => PublishNotification(instance, cancellationToken),
            _ => throw new ArgumentException($"{nameof(notification)} does not implement ${nameof(INotification)}")
        };

    /// <summary>
    /// Override in a derived class to control how the tasks are awaited. By default the implementation is a foreach and await of each handler
    /// </summary>
    /// <param name="allHandlers">Enumerable of tasks representing invoking each notification handler</param>
    /// <param name="notification">The notification being published</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing invoking all handlers</returns>
    protected virtual async Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in allHandlers)
        {
            await handler(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task PublishNotification(INotification notification, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        var handler = _notificationHandlers.GetOrAdd(notificationType,
            static t => (NotificationHandlerWrapper) (Activator.CreateInstance(typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(t)) 
                                                      ?? throw new InvalidOperationException($"Could not create wrapper for type {t}")));

        return handler.Handle(notification, _serviceFactory, PublishCore, cancellationToken);
    }


    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        var streamHandler = (StreamRequestHandlerWrapper<TResponse>) _streamRequestHandlers.GetOrAdd(requestType,
            t => (StreamRequestHandlerBase) Activator.CreateInstance(typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse))));

        var items = streamHandler.Handle(request, _serviceFactory, cancellationToken);

        return items;
    }


    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        var handler = _streamRequestHandlers.GetOrAdd(requestType,
            requestTypeKey =>
            {
                var requestInterfaceType = requestTypeKey
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>));
                var isValidRequest = requestInterfaceType != null;

                if (!isValidRequest)
                {
                    throw new ArgumentException($"{requestType.Name} does not implement IStreamRequest<TResponse>", nameof(requestTypeKey));
                }

                var responseType = requestInterfaceType!.GetGenericArguments()[0];
                return (StreamRequestHandlerBase) Activator.CreateInstance(typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestTypeKey, responseType));
            });

        // call via dynamic dispatch to avoid calling through reflection for performance reasons
        var items = handler.Handle(request, _serviceFactory, cancellationToken);

        return items;
    }
}