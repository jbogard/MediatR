using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Unity;

namespace MediatR.Examples.Unity
{
    internal class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Unity");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new UnityContainer();

            container.RegisterInstance<TextWriter>(writer)
                     .RegisterMediatorHandlers(Assembly.GetAssembly(typeof(Ping)));

            container.RegisterType(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), "RequestPreProcessorBehavior");
            container.RegisterType(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>), "RequestPostProcessorBehavior");
            container.RegisterType(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>), "GenericPipelineBehavior");
            container.RegisterType(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>), "GenericRequestPreProcessor");
            container.RegisterType(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>), "GenericRequestPostProcessor");

            container.RegisterType(typeof(IRequestMediator<,>), typeof(RequestMediator<,>));
            container.RegisterType(typeof(INotificationMediator<>), typeof(NotificationMediator<>));

            // note: Unity doesn't support generic constraints
            //container.RegisterType(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>), "ConstrainedPingedHandler");
            //container.RegisterType(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>), "ConstrainedRequestPostProcessor");

            return new Mediator(t => container.Resolve(t));
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class IUnityContainerExtensions
    {
        public static IUnityContainer RegisterMediatorHandlers(this IUnityContainer container, Assembly assembly)
        {
            return container.RegisterTypesImplementingType(assembly, typeof(IRequestHandler<,>))
                            .RegisterNamedTypesImplementingType(assembly, typeof(INotificationHandler<>));
        }

        /// <summary>
        ///     Register all implementations of a given type for provided assembly.
        /// </summary>
        public static IUnityContainer RegisterTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
        {
            foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
            {
                var interfaces = implementation.GetInterfaces();
                foreach (var @interface in interfaces)
                    container.RegisterType(@interface, implementation);
            }

            return container;
        }

        /// <summary>
        ///     Register all implementations of a given type for provided assembly.
        /// </summary>
        public static IUnityContainer RegisterNamedTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
        {
            foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
            {
                var interfaces = implementation.GetInterfaces();
                foreach (var @interface in interfaces)
                    container.RegisterType(@interface, implementation, implementation.FullName);
            }

            return container;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                    return true;

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}