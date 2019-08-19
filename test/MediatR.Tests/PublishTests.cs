using System.Threading;

namespace MediatR.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;
    using Xunit;

    public class PublishTests
    {
        public class Ping : INotification
        {
            public string Message { get; set; }
        }

        public class PongHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pong");
            }
        }

        public class PungHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PungHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pung");
            }
        }

        [Fact]
        public async Task Should_resolve_main_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            });

            var mediator = container.GetInstance<IMediator>();

            await mediator.Publish(new Ping { Message = "Ping" });

            var result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }

        [Fact]
        public async Task Should_resolve_main_handler_when_object_is_passed()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            });

            var mediator = container.GetInstance<IMediator>();

            object message = new Ping { Message = "Ping" };
            await mediator.Publish(message);

            var result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }

        public class SequentialMediator : Mediator
        {
            public SequentialMediator(ServiceFactory serviceFactory)
                : base(serviceFactory)
            {
            }

            protected override async Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
            {
                foreach (var handler in allHandlers)
                {
                    await handler(notification, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task Should_override_with_sequential_firing()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<SequentialMediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            });

            var mediator = container.GetInstance<IMediator>();

            await mediator.Publish(new Ping { Message = "Ping" });

            var result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }

        [Fact]
        public async Task Should_resolve_handlers_given_interface()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<SequentialMediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            });

            var mediator = container.GetInstance<IMediator>();

            INotification notification = new Ping { Message = "Ping" };
            await mediator.Publish(notification);

            var result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }
    }
}