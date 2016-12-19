using System;
using System.Reflection;
using LightInject;

namespace MediatR.Examples.LightInject
{
    class Program
    {
        static void Main(string[] args)
        {
            var mediator = BuildMediator();
            
            Runner.Run(mediator, Console.Out).Wait();

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.Register<IMediator, Mediator>();
            serviceContainer.RegisterAssembly(typeof(Ping).GetTypeInfo().Assembly, (serviceType, implementingType) => !serviceType.GetTypeInfo().IsClass);
            serviceContainer.RegisterAssembly(typeof(IMediator).GetTypeInfo().Assembly, (serviceType, implementingType) => !serviceType.GetTypeInfo().IsClass);
            serviceContainer.RegisterInstance(Console.Out);
            serviceContainer.Register<SingleInstanceFactory>(fac => t => fac.GetInstance(t));
            serviceContainer.Register<MultiInstanceFactory>(fac => t => fac.GetAllInstances(t));
            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
