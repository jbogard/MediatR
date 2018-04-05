using System;
using System.IO;
using System.Threading.Tasks;
using DryIocZero;

namespace MediatR.Examples.DryIocZero
{
    static class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);
            return Runner.Run(mediator, writer, "DryIocZero");
        }

        private static IMediator BuildMediator(TextWriter writer)
        {
            var container = new Container();

            container.UseInstance(writer);

            return new Mediator(container.Resolve);
        }
    }
}
