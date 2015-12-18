using System;
using LightInject;

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
            serviceContainer.RegisterAssembly(typeof(Ping).Assembly, (serviceType, implementingType) => !serviceType.IsClass);
            serviceContainer.RegisterAssembly(typeof(IMediator).Assembly, (serviceType, implementingType) => !serviceType.IsClass);
            serviceContainer.RegisterInstance(Console.Out);
            serviceContainer.Register<SingleInstanceFactory>(fac => t => fac.GetInstance(t));
            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
