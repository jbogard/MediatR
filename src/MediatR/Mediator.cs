namespace MediatR
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    /// <summary>
    /// Provides access to the request and notification pipelines.
    /// The life-time of the <see cref="IMediator"/> may be controlled by registering it in IoC Container
    /// with specific sharing policy, e.g. as Singleton, Scoped, Transient, etc.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly ServiceFactory _serviceFactory;

        /// <summary>
        /// Initializes the default mediator by providing the service factory for creating the request and notification mediators.
        /// Service factory may be a delegate wrapping resolution from IoC Container library.
        /// </summary>
        /// <param name="serviceFactory">The service factory.</param>
        public Mediator(ServiceFactory serviceFactory) =>
            _serviceFactory = serviceFactory;

        /// <inheritdoc />
        public IRequestMediator<TResponse> GetRequestMediator<TResponse>(Type requestType)
        {
            Type[] requestResponseTypes = { requestType, typeof(TResponse) };
            return (IRequestMediator<TResponse>)Activator.CreateInstance(
                _requestMediatorType.MakeGenericType(requestResponseTypes),
                    _serviceFactory(_requestHandlerType.MakeGenericType(requestResponseTypes)),
                    _serviceFactory(_enumerableType.MakeGenericType(_requestPipelineType.MakeGenericType(requestResponseTypes))));
        }

        /// <inheritdoc />
        public INotificationMediator<TNotification> GetNotificationMediator<TNotification>()
            where TNotification : INotification
            {
                Type[] notificationType = { typeof(TNotification) };
                return (INotificationMediator<TNotification>)Activator.CreateInstance(
                    _notificationMediator.MakeGenericType(notificationType),
                    _serviceFactory(_enumerableType.MakeGenericType(_notificationHandler.MakeGenericType(notificationType))));
            }

        private static readonly Type _enumerableType = typeof(IEnumerable<>);

        private static readonly Type _requestMediatorType = typeof(RequestMediator<,>);
        private static readonly Type _requestHandlerType = typeof(IRequestHandler<,>);
        private static readonly Type _requestPipelineType = typeof(IPipelineBehavior<,>);

        private static readonly Type _notificationMediator = typeof(NotificationMediator<>);
        private static readonly Type _notificationHandler = typeof(INotificationHandler<>);
    }

    /// <summary>
    /// Handy extensions to simplify sending requests and publishing notifications via mediator.
    /// </summary>
    public static class MediatorExtensions
    {
        /// <summary>
        /// Asynchronously sends a request to a single handler and pipeline behaviors if any.
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="mediator">Mediator to use</param>
        /// <param name="request">Request object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        public static Task<TResponse> Send<TResponse>(this IMediator mediator,
            IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.NotNull(nameof(request)).GetType();
            var requestMediator = mediator.GetRequestMediator<TResponse>(requestType);
            return requestMediator.Send(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a notification to multiple handlers
        /// </summary>
        /// <param name="mediator">Mediator to use</param>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        public static Task Publish<TNotification>(this IMediator mediator,
            TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            notification = notification.NotNull(nameof(notification));
            var notificationMediator = mediator.GetNotificationMediator<TNotification>();
            return notificationMediator.Publish(notification, cancellationToken);
        }

        static T NotNull<T>(this T arg, string name) =>
            arg != null ? arg : throw new ArgumentNullException(name);
    }
}
