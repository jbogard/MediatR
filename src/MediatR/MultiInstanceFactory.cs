using System;
using System.Collections.Generic;

namespace MediatR
{
    /// <summary>
    /// Factory method for creating multiple instances. Used to build instances of
    /// <see cref="INotificationHandler{TNotification}"/> and <see cref="IAsyncNotificationHandler{TNotification}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An enumerable of instances of type <paramref name="serviceType" /></returns>
    public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);
}