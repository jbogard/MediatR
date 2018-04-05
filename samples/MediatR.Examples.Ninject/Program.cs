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
            kernel.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<,>)).BindDefaultInterface());

            kernel.Bind<TextWriter>().ToConstant(writer);

            // note: Generic constraints are not supported out-of-the-box
            bool SkipConstrained(Type x) => !x.Name.StartsWith("Constrained");

            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(IRequestHandler<,>))
                .Where(SkipConstrained).BindAllInterfaces());
            kernel.Bind(scan => scan.FromAssemblyContaining<Ping>().SelectAllClasses().InheritedFrom(typeof(INotificationHandler<>))
                .Where(SkipConstrained).BindAllInterfaces());

            //Pipeline
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPreProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPostProcessorBehavior<,>));
            kernel.Bind(typeof(IPipelineBehavior<,>)).To(typeof(GenericPipelineBehavior<,>));
            kernel.Bind(typeof(IRequestPreProcessor<>)).To(typeof(GenericRequestPreProcessor<>));
            kernel.Bind(typeof(IRequestPostProcessor<,>)).To(typeof(GenericRequestPostProcessor<,>));

            // note: Generic constraints are not supported out-of-the-box
            //kernel.Bind(typeof(INotificationHandler<>)).To(typeof(ConstrainedPingedHandler<>));
            //kernel.Bind(typeof(IRequestPostProcessor<,>)).To(typeof(ConstrainedRequestPostProcessor<,>));

            kernel.Bind(typeof(IRequestMediator<,>)).To(typeof(RequestMediator<,>))
                .WhenNotificationMatchesType(new[] { typeof(Ping), typeof(Pong)},
                                             new[] { typeof(Jing), typeof(Unit) });

            kernel.Bind(typeof(INotificationMediator<>)).To(typeof(NotificationMediator<>))
                .WhenNotificationMatchesType(new[] { typeof(Pinged) }, new[] { typeof(Ponged) });

            return new Mediator(t => kernel.TryGet(t));
        }
    }

    public static class BindingExtensions
    {
        // note: May be done better to check Request constraints, but for sample is fine
        public static IBindingInNamedWithOrOnSyntax<object> WhenNotificationMatchesType(
            this IBindingWhenSyntax<object> syntax, params Type[][] typeArgsVariants) =>
            syntax.When(request =>
            {
                var typeArgs = request.Service.GenericTypeArguments;
                return typeArgsVariants.Any(closedTypeArgs => closedTypeArgs.Select((t, i) => t.IsAssignableFrom(typeArgs[i])).All(id => id));
            });
    }
}
