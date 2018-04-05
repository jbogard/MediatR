using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;
using SimpleInjector;

namespace MediatR.Examples.SimpleInjector
{
    internal class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "SimpleInjector");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Container();
            var assemblies = GetAssemblies().ToArray();

            container.Register(typeof(IRequestMediator<,>), typeof(RequestMediator<,>));
            container.Register(typeof(INotificationMediator<>), typeof(NotificationMediator<>));

            container.Register(typeof(IRequestHandler<,>), assemblies);

            // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
            var notificationHandlerTypes = container.GetTypesToRegister(typeof(INotificationHandler<>), assemblies, new TypesToRegisterOptions
            {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });
            container.RegisterCollection(typeof(INotificationHandler<>), notificationHandlerTypes);

            container.RegisterSingleton<TextWriter>(writer);

            //Pipeline(
            container.RegisterCollection(typeof(IPipelineBehavior<,>), new []
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
                typeof(GenericPipelineBehavior<,>)
            });
            container.RegisterCollection(typeof(IRequestPreProcessor<>), new [] { typeof(GenericRequestPreProcessor<>) });
            container.RegisterCollection(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });

            container.Verify();

            return new Mediator(container.GetInstance);
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(IMediator).GetTypeInfo().Assembly;
            yield return typeof(Ping).GetTypeInfo().Assembly;
        }
    }
}
