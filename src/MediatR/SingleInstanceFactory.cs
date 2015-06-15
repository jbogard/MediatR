using System;

namespace MediatR
{
    /// <summary>
    /// Factory method for creating single instances. Used to build instances of
    /// <see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IAsyncRequestHandler{TRequest,TResponse}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An instance of type <paramref name="serviceType" /></returns>
    public delegate object SingleInstanceFactory(Type serviceType);
}