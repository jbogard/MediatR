namespace MediatR.Tests
{
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
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (IRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");
        }
    }
}