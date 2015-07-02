
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
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request);

        /// <summary>
        /// Asynchronously send a cancellable request to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken);

        /// <summary>
        /// Send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        void Publish(INotification notification);

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(IAsyncNotification notification);

        /// <summary>
        /// Asynchronously send a cancellable notification to multiple handlers
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken);
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

            var result = defaultHandler.Handle(request);

            return result;
        }

        public async Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            var result = await defaultHandler.Handle(request);

            return result;
        }

        public async Task<TResponse> SendAsync<TResponse>(ICancellableAsyncRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var defaultHandler = GetHandler(request);

            var result = await defaultHandler.Handle(request, cancellationToken);

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

        public async Task PublishAsync(IAsyncNotification notification)
        {
            var notificationHandlers = GetNotificationHandlers(notification);

            foreach (var handler in notificationHandlers)
            {
                await handler.Handle(notification);
            }
        }

        public async Task PublishAsync(ICancellableAsyncNotification notification, CancellationToken cancellationToken)
        {
            var notificationHandlers = GetNotificationHandlers(notification);

            foreach (var handler in notificationHandlers)
            {
                await handler.Handle(notification, cancellationToken);
            }
        }

        private RequestHandler<TResponse> GetHandler<TResponse>(IRequest<TResponse> request)
        {
            return GetHandler<RequestHandler<TResponse>, TResponse>(request,
                typeof(IRequestHandler<,>),
                typeof(RequestHandler<,>));
        }

        private AsyncRequestHandler<TResponse> GetHandler<TResponse>(IAsyncRequest<TResponse> request)
        {
            return GetHandler<AsyncRequestHandler<TResponse>, TResponse>(request,
                typeof(IAsyncRequestHandler<,>),
                typeof(AsyncRequestHandler<,>));
        }

        private CancellableAsyncRequestHandler<TResponse> GetHandler<TResponse>(ICancellableAsyncRequest<TResponse> request)
        {
            return GetHandler<CancellableAsyncRequestHandler<TResponse>, TResponse>(request,
                typeof(ICancellableAsyncRequestHandler<,>),
                typeof(CancellableAsyncRequestHandler<,>));
        }

        private TWrapper GetHandler<TWrapper, TResponse>(object request, Type handlerType, Type wrapperType)
        {
            var requestType = request.GetType();

            var genericHandlerType = handlerType.MakeGenericType(requestType, typeof(TResponse));
            var genericWrapperType = wrapperType.MakeGenericType(requestType, typeof(TResponse));

            var handler = GetHandler(request, genericHandlerType);

            return (TWrapper) Activator.CreateInstance(genericWrapperType, handler);
        }

        private object GetHandler(object request, Type handlerType)
        {
            try
            {
                return _singleInstanceFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(request, e);
            }
        }

        private IEnumerable<NotificationHandler> GetNotificationHandlers(INotification notification)
        {
            return GetNotificationHandlers<NotificationHandler>(notification,
                typeof(INotificationHandler<>),
                typeof(NotificationHandler<>));
        }

        private IEnumerable<AsyncNotificationHandler> GetNotificationHandlers(IAsyncNotification notification)
        {
            return GetNotificationHandlers<AsyncNotificationHandler>(notification,
                typeof(IAsyncNotificationHandler<>),
                typeof(AsyncNotificationHandler<>));
        }

        private IEnumerable<CancellableAsyncNotificationHandler> GetNotificationHandlers(ICancellableAsyncNotification notification)
        {
            return GetNotificationHandlers<CancellableAsyncNotificationHandler>(notification,
                typeof (ICancellableAsyncNotificationHandler<>),
                typeof (CancellableAsyncNotificationHandler<>));
        }

        private IEnumerable<TWrapper> GetNotificationHandlers<TWrapper>(object notification, Type handlerType, Type wrapperType)
        {
            var genericHandlerType = handlerType.MakeGenericType(notification.GetType());
            var genericWrapperType = wrapperType.MakeGenericType(notification.GetType());

            return GetNotificationHandlers(notification, genericHandlerType)
                .Select(handler => Activator.CreateInstance(genericWrapperType, handler))
                .Cast<TWrapper>()
                .ToList();
        }

        private IEnumerable<object> GetNotificationHandlers(object notification, Type handlerType)
        {
            try
            {
                return _multiInstanceFactory(handlerType);
            }
            catch (Exception e)
            {
                throw BuildException(notification, e);
            }
        }

        private static InvalidOperationException BuildException(object message, Exception inner)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", inner);
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
            public abstract Task<TResult> Handle(IAsyncRequest<TResult> message);
        }

        private class AsyncRequestHandler<TCommand, TResult> : AsyncRequestHandler<TResult>
            where TCommand : IAsyncRequest<TResult>
        {
            private readonly IAsyncRequestHandler<TCommand, TResult> _inner;

            public AsyncRequestHandler(IAsyncRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override Task<TResult> Handle(IAsyncRequest<TResult> message)
            {
                return _inner.Handle((TCommand) message);
            }
        }

        private abstract class CancellableAsyncRequestHandler<TResult>
        {
            public abstract Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken);
        }

        private class CancellableAsyncRequestHandler<TCommand, TResult> : CancellableAsyncRequestHandler<TResult>
            where TCommand : ICancellableAsyncRequest<TResult>
        {
            private readonly ICancellableAsyncRequestHandler<TCommand, TResult> _inner;

            public CancellableAsyncRequestHandler(ICancellableAsyncRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override Task<TResult> Handle(ICancellableAsyncRequest<TResult> message, CancellationToken cancellationToken)
            {
                return _inner.Handle((TCommand) message, cancellationToken);
            }
        }

        private abstract class AsyncNotificationHandler
        {
            public abstract Task Handle(IAsyncNotification message);
        }

        private class AsyncNotificationHandler<TNotification> : AsyncNotificationHandler
            where TNotification : IAsyncNotification
        {
            private readonly IAsyncNotificationHandler<TNotification> _inner;

            public AsyncNotificationHandler(IAsyncNotificationHandler<TNotification> inner)
            {
                _inner = inner;
            }

            public override Task Handle(IAsyncNotification message)
            {
                return _inner.Handle((TNotification) message);
            }
        }

        private abstract class CancellableAsyncNotificationHandler
        {
            public abstract Task Handle(ICancellableAsyncNotification message, CancellationToken canellationToken);
        }

        private class CancellableAsyncNotificationHandler<TNotification> : CancellableAsyncNotificationHandler
            where TNotification : ICancellableAsyncNotification
        {
            private readonly ICancellableAsyncNotificationHandler<TNotification> _inner;

            public CancellableAsyncNotificationHandler(ICancellableAsyncNotificationHandler<TNotification> inner)
            {
                _inner = inner;
            }

            public override Task Handle(ICancellableAsyncNotification message, CancellationToken cancellationToken)
            {
                return _inner.Handle((TNotification) message, cancellationToken);
            }
        }
    }
}