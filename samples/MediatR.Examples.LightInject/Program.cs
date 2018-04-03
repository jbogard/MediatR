using System;
using System.IO;
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
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "LightInject");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.Register<IMediator, Mediator>();            
            serviceContainer.RegisterInstance<TextWriter>(writer);

            serviceContainer.RegisterAssembly(typeof(Ping).GetTypeInfo().Assembly, (serviceType, implementingType) =>
                serviceType.IsConstructedGenericType &&
                (
                    serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                    serviceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    serviceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                ));
                    
            serviceContainer.RegisterOrdered(typeof(IPipelineBehavior<,>),
                new[]
                {
                    typeof(RequestPreProcessorBehavior<,>),
                    typeof(RequestPostProcessorBehavior<,>),
                    typeof(GenericPipelineBehavior<,>)
                }, type => new PerContainerLifetime());

            
            serviceContainer.RegisterOrdered(typeof(IRequestPostProcessor<,>),
                new[]
                {
                    typeof(GenericRequestPostProcessor<,>),
                    typeof(ConstrainedRequestPostProcessor<,>)
                }, type => new PerContainerLifetime());
                   
            serviceContainer.Register(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));            
            

            serviceContainer.Register<ServiceFactory>(fac => fac.GetInstance);
            serviceContainer.Register<MultiInstanceFactory>(fac => fac.GetAllInstances);
            return serviceContainer.GetInstance<IMediator>(); 
        }
    }
}
