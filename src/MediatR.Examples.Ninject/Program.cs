namespace MediatR.Examples.Ninject
{
    using System;
    using System.IO;
    using CommonServiceLocator.NinjectAdapter.Unofficial;
    using global::Ninject;
    using global::Ninject.Extensions.Conventions;
    using global::Ninject.Planning.Bindings.Resolvers;
    using Microsoft.Practices.ServiceLocation;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);
        }

        private static IMediator BuildMediator()
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().BindAllInterfaces());
            kernel.Bind<TextWriter>().ToConstant(Console.Out);

            var serviceLocator = new NinjectServiceLocator(kernel);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            kernel.Bind<ServiceLocatorProvider>().ToConstant(serviceLocatorProvider);

            var handlers = kernel.GetAll<INotificationHandler<Pinged>>();

            var mediator = serviceLocator.GetInstance<IMediator>();

            return mediator;
        }
    }
}