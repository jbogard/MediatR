using System;
using System.IO;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Extensions;

namespace MediatR.Examples.GraceIoc
{
    class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "GraceIoC");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new DependencyInjectionContainer();
            
            container.Configure(c =>
            {
                c.ExportFunc<ServiceFactory>(scope => scope.Locate);
            
                c.ExportInstance<TextWriter>(writer);
            
                //Pipeline works out of the box here
                c.ExportAssemblies(
                        new[] {typeof(IMediator).Assembly, typeof(Ping).Assembly})
                    .ByInterfaces();
            });

            return container.Locate<IMediator>();
        }
    }
}