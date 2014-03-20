namespace MediatR.Examples.Unity
{
    using System;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Practices.Unity;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);
        }

        private static IMediator BuildMediator()
        {
            var container = new UnityContainer();
            container.RegisterTypes(AllClasses.FromAssemblies(typeof (Ping).Assembly), WithMappings.FromAllInterfaces);
            container.RegisterTypes(AllClasses.FromAssemblies(typeof (IMediator).Assembly), WithMappings.FromAllInterfaces);
            container.RegisterInstance(Console.Out);

            var serviceLocator = new UnityServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            container.RegisterInstance(serviceLocatorProvider);

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}