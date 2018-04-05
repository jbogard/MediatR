using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;
using StructureMap;

namespace MediatR.Examples.StructureMap
{
    class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "StructureMap");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<Ping>();
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });

                //Pipeline
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(GenericPipelineBehavior<,>));
                cfg.For(typeof(IRequestPreProcessor<>)).Add(typeof(GenericRequestPreProcessor<>));
                cfg.For(typeof(IRequestPostProcessor<,>)).Add(typeof(GenericRequestPostProcessor<,>));
                cfg.For(typeof(IRequestPostProcessor<,>)).Add(typeof(ConstrainedRequestPostProcessor<,>));

                //Constrained notification handlers
                cfg.For(typeof(INotificationHandler<>)).Add(typeof(ConstrainedPingedHandler<>));

                // This is the default but let's be explicit. At most we should be container scoped.
                cfg.For(typeof(IRequestMediator<,>)).Use(typeof(RequestMediator<,>));
                cfg.For(typeof(INotificationMediator<>)).Use(typeof(NotificationMediator<>));

                cfg.For<TextWriter>().Use(writer);
            });

            return new Mediator(container.GetInstance);
        }
    }
}
