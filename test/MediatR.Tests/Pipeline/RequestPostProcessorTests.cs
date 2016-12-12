namespace MediatR.Tests.Pipeline
{
    using System.Threading.Tasks;
    using MediatR.Pipeline;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class RequestPostProcessorTests
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

        public class PingPongPostProcessor : IRequestPostProcessor<Ping, Pong>
        {
            public Task Process(Ping request, Pong response)
            {
                response.Message = response.Message + " " + request.Message;

                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task Should_run_postprocessors()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
                });
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAsync(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong Ping");
        }

    }
}