namespace MediatR.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class AsyncPublishTests
    {
        public class Ping : IAsyncNotification
        {
            public string Message { get; set; }
        }

        public class PongHandler : IAsyncNotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public async Task Handle(Ping message)
            {
                await _writer.WriteLineAsync(message.Message + " Pong");
            }
        }

        public class PungHandler : IAsyncNotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PungHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public async Task Handle(Ping message)
            {
                await _writer.WriteLineAsync(message.Message + " Pung");
            }
        }

        public void Should_resolve_main_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (IAsyncNotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            });

            var mediator = container.GetInstance<IMediator>();

            var response = mediator.PublishAsync(new Ping { Message = "Ping" });

            Task.WaitAll(response);

            var result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }
    }
}