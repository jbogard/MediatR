namespace MediatR.Examples.StructureMap
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using global::StructureMap;
    using Microsoft.Practices.ServiceLocation;

    class Program
    {
        static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out);
        }

        private static IMediator BuildMediator()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<Ping>();
                    scanner.AssemblyContainingType<IMediator>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IPostRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IAsyncPostRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(INotificationHandler<>));
                    scanner.AddAllTypesOf(typeof(IAsyncNotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(Console.Out);
                cfg.For(typeof (IPostRequestHandler<,>)).Use(typeof (GenericPostRequestHandler<,>));
            });

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            container.Configure(cfg => cfg.For<ServiceLocatorProvider>().Use(serviceLocatorProvider));

            var mediator = serviceLocator.GetInstance<IMediator>();

            return mediator;
        }
    }
}