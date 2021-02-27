using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using MediatR.Pipeline;

namespace MediatR.Examples.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    internal class Program
    {
        private static Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Castle.Windsor");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
            container.Kernel.AddHandlersFilter(new ContravariantFilter());

            // *** The default lifestyle for Windsor is Singleton
            // *** If you are using ASP.net, it's better to register your services with 'Per Web Request LifeStyle'.
            
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(IRequestHandler<,>)).WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(INotificationHandler<>)).WithServiceAllInterfaces());

            container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());
            container.Register(Component.For<TextWriter>().Instance(writer));
            container.Register(Component.For<ServiceFactory>().UsingFactoryMethod<ServiceFactory>(k => (type =>
            {
                var enumerableType = type
                    .GetInterfaces()
                    .Concat(new [] {type})
                    .FirstOrDefault(t =>  t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return enumerableType != null ? k.ResolveAll(enumerableType.GetGenericArguments()[0]) : k.Resolve(type);
            })));

            //Pipeline
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPreProcessorBehavior<,>)).NamedAutomatically("PreProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPostProcessorBehavior<,>)).NamedAutomatically("PostProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(GenericPipelineBehavior<,>)).NamedAutomatically("Pipeline"));
            container.Register(Component.For(typeof(IRequestPreProcessor<>)).ImplementedBy(typeof(GenericRequestPreProcessor<>)).NamedAutomatically("PreProcessor"));
            container.Register(Component.For(typeof(IRequestPostProcessor <,>)).ImplementedBy(typeof(GenericRequestPostProcessor<,>)).NamedAutomatically("PostProcessor"));
            container.Register(Component.For(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>)).NamedAutomatically("ConstrainedRequestPostProcessor"));
            container.Register(Component.For(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>)).NamedAutomatically("ConstrainedPingedHandler"));

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}
