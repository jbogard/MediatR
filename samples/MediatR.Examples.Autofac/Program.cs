using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Autofac;

namespace MediatR.Examples.Autofac
{
    internal static class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Autofac");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(typeof(Ping).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();
            }

            builder.RegisterInstance(writer).As<TextWriter>();

            // It appears Autofac returns the last registered types first
            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(GenericRequestPreProcessor<>)).As(typeof(IRequestPreProcessor<>));
            builder.RegisterGeneric(typeof(GenericRequestPostProcessor<,>)).As(typeof(IRequestPostProcessor<,>));
            builder.RegisterGeneric(typeof(GenericPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ConstrainedRequestPostProcessor<,>)).As(typeof(IRequestPostProcessor<,>));
            builder.RegisterGeneric(typeof(ConstrainedPingedHandler<>)).As(typeof(INotificationHandler<>));

            builder.RegisterGeneric(typeof(RequestMediator<,>)).As(typeof(IRequestMediator<,>));
            builder.RegisterGeneric(typeof(NotificationMediator<>)).As(typeof(INotificationMediator<>));

            var container = builder.Build();

            // The below returns:
            //  - RequestPreProcessorBehavior
            //  - RequestPostProcessorBehavior
            //  - GenericPipelineBehavior

            //var behaviors = container
            //    .Resolve<IEnumerable<IPipelineBehavior<Ping, Pong>>>()
            //    .ToList();

            return new Mediator(container.Resolve);
        }
    }
}