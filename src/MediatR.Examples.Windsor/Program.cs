namespace MediatR.Examples.Windsor
{
    using System;
    using System.IO;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using CommonServiceLocator.WindsorAdapter.Unofficial;
    using Microsoft.Practices.ServiceLocation;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var container = new WindsorContainer();
            container.Register(Classes.FromAssemblyContaining<IMediator>().Pick().WithServiceAllInterfaces());
            container.Register(Classes.FromAssemblyContaining<Ping>().Pick().WithServiceAllInterfaces());
            container.Register(Component.For<TextWriter>().Instance(Console.Out));
            container.Kernel.AddHandlersFilter(new ContravariantFilter());

            var serviceLocator = new WindsorServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            container.Register(Component.For<ServiceLocatorProvider>().Instance(serviceLocatorProvider));

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}