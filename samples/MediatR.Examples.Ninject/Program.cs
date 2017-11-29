using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Examples.Ninject
{
    using System;
    using System.IO;
    using global::Ninject;
    using global::Ninject.Extensions.Conventions;
    using global::Ninject.Planning.Bindings.Resolvers;

    internal class Program
    {
        private static Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Ninject");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind<TextWriter>().ToConstant(writer);

            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<,>)).BindAllInterfaces());
             kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(INotificationHandler<>)).BindAllInterfaces());

            //Pipeline
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPreProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPostProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(GenericPipelineBehavior<,>));
            kernel.Bind(typeof(IRequestPreProcessor<>)).To(typeof(GenericRequestPreProcessor<>));
            kernel.Bind(typeof(IRequestPostProcessor<,>)).To(typeof(GenericRequestPostProcessor<,>));

            kernel.Bind<SingleInstanceFactory>().ToMethod(ctx => t => ctx.Kernel.TryGet(t));
            kernel.Bind<MultiInstanceFactory>().ToMethod(ctx => t => ctx.Kernel.GetAll(t));

            var mediator = kernel.Get<IMediator>();

            return mediator;
        }
    }
}
