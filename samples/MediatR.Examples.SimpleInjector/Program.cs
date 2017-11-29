using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Examples.SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using global::SimpleInjector;

    internal class Program
    {
        private static Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "SimpleInjector");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Container();
            var assemblies = GetAssemblies().ToArray();
            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(typeof(IRequestHandler<,>), assemblies);
			container.Register(typeof(IRequestHandler<>), assemblies);
            container.RegisterCollection(typeof(INotificationHandler<>), assemblies);
            container.RegisterSingleton<TextWriter>(writer);

            //Pipeline
            container.RegisterCollection(typeof(IPipelineBehavior<,>), new []
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
                typeof(GenericPipelineBehavior<,>)
            });
            container.RegisterCollection(typeof(IRequestPreProcessor<>), new [] { typeof(GenericRequestPreProcessor<>)});
            container.RegisterCollection(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>) });

            container.RegisterSingleton(new SingleInstanceFactory(container.GetInstance));
            container.RegisterSingleton(new MultiInstanceFactory(container.GetAllInstances));

            container.Verify();

            var mediator = container.GetInstance<IMediator>();

            return mediator;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(IMediator).GetTypeInfo().Assembly;
            yield return typeof(Ping).GetTypeInfo().Assembly;
        }
    }
}
