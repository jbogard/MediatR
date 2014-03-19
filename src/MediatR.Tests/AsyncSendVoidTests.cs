namespace MediatR.Tests
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class AsyncSendVoidTests
    {
        public class Ping : IAsyncRequest
        {
            public string Message { get; set; }
        }

        public class PingHandler : AsyncRequestHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PingHandler(TextWriter writer)
            {
                _writer = writer;
            }

            protected async override Task HandleCore(Ping message)
            {
                await _writer.WriteAsync(message.Message + " Pong");
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
                    scanner.AddAllTypesOf(typeof (IAsyncRequestHandler<,>));
                });
                cfg.For<TextWriter>().Use(writer);
            });


            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            var response = mediator.SendAsync(new Ping { Message = "Ping" });

            Task.WaitAll(response);

            builder.ToString().ShouldBe("Ping Pong");
        }
    }
}