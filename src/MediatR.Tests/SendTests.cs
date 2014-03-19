namespace MediatR.Tests
{
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class SendTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IRequestHandler<Ping, Pong>
        {
            public Pong Handle(Ping message)
            {
                return new Pong { Message = message.Message + " Pong" };
            }
        }

        public void Should_resolve_main_handler()
        {
            var container = new Container(cfg =>
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                }));

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            var response = mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");
        }
    }
}