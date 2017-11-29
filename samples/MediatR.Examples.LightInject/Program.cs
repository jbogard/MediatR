using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LightInject;
using MediatR.Pipeline;

namespace MediatR.Examples.LightInject
{
    class Program
    {
        static Task Main(string[] args)
        {
            var mediator = BuildMediator();
            
            return Runner.Run(mediator, Console.Out, "LightInject");
        }

        private static IMediator BuildMediator()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.Register<IMediator, Mediator>();
            serviceContainer.RegisterAssembly(typeof(IMediator).GetTypeInfo().Assembly, (serviceType, implementingType) => !serviceType.GetTypeInfo().IsClass);
            serviceContainer.RegisterInstance(Console.Out);

            serviceContainer.RegisterAssembly(typeof(Ping).GetTypeInfo().Assembly, (serviceType, implementingType) =>
                serviceType.IsConstructedGenericType &&
                (
                    serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                    serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    serviceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                ));
            
            //Pipeline
            //TODO: can't get the order right..
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            serviceContainer.Register(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            serviceContainer.Register(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            serviceContainer.Register(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));


            serviceContainer.Register<SingleInstanceFactory>(fac => fac.GetInstance);
            serviceContainer.Register<MultiInstanceFactory>(fac => fac.GetAllInstances);
            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
