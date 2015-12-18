using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatR.Examples.NoContainer
{
    class Program
    {
        static void Main()
        {
            var mediator = BuildMediator();
            Runner.Run(mediator, Console.Out);
            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var mediator = new Mediator(SingleInstanceFactory, MultiInstanceFactory);
            return mediator;
        }

        private static IEnumerable<object> MultiInstanceFactory(Type serviceType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(serviceType.IsAssignableFrom)
                .Select(type => Activator.CreateInstance(type, Console.Out));
        }

        private static object SingleInstanceFactory(Type serviceType)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .First(serviceType.IsAssignableFrom);
            return Activator.CreateInstance(type);
        }
    }
}
