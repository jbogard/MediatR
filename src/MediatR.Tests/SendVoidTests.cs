namespace MediatR.Tests
{
    using System.IO;
    using System.Text;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class SendVoidTests
    {
        public class Ping : IRequest
        {
            public string Message { get; set; }
        }

        public class PingHandler : RequestHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PingHandler(TextWriter writer)
            {
                _writer = writer;
            }

            protected override void HandleCore(Ping message)
            {
                _writer.Write(message.Message + " Pong");
            }
        }

        public void Should_resolve_main_void_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (IRequestHandler<,>));
                });
                cfg.For<TextWriter>().Use(writer);
            });


            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            mediator.Send(new Ping { Message = "Ping" });

            builder.ToString().ShouldBe("Ping Pong");
        }
    }
}