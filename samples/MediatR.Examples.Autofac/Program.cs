using System.Linq;
using System.Reflection;
using Autofac.Core;
using MediatR.Pipeline;

namespace MediatR.Examples.Autofac
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::Autofac;
    using global::Autofac.Features.Variance;

    internal static class Program
    {
        public static void Main(string[] args)
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out, "Autofac").Wait();
        }

        private static IMediator BuildMediator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new ContravariantRegistrationSource());
            builder.RegisterAssemblyTypes(typeof (IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof (Ping).GetTypeInfo().Assembly).Where(t => 
                    t.GetInterfaces().Any(i => i.IsClosedTypeOf(typeof(IRequestHandler<,>))
                                            || i.IsClosedTypeOf(typeof(IAsyncRequestHandler <,>))
                                            || i.IsClosedTypeOf(typeof(ICancellableAsyncRequestHandler<,>))
                                            || i.IsClosedTypeOf(typeof(INotificationHandler<>))
                                            || i.IsClosedTypeOf(typeof(IAsyncNotificationHandler<>))
                                            || i.IsClosedTypeOf(typeof(ICancellableAsyncNotificationHandler<>))
                                         )
                )
                .AsImplementedInterfaces();
            builder.RegisterInstance(Console.Out).As<TextWriter>();

            //Pipeline
            //TODO: doesn't work, too many implementations of the pipelinebehaviors:
            // - GenericPipelineBehavior<Ping, Pong>
            // - GenericPipelineBehavior<IRequest<Pong>, Pong>
            // - GenericPipelineBehavior<Object, Pong>
            //builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            //builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            //builder.RegisterGeneric(typeof(GenericPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            //builder.RegisterGeneric(typeof(GenericRequestPreProcessor<>)).As(typeof(IRequestPreProcessor<>));
            //builder.RegisterGeneric(typeof(GenericRequestPostProcessor<,>)).As(typeof(IRequestPostProcessor<,>));

            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t =>
                {
                    object o;
                    return c.TryResolve(t, out o) ? o : null;
                };
            });
            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => (IEnumerable<object>) c.Resolve(typeof (IEnumerable<>).MakeGenericType(t));
            });
            
            var mediator = builder.Build().Resolve<IMediator>();

            return mediator;
        }
    }
}