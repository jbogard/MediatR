namespace MediatR.Examples.Ninject
{
    using System;
    using System.IO;
    using CommonServiceLocator.NinjectAdapter.Unofficial;
    using global::Ninject;
    using global::Ninject.Extensions.Conventions;
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
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof (IRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IPostRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncPostRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(INotificationHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IAsyncNotificationHandler<>)).BindAllInterfaces());
            kernel.Bind<TextWriter>().ToConstant(Console.Out);

            var serviceLocator = new NinjectServiceLocator(kernel);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            kernel.Bind<ServiceLocatorProvider>().ToConstant(serviceLocatorProvider);

            var mediator = serviceLocator.GetInstance<IMediator>();

            return mediator;
        }
    }
}