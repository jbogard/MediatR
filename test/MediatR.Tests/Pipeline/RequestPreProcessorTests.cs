namespace MediatR.Tests.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR.Pipeline;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class RequestPreProcessorTests
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
            public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong { Message = request.Message + " Pong" });
            }
        }

        public class PingPreProcessor : IRequestPreProcessor<Ping>
        {
            public Task Process(Ping request, CancellationToken cancellationToken)
            {
                request.Message = request.Message + " Ping";

                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task Should_run_preprocessors()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IRequestPreProcessor<>));
                });
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
                cfg.For(typeof(IRequestMediator<,>)).Add(typeof(RequestMediator<,>));
            });

            var mediator = new Mediator(container.GetInstance);

            var response = await mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Ping Pong");
        }

    }
}