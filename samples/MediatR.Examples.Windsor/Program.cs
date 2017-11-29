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
            var mediator = BuildMediator();

            return Runner.Run(mediator, Console.Out, "Castle.Windsor");
        }

        private static IMediator BuildMediator()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
            container.Kernel.AddHandlersFilter(new ContravariantFilter());

            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(IRequestHandler<,>)).WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(IRequestHandler<>)).WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(INotificationHandler<>)).WithServiceAllInterfaces());

            container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());
            container.Register(Component.For<TextWriter>().Instance(Console.Out));
            container.Register(Component.For<SingleInstanceFactory>().UsingFactoryMethod<SingleInstanceFactory>(k => k.Resolve));
            container.Register(Component.For<MultiInstanceFactory>().UsingFactoryMethod<MultiInstanceFactory>(k => t => (IEnumerable<object>)k.ResolveAll(t)));

            //Pipeline
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPreProcessorBehavior<,>)).NamedAutomatically("PreProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPostProcessorBehavior<,>)).NamedAutomatically("PostProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(GenericPipelineBehavior<,>)).NamedAutomatically("Pipeline"));
            container.Register(Component.For(typeof(IRequestPreProcessor<>)).ImplementedBy(typeof(GenericRequestPreProcessor<>)).NamedAutomatically("PreProcessor"));
            container.Register(Component.For(typeof(IRequestPostProcessor <,>)).ImplementedBy(typeof(GenericRequestPostProcessor<,>)).NamedAutomatically("PostProcessor"));

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}