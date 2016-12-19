using System;
using System.Reflection;
using DryIoc;

namespace MediatR.Examples.DryIoc
{
    class Program
    {
        static void Main()
        {
            var mediator = BuildMediator();

            Runner.Run(mediator, Console.Out).Wait();

            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var container = new Container();

            container.RegisterDelegate<SingleInstanceFactory>(r => serviceType => r.Resolve(serviceType));
            container.RegisterDelegate<MultiInstanceFactory>(r => serviceType => r.ResolveMany(serviceType));
            container.RegisterInstance(Console.Out);

            container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Ping).GetAssembly() }, type => type.GetTypeInfo().IsInterface); 

            return container.Resolve<IMediator>();
        }
    }
}
