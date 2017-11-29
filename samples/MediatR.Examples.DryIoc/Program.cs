using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DryIoc;
using MediatR.Pipeline;

namespace MediatR.Examples.DryIoc
{
    class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "DryIoc");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Container();

            container.RegisterDelegate<SingleInstanceFactory>(r => r.Resolve);
            container.RegisterDelegate<MultiInstanceFactory>(r => serviceType => r.ResolveMany(serviceType));
            container.UseInstance<TextWriter>(writer);

            //Pipeline works out of the box here

            container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Ping).GetAssembly() }, type => type.GetTypeInfo().IsInterface); 

            return container.Resolve<IMediator>();
        }
    }
}
