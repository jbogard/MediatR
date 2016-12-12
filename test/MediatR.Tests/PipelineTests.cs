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

        public class OuterBehavior : IPipelineBehavior<Ping, Pong>
        {
            private readonly Logger _output;

            public OuterBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next)
            {
                _output.Messages.Add("Outer before");
                var response = await next();
                _output.Messages.Add("Outer after");

                return response;
            }
        }

        public class InnerBehavior : IPipelineBehavior<Ping, Pong>
        {
            private readonly Logger _output;

            public InnerBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next)
            {
                _output.Messages.Add("Inner before");
                var response = await next();
                _output.Messages.Add("Inner after");

                return response;
            }
        }

        public class GenericBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly Logger _output;

            public GenericBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Generic before");
                var response = await next();
                _output.Messages.Add("Generic after");

                return response;
            }
        }

        public class GenericObjectBehavior : IPipelineBehavior<object, object>
        {
            private readonly Logger _output;

            public GenericObjectBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<object> Handle(object request, RequestHandlerDelegate<object> next)
            {
                _output.Messages.Add("Generic object before");
                var response = await next();
                _output.Messages.Add("Generic object after");

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
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<Logger>().Singleton().Use(output);
                cfg.For<IPipelineBehavior<Ping, Pong>>().Add<OuterBehavior>();
                cfg.For<IPipelineBehavior<Ping, Pong>>().Add<InnerBehavior>();
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

        [Fact]
        public async Task Should_wrap_generics_with_behavior()
        {
            var output = new Logger();
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<Logger>().Singleton().Use(output);
                cfg.For<IPipelineBehavior<object, object>>().Add<GenericObjectBehavior>();
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(GenericBehavior<,>));
                cfg.For<IPipelineBehavior<Ping, Pong>>().Add<OuterBehavior>();
                cfg.For<IPipelineBehavior<Ping, Pong>>().Add<InnerBehavior>();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });
            Console.WriteLine(container.WhatDoIHave());

            container.GetAllInstances<IPipelineBehavior<Ping, Pong>>();

            Console.WriteLine(container.WhatDoIHave());

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAsync(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong");

            output.Messages.ShouldBe(new[]
            {
                "Generic before",
                "Outer before",
                "Inner before",
                "Handler",
                "Inner after",
                "Outer after",
                "Generic after"
            });
        }
    }
}