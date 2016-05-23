namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;
    using Xunit;

    public class AsyncSendTests
    {

        public class Ping : IAsyncRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IAsyncRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping message)
            {
                return Task.FromResult(new Pong { Message = message.Message + " Pong" });
            }
        }

        [Fact]
        public async Task Should_resolve_main_handler()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (IAsyncRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAsync(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");
        }
    }
}