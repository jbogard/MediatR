using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;

namespace MediatR.Examples.Windsor
{
    internal class Program
    {
        static Task Main()
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

            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(IRequestHandler<,>)).WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(IRequestHandler<>)).WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().BasedOn(typeof(INotificationHandler<>)).WithServiceAllInterfaces());

            container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());
            container.Register(Component.For<TextWriter>().Instance(writer));
            container.Register(Component.For<SingleInstanceFactory>().UsingFactoryMethod<SingleInstanceFactory>(k => k.Resolve));

            //Pipeline
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPreProcessorBehavior<,>)).NamedAutomatically("PreProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(RequestPostProcessorBehavior<,>)).NamedAutomatically("PostProcessorBehavior"));
            container.Register(Component.For(typeof(IPipelineBehavior<,>)).ImplementedBy(typeof(GenericPipelineBehavior<,>)).NamedAutomatically("Pipeline"));
            container.Register(Component.For(typeof(IRequestPreProcessor<>)).ImplementedBy(typeof(GenericRequestPreProcessor<>)).NamedAutomatically("PreProcessor"));
            container.Register(Component.For(typeof(IRequestPostProcessor <,>)).ImplementedBy(typeof(GenericRequestPostProcessor<,>)).NamedAutomatically("PostProcessor"));
            container.Register(Component.For(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>)).NamedAutomatically("ConstrainedRequestPostProcessor"));
            container.Register(Component.For(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>)).NamedAutomatically("ConstrainedPingedHandler"));

            container.Register(Component.For(typeof(RequestProcessor<,>)));
            container.Register(Component.For(typeof(RequestProcessor<>)));
            container.Register(Component.For(typeof(NotificationProcessor<>)));

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}