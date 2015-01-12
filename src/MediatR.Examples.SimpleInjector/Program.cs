namespace MediatR.Examples.SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommonServiceLocator.SimpleInjectorAdapter;
    using global::SimpleInjector;
    using global::SimpleInjector.Extensions;
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
            var container = new Container();
            var assemblies = GetAssemblies().ToArray();
            container.Register<IMediator, Mediator>();
            container.RegisterManyForOpenGeneric(typeof(IRequestHandler<,>), assemblies);
            container.RegisterManyForOpenGeneric(typeof(IAsyncRequestHandler<,>), assemblies);
            container.RegisterManyForOpenGeneric(typeof(INotificationHandler<>), container.RegisterAll, assemblies);
            container.RegisterManyForOpenGeneric(typeof(IAsyncNotificationHandler<>), container.RegisterAll, assemblies);
            container.Register(() => Console.Out);

            var serviceLocator = new SimpleInjectorServiceLocatorAdapter(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);
            container.Register(() => serviceLocatorProvider);

            container.Verify();

            var mediator = container.GetInstance<IMediator>();

            return mediator;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(IMediator).Assembly;
            yield return typeof(Ping).Assembly;
        }
    }
}
