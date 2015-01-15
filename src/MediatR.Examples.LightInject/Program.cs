using System;
using LightInject;
using LightInject.ServiceLocation;
using Microsoft.Practices.ServiceLocation;

namespace MediatR.Examples.LightInject
{
    class Program
    {
        static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.Register<IMediator, Mediator>();
            serviceContainer.RegisterAssembly(typeof (Ping).Assembly);
            serviceContainer.RegisterAssembly(typeof(IMediator).Assembly);

            serviceContainer.RegisterInstance(Console.Out);

            var serviceLocator = new LightInjectServiceLocator(serviceContainer);
            serviceContainer.RegisterInstance(new ServiceLocatorProvider(() => serviceLocator));

            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
