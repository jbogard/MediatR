namespace MediatR.Examples.Unity
{
    using System;
    using System.Linq;
    using Microsoft.Practices.Unity;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out).Wait();

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var container = new UnityContainer();
            container.RegisterType<IMediator, Mediator>();
            container.RegisterTypes(AllClasses.FromAssemblies(typeof(Ping).Assembly), WithMappings.FromAllInterfaces, GetName, GetLifetimeManager);
            container.RegisterType(typeof(INotificationHandler<>), typeof(GenericHandler), GetName(typeof(GenericHandler)));
            container.RegisterType(typeof(IAsyncNotificationHandler<>), typeof(GenericAsyncHandler), GetName(typeof(GenericAsyncHandler)));
            container.RegisterInstance(Console.Out);
            container.RegisterInstance<SingleInstanceFactory>(t => container.Resolve(t));
            container.RegisterInstance<MultiInstanceFactory>(t => container.ResolveAll(t));

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