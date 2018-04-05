using System;

namespace MediatR
{
    /// <summary>
    /// Factory method used to resolve services.
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An instance of type <paramref name="serviceType" /></returns>
    public delegate object ServiceFactory(Type serviceType);

    internal static class ServiceFactoryExtensions
    {
        public static T GetInstance<T>(this ServiceFactory factory) =>
            (T) factory(typeof(T));
    }
}