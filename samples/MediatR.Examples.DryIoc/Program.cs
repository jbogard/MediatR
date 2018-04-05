using System;
using System.IO;
using System.Threading.Tasks;
using DryIoc;

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

            container.UseInstance<TextWriter>(writer);
            container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Ping).GetAssembly() }, Registrator.Interfaces);

            return new Mediator(container.Resolve);
        }
    }
}
