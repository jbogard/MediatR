namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

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
            public async Task<Pong> Handle(Ping message)
            {
                return await Task.Factory.StartNew(() => new Pong {Message = message.Message + " Pong"});
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
                }));

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            var response = mediator.SendAsync(new Ping { Message = "Ping" });

            Task.WaitAll(response);

            response.Result.Message.ShouldBe("Ping Pong");
        }
    }
}