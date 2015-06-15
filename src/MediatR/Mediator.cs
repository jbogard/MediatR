namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a mediator to encapsulate request/response and publishing interaction patterns
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Send a request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <returns>Response</returns>
        TResponse Send<TResponse>(IRequest<TResponse> request);

        /// <summary>
        /// Asynchronously send a request to a single handler 
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        void Publish(INotification notification);

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(IAsyncNotification notification, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Factory method for creating single instances. Used to build instances of <see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IAsyncRequestHandler{TRequest,TResponse}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An instance of type <paramref name="serviceType" /></returns>
    public delegate object SingleInstanceFactory(Type serviceType);

    /// <summary>
    /// Factory method for creating multiple instances. Used to build instances of <see cref="INotificationHandler{TNotification}"/> and <see cref="IAsyncNotificationHandler{TNotification}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An enumerable of instances of type <paramref name="serviceType" /></returns>
    public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);

    /// <summary>
    /// Default mediator implementation relying on Common Service Locator for resolving handlers
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly SingleInstanceFactory _singleInstanceFactory;
        private readonly MultiInstanceFactory _multiInstanceFactory;

        public Mediator(SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
            _multiInstanceFactory = multiInstanceFactory;
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            TResponse result = defaultHandler.Handle(request);

            return result;
        }

        public async Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var defaultHandler = GetHandler(request);

            TResponse result = await defaultHandler.Handle(request, cancellationToken);

            return result;
        }

        public void Publish(INotification notification)
        {
            var notificationHandlers = GetNotificationHandlers(notification);

            foreach (var handler in notificationHandlers)
            {
                handler.Handle(notification);
            }
        }

        public async Task PublishAsync(IAsyncNotification notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var notificationHandlers = GetAsyncNotificationHandlers(notification);

            foreach (var handler in notificationHandlers)
            {
                await handler.Handle(notification, cancellationToken);
            }
        }

        private static InvalidOperationException BuildException(object message, Exception inner = null)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", inner);
        }

        private RequestHandler<TResponse> GetHandler<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(RequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(request);
            }
            catch (Exception e)
            {
                throw BuildException(request, e);
            }
            
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (RequestHandler<TResponse>) wrapperHandler;
        }

        private AsyncRequestHandler<TResponse> GetHandler<TResponse>(IAsyncRequest<TResponse> request)
        {
            var handlerType = typeof(IAsyncRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(AsyncRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(request);
            }
            catch (Exception e)
            {
                throw BuildException(request, e);
            }

            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncRequestHandler<TResponse>) wrapperHandler;
        }

        private IEnumerable<NotificationHandler> GetNotificationHandlers(INotification notification)
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var wrapperType = typeof(NotificationHandler<>).MakeGenericType(notification.GetType());

            var handlers = _multiInstanceFactory(handlerType);

            return handlers.Select(handler => (NotificationHandler) Activator.CreateInstance(wrapperType, handler)).ToList();
        }

        private IEnumerable<AsyncNotificationHandler> GetAsyncNotificationHandlers(IAsyncNotification notification)
        {
            var handlerType = typeof(IAsyncNotificationHandler<>).MakeGenericType(notification.GetType());
            var wrapperType = typeof(AsyncNotificationHandler<>).MakeGenericType(notification.GetType());

            var handlers = _multiInstanceFactory(handlerType);

            return handlers.Select(handler => (AsyncNotificationHandler) Activator.CreateInstance(wrapperType, handler)).ToList();
        }

        private abstract class RequestHandler<TResult>
        {
            public abstract TResult Handle(IRequest<TResult> message);
        }

        private class RequestHandler<TCommand, TResult> : RequestHandler<TResult> where TCommand : IRequest<TResult>
        {
            private readonly IRequestHandler<TCommand, TResult> _inner;

            public RequestHandler(IRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(IRequest<TResult> message)
            {
                return _inner.Handle((TCommand) message);
            }
        }

        private abstract class NotificationHandler
        {
            public abstract void Handle(INotification message);
        }

        private class NotificationHandler<TNotification> : NotificationHandler where TNotification : INotification
        {
            private readonly INotificationHandler<TNotification> _inner;

            public NotificationHandler(INotificationHandler<TNotification> inner)
            {
                _inner = inner;
            }

            public override void Handle(INotification message)
            {
                _inner.Handle((TNotification) message);
            }
        }

        private abstract class AsyncRequestHandler<TResult>
        {
            public abstract Task<TResult> Handle(IAsyncRequest<TResult> message, CancellationToken cancellationToken);
        }

        private class AsyncRequestHandler<TCommand, TResult> : AsyncRequestHandler<TResult>
            where TCommand : IAsyncRequest<TResult>
        {
            private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

            public AsyncRequestHandler(IAsyncRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override Task<TResult> Handle(IAsyncRequest<TResult> message, CancellationToken cancellationToken)
            {
                return _inner.Handle((TCommand) message, cancellationToken);
            }
        }

        private abstract class AsyncNotificationHandler
        {
            public abstract Task Handle(IAsyncNotification message, CancellationToken cancellationToken);
        }

        private class AsyncNotificationHandler<TNotification> : AsyncNotificationHandler
            where TNotification : IAsyncNotification
        {
            private readonly IAsyncNotificationHandler<TNotification> _inner;

            public AsyncNotificationHandler(IAsyncNotificationHandler<TNotification> inner)
            {
                _inner = inner;
            }

            public override Task Handle(IAsyncNotification message, CancellationToken cancellationToken)
            {
                return _inner.Handle((TNotification) message, cancellationToken);
            }
        }
    }
}