using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;

namespace MediatR.Examples.NoContainer
{
    static class Program
    {
        static void Main()
        {
            var mediator = BuildMediator();
            Runner.Run(mediator, Console.Out);
            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var libraryManager = PlatformServices.Default.LibraryManager;
            var mediator = new Mediator(t => SingleInstanceFactory(t, libraryManager), t => MultiInstanceFactory(t, libraryManager));
            return mediator;
        }

        private static IEnumerable<object> MultiInstanceFactory(Type serviceType, ILibraryManager libraryManager)
        {
            return libraryManager.GetAssemblies()
                .SelectMany(s => s.ExportedTypes)
                .Where(t => serviceType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                .Select(type => Activator.CreateInstance(type, Console.Out));
        }

        private static object SingleInstanceFactory(Type serviceType, ILibraryManager libraryManager)
        {
            var type = libraryManager.GetAssemblies()
                .SelectMany(s => s.ExportedTypes)
                .First(t => serviceType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));

            return Activator.CreateInstance(type);
        }

        private static IEnumerable<Assembly> GetAssemblies(this ILibraryManager libraryManager)
        {
            // Get and load all assemblies referencing MediatR...
            return libraryManager.GetReferencingLibraries("MediatR")
                .SelectMany(a => a.Assemblies)
                .Select(Assembly.Load);
        }
    }
}
