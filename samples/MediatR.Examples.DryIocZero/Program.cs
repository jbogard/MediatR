using System;
using System.IO;
using DryIocZero;

namespace MediatR.Examples.DryIocZero
{
    static class Program
    {
        public static void Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            Runner.Run(mediator, writer, "DryIocZero").Wait();
        }

        private static IMediator BuildMediator(TextWriter writer)
        {
            var container = new Container();

            container.RegisterDelegate<SingleInstanceFactory>(r => r.Resolve);
            container.RegisterDelegate<MultiInstanceFactory>(r => r.ResolveMany);
            container.UseInstance(writer);

            return container.Resolve<IMediator>();
        }
    }
}
