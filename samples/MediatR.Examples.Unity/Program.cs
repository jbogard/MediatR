using System;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;

namespace MediatR.Examples.Unity
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out, "Unity").Wait();
        }

        private static IMediator BuildMediator()
        {
            var container = new UnityContainer();

            container.RegisterInstance(Console.Out)
                     .RegisterMediator(new HierarchicalLifetimeManager())
                     .RegisterMediatorHandlers(Assembly.GetAssembly(typeof(Ping)));

            return container.Resolve<IMediator>();
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class IUnityContainerExtensions
    {
        public static IUnityContainer RegisterMediator(this IUnityContainer container, LifetimeManager lifetimeManager)
        {
            return container.RegisterType<IMediator, Mediator>(lifetimeManager)
                            .RegisterInstance<SingleInstanceFactory>(t => container.IsRegistered(t) ? container.Resolve(t) : null)
                            .RegisterInstance<MultiInstanceFactory>(t => container.IsRegistered(t) ? container.ResolveAll(t) : new object[0]);
        }

        public static IUnityContainer RegisterMediatorHandlers(this IUnityContainer container, Assembly assembly)
        {
            return container.RegisterTypesImplementingType(assembly, typeof(IRequestHandler<>))
                            .RegisterTypesImplementingType(assembly, typeof(IRequestHandler<,>))
                            .RegisterTypesImplementingType(assembly, typeof(IAsyncRequestHandler<>))
                            .RegisterTypesImplementingType(assembly, typeof(IAsyncRequestHandler<,>))
                            .RegisterTypesImplementingType(assembly, typeof(INotificationHandler<>))
                            .RegisterTypesImplementingType(assembly, typeof(IAsyncNotificationHandler<>));
        }

        /// <summary>
        ///     Register all implementations of a given type for provided assembly.
        /// </summary>
        public static IUnityContainer RegisterTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
        {
            foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
            {
                var interfaces = implementation.GetInterfaces();
                foreach (var @interface in interfaces)
                    container.RegisterType(@interface, implementation);
            }

            return container;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                    return true;

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}