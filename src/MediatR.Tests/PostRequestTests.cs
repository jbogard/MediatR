namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class PostRequestTests
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

        public class PungHandler : IPostRequestHandler<Ping, Pong>
        {
            public void Handle(Ping request, Pong response)
            {
                response.Message = response.Message + " Pung";
            }
        }

        public class PangHandler : IPostRequestHandler<Ping, Pong>
        {
            public void Handle(Ping request, Pong response)
            {
                response.Message = "Pang " + response.Message;
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
                    scanner.AddAllTypesOf(typeof(IPostRequestHandler<,>));
                }));

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            var response = mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Pang Ping Pong Pung");
        }
    }
}