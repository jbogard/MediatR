namespace MediatR.Tests
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NotificationHandlersOrder;
    using System;
    using System.Text;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class NotificationOrderTests
    {
        public class Ping : INotification
        {
            public string Message { get; set; }
        }

        [NotificationHandlerOrder(1)]
        public class PongHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pong 1");
            }
        }

        [NotificationHandlerOrder(3)]
        public class ThirdPongHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public ThirdPongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pong 3");
            }
        }


        [NotificationHandlerOrder(2)]
        public class SecondPongHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public SecondPongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pong 2");
            }
        }

        [Fact]
        public async Task Should_process_notification_handlers_in_correct_order()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(NotificationOrderTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
            });

            var mediator = container.GetInstance<IMediator>();
            await mediator.Publish(new Ping { Message = "Ping" });

            var result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            result[0].ShouldBe("Ping Pong 1");
            result[1].ShouldBe("Ping Pong 2");
            result[2].ShouldBe("Ping Pong 3");
        }
    }
}