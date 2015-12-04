using System;
using DryIoc;

namespace MediatR.Examples.DryIoc
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
            var container = new Container();

            container.RegisterDelegate<SingleInstanceFactory>(r => serviceType => r.Resolve(serviceType));
            container.RegisterDelegate<MultiInstanceFactory>(r => serviceType => r.ResolveMany(serviceType));
            container.RegisterInstance(Console.Out);

            container.RegisterMany(new[] {typeof (IMediator).Assembly, typeof (Ping).Assembly}, type => type.IsInterface); 

            return container.Resolve<IMediator>();
        }
    }
}
