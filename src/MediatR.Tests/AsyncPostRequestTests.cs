namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class AsyncPostRequestTests
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
            public async Task<Pong> Handle(Ping message)
            {
                return await Task.Factory.StartNew(() => new Pong { Message = message.Message + " Pong" });
            }
        }

        public class PungHandler : IAsyncPostRequestHandler<Ping, Pong>
        {
            public async Task Handle(Ping request, Pong response)
            {
                await Task.Factory.StartNew(() => response.Message = response.Message + " Pung");
            }
        }

        public class PangHandler : IAsyncPostRequestHandler<Ping, Pong>
        {
            public async Task Handle(Ping request, Pong response)
            {
                await Task.Factory.StartNew(() => response.Message = "Pang " + response.Message);
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
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IAsyncPostRequestHandler<,>));
                }));

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            var response = mediator.SendAsync(new Ping { Message = "Ping" });

            Task.WaitAll(response);

            response.Result.Message.ShouldBe("Pang Ping Pong Pung");
        }
    }
}