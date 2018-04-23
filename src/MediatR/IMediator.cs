namespace MediatR
{
    using System;

    /// <summary>
    /// Defines the mediator able to send requests and notifications
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Creates request pipeline handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="requestType">Reflected request type</param>
        /// <returns>The subject</returns>
        IRequestMediator<TResponse> GetRequestMediator<TResponse>(Type requestType);

        /// <summary>
        /// Creates notification hub
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <returns>The subject</returns>
        INotificationMediator<TNotification> GetNotificationMediator<TNotification>()
            where TNotification : INotification;
    }
}