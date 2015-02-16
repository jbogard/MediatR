namespace MediatR.Examples.Windsor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

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
            container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());
            container.Register(Classes.FromAssemblyContaining<Ping>().Pick().WithServiceAllInterfaces());
            container.Register(Component.For<TextWriter>().Instance(Console.Out));
            container.Kernel.AddHandlersFilter(new ContravariantFilter());
            container.Register(Component.For<SingleInstanceFactory>().UsingFactoryMethod<SingleInstanceFactory>(k => t => k.Resolve(t)));
            container.Register(Component.For<MultiInstanceFactory>().UsingFactoryMethod<MultiInstanceFactory>(k => t => (IEnumerable<object>)k.ResolveAll(t)));

            var mediator = container.Resolve<IMediator>();

            return mediator;
        }
    }
}