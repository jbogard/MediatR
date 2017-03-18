using System;
using System.Reflection;
using LightInject;
using MediatR.Pipeline;

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
            serviceContainer.RegisterAssembly(typeof(IMediator).GetTypeInfo().Assembly, (serviceType, implementingType) => !serviceType.GetTypeInfo().IsClass);
            serviceContainer.RegisterInstance(Console.Out);

            serviceContainer.RegisterAssembly(typeof(Ping).GetTypeInfo().Assembly, (serviceType, implementingType) =>
            {
                return serviceType.IsConstructedGenericType &&
                        (
                            serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                            serviceType.GetGenericTypeDefinition() == typeof(IAsyncRequestHandler<,>) ||
                            serviceType.GetGenericTypeDefinition() == typeof(ICancellableAsyncRequestHandler<,>) ||
                            serviceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>) ||
                            serviceType.GetGenericTypeDefinition() == typeof(IAsyncNotificationHandler<>) ||
                            serviceType.GetGenericTypeDefinition() == typeof(ICancellableAsyncNotificationHandler<>
                        ));
            });
            
            //Pipeline
            //TODO: can't get the order right..
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            serviceContainer.Register(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            serviceContainer.Register(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));


            serviceContainer.Register<SingleInstanceFactory>(fac => t => fac.GetInstance(t));
            serviceContainer.Register<MultiInstanceFactory>(fac => t => fac.GetAllInstances(t));
            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
