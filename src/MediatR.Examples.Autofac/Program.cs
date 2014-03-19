namespace MediatR.Examples.Autofac
{
    using System;
    using System.IO;
    using CommonServiceLocator.AutofacAdapter.Unofficial;
    using global::Autofac;
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
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof (IMediator).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (IRequestHandler<,>));
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (IAsyncRequestHandler<,>));
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (IPostRequestHandler<,>));
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (IAsyncPostRequestHandler<,>));
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (INotificationHandler<>));
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsClosedTypesOf(typeof (IAsyncNotificationHandler<>));
            builder.RegisterInstance(Console.Out).As<TextWriter>();

            var lazy = new Lazy<IServiceLocator>(() => new AutofacServiceLocator(builder.Build()));
            var serviceLocatorProvider = new ServiceLocatorProvider(() => lazy.Value);
            builder.RegisterInstance(serviceLocatorProvider);
            
            var mediator = serviceLocatorProvider().GetInstance<IMediator>();

            return mediator;
        }
    }
}