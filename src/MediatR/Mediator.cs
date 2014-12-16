namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Practices.ServiceLocation;

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
        /// Send a notification to multiple handlers
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <param name="notification">Notification object</param>
        void Publish<TNotification>(TNotification notification) where TNotification : INotification;

        /// <summary>
        /// Asynchronously send a notification to multiple handlers
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <param name="notification">Notification object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync<TNotification>(TNotification notification) where TNotification : IAsyncNotification;
    }

    /// <summary>
    /// Default mediator implementation relying on Common Service Locator for resolving handlers
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly ServiceLocatorProvider _serviceLocatorProvider;

        /// <summary>
        /// Constructs a Mediator instance with the supplied service locator provider delegate
        /// </summary>
        /// <param name="serviceLocatorProvider">Provider delegate for instantiating a service locator. Invoked on every request/notification</param>
        public Mediator(ServiceLocatorProvider serviceLocatorProvider)
        {
            _serviceLocatorProvider = serviceLocatorProvider;
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            TResponse result = defaultHandler.Handle(request);

            return result;
        }

        public async Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            TResponse result = await defaultHandler.Handle(request);

            return result;
        }

        public void Publish<TNotification>(TNotification notification) where TNotification : INotification
        {
            var notificationHandlers = GetNotificationHandlers<TNotification>();

            foreach (var handler in notificationHandlers)
            {
                handler.Handle(notification);
            }
        }

        public async Task PublishAsync<TNotification>(TNotification notification) where TNotification : IAsyncNotification
        {
            var notificationHandlers = GetAsyncNotificationHandlers<TNotification>();

            foreach (var handler in notificationHandlers)
            {
                await handler.Handle(notification);
            }
        }

        private static InvalidOperationException BuildException(object message)
        {
            return new InvalidOperationException("Handler was not found for request of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.");
        }

        private RequestHandler<TResponse> GetHandler<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(RequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceLocatorProvider().GetInstance(handlerType);

            if (handler == null)
                throw BuildException(request);

            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (RequestHandler<TResponse>)wrapperHandler;
        }

        private AsyncRequestHandler<TResponse> GetHandler<TResponse>(IAsyncRequest<TResponse> request)
        {
            var handlerType = typeof(IAsyncRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(AsyncRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceLocatorProvider().GetInstance(handlerType);

            if (handler == null)
                throw BuildException(request);

            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncRequestHandler<TResponse>)wrapperHandler;
        }

        private IEnumerable<INotificationHandler<TNotification>> GetNotificationHandlers<TNotification>()
            where TNotification : INotification
        {
            return _serviceLocatorProvider().GetAllInstances<INotificationHandler<TNotification>>();
        }

        private IEnumerable<IAsyncNotificationHandler<TNotification>> GetAsyncNotificationHandlers<TNotification>()
            where TNotification : IAsyncNotification
        {
            return _serviceLocatorProvider().GetAllInstances<IAsyncNotificationHandler<TNotification>>();
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
                return _inner.Handle((TCommand)message);
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
                return _inner.Handle((TCommand)message);
            }
        }
    }
}