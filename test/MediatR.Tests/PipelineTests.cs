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

        public class Zing : IAsyncRequest<Zong>
        {
            public string Message { get; set; }
        }

        public class Zong
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

        public class ZingHandler : IAsyncRequestHandler<Zing, Zong>
        {
            private readonly Logger _output;

            public ZingHandler(Logger output)
            {
                _output = output;
            }
            public Task<Zong> Handle(Zing message)
            {
                _output.Messages.Add("Handler");
                return Task.FromResult(new Zong { Message = message.Message + " Zong" });
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

        public class InnerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly Logger _output;

            public InnerBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Inner generic before");
                var response = await next();
                _output.Messages.Add("Inner generic after");

                return response;
            }
        }

        public class OuterBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly Logger _output;

            public OuterBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Outer generic before");
                var response = await next();
                _output.Messages.Add("Outer generic after");

                return response;
            }
        }

        public class ConstrainedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : Ping
            where TResponse : Pong
        {
            private readonly Logger _output;

            public ConstrainedBehavior(Logger output)
            {
                _output = output;
            }

            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
            {
                _output.Messages.Add("Constrained before");
                var response = await next();
                _output.Messages.Add("Constrained after");

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

                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));

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
                "Outer generic before",
                "Inner generic before",
                "Handler",
                "Inner generic after",
                "Outer generic after",
            });
        }

        [Fact]
        public async Task Should_handle_constrained_generics()
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

                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(OuterBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(InnerBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(ConstrainedBehavior<,>));

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
                "Outer generic before",
                "Inner generic before",
                "Constrained before",
                "Handler",
                "Constrained after",
                "Inner generic after",
                "Outer generic after",
            });

            output.Messages.Clear();

            var zingResponse = await mediator.SendAsync(new Zing { Message = "Zing" });

            Console.WriteLine(container.WhatDoIHave());

            zingResponse.Message.ShouldBe("Zing Zong");

            output.Messages.ShouldBe(new[]
            {
                "Outer generic before",
                "Inner generic before",
                "Handler",
                "Inner generic after",
                "Outer generic after",
            });
        }
    }
}