namespace MediatR.Examples.Unity
{
    using System;
    using System.Linq;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Practices.Unity;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var container = new UnityContainer();
            container.RegisterTypes(AllClasses.FromAssemblies(typeof (IMediator).Assembly), WithMappings.FromAllInterfaces);
            container.RegisterTypes(AllClasses.FromAssemblies(typeof(Ping).Assembly), WithMappings.FromAllInterfaces, GetName, GetLifetimeManager);
            container.RegisterType(typeof(INotificationHandler<>), typeof(GenericHandler), GetName(typeof(GenericHandler)));
            container.RegisterType(typeof(IAsyncNotificationHandler<>), typeof(GenericAsyncHandler), GetName(typeof(GenericAsyncHandler)));
            container.RegisterInstance(Console.Out);

            var serviceLocator = new UnityServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            container.RegisterInstance(serviceLocatorProvider);

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }

        static bool IsNotificationHandler(Type type)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && (x.GetGenericTypeDefinition() == typeof(INotificationHandler<>) || x.GetGenericTypeDefinition() == typeof(IAsyncNotificationHandler<>)));
        }

        static LifetimeManager GetLifetimeManager(Type type)
        {
            return IsNotificationHandler(type) ? new ContainerControlledLifetimeManager() : null;
        }

        static string GetName(Type type)
        {
            return IsNotificationHandler(type) ? string.Format("HandlerFor" + type.Name) : string.Empty;
        }
    }
}