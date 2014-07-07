namespace MediatR.Examples.Autofac
{
    using System;
    using System.IO;
    using CommonServiceLocator.AutofacAdapter.Unofficial;
    using global::Autofac;
    using global::Autofac.Features.Variance;
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
            builder.RegisterSource(new ContravariantRegistrationSource());
            builder.RegisterAssemblyTypes(typeof (IMediator).Assembly).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof (Ping).Assembly).AsImplementedInterfaces();
            builder.RegisterInstance(Console.Out).As<TextWriter>();

            var lazy = new Lazy<IServiceLocator>(() => new AutofacServiceLocator(builder.Build()));
            var serviceLocatorProvider = new ServiceLocatorProvider(() => lazy.Value);
            builder.RegisterInstance(serviceLocatorProvider);
            
            var mediator = serviceLocatorProvider().GetInstance<IMediator>();

            return mediator;
        }
    }
}