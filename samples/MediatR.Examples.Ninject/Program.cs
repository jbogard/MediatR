using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using MediatR.Pipeline;
using Ninject;
using Ninject.Syntax;
using Ninject.Extensions.Conventions;
using Ninject.Planning.Bindings.Resolvers;

namespace MediatR.Examples.Ninject
{
    internal class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Ninject");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            kernel.Bind<TextWriter>().ToConstant(writer);

            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<,>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<>)).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(INotificationHandler<>)).BindAllInterfaces());

            //Pipeline
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPreProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPostProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(GenericPipelineBehavior<,>));
            kernel.Bind(typeof(IRequestPreProcessor<>)).To(typeof(GenericRequestPreProcessor<>));
            kernel.Bind(typeof(IRequestPostProcessor<,>)).To(typeof(GenericRequestPostProcessor<,>));
            kernel.Bind(typeof(IRequestPostProcessor<,>)).To(typeof(ConstrainedRequestPostProcessor<,>));
            kernel.Bind(typeof(INotificationHandler<>)).To(typeof(ConstrainedPingedHandler<>)).WhenNotificationMatchesType<Pinged>();

            kernel.Bind(typeof(RequestProcessor<,>)).To(typeof(RequestProcessor<,>));
            kernel.Bind(typeof(RequestProcessor<>)).To(typeof(RequestProcessor<>));
            kernel.Bind(typeof(NotificationProcessor<>)).To(typeof(NotificationProcessor<>));

            kernel.Bind<SingleInstanceFactory>().ToMethod(ctx => t => ctx.Kernel.TryGet(t));

            var mediator = kernel.Get<IMediator>();

            return mediator;
        }
    }

    public static class BindingExtensions
    {
        public static IBindingInNamedWithOrOnSyntax<object> WhenNotificationMatchesType<TNotification>(this IBindingWhenSyntax<object> syntax)
            where TNotification : INotification
        {
            return syntax.When(request => typeof(TNotification).IsAssignableFrom(request.Service.GenericTypeArguments.Single()));
        }
    }

}
