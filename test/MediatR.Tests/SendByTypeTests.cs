using System.Threading;

namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class SendByTypeTests
    {
        public interface IPing : IRequest<Pong>
        {
            string Message { get; set; }
        }

        public class Ping : IPing
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IRequestHandler<IPing, Pong>
        {
            public Task<Pong> Handle(IPing request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong { Message = request.Message + " Pong" });
            }
        }

        [Fact]
        public async Task Should_resolve_main_handler()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.Send<IPing, Pong>(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");
        }
    }
}