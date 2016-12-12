namespace MediatR.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class PipelineTests
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
            private readonly Logger _output;

            public PingHandler(Logger output)
            {
                _output = output;
            }
            public Task<Pong> Handle(Ping message)
            {
                _output.Messages.Add("Handler");
                return Task.FromResult(new Pong { Message = message.Message + " Pong" });
            }
        }

        public class OuterBehavior : IPipelineBehavior
        {
            private readonly Logger _output;

            public OuterBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<object> Handle(object request, RequestHandlerDelegate next)
            {
                _output.Messages.Add("Outer before");
                var response = await next();
                _output.Messages.Add("Outer after");

                return response;
            }
        }

        public class InnerBehavior : IPipelineBehavior
        {
            private readonly Logger _output;

            public InnerBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<object> Handle(object request, RequestHandlerDelegate next)
            {
                _output.Messages.Add("Inner before");
                var response = await next();
                _output.Messages.Add("Inner after");

                return response;
            }
        }

        public class Logger
        {
            public IList<string> Messages { get; } = new List<string>();
        }

        [Fact]
        public async Task Should_wrap_with_behavior()
        {
            var output = new Logger();
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<AsyncSendTests.Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<Logger>().Singleton().Use(output);
                cfg.For<IPipelineBehavior>().Add<OuterBehavior>();
                cfg.For<IPipelineBehavior>().Add<InnerBehavior>();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAsync(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");

            output.Messages.ShouldBe(new []
            {
                "Outer before",
                "Inner before",
                "Handler",
                "Inner after",
                "Outer after"
            });
        }

    }
}