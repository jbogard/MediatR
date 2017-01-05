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

            Runner.Run(mediator, Console.Out).Wait();

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().BindAllInterfaces());
            kernel.Bind<TextWriter>().ToConstant(Console.Out);
            kernel.Bind<SingleInstanceFactory>().ToMethod(ctx => t => ctx.Kernel.TryGet(t));
            kernel.Bind<MultiInstanceFactory>().ToMethod(ctx => t => ctx.Kernel.GetAll(t));

            var mediator = kernel.Get<IMediator>();

            return mediator;
        }
    }
}
