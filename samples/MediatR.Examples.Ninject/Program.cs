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
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out, "Ninject").Wait();
        }

        private static IMediator BuildMediator()
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind<TextWriter>().ToConstant(Console.Out);

            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<,>)).BindAllInterfaces());
             kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncRequestHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(ICancellableAsyncRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(ICancellableAsyncRequestHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(INotificationHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncNotificationHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(ICancellableAsyncNotificationHandler<>)).BindAllInterfaces());

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
