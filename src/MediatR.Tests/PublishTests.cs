namespace MediatR.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Practices.ServiceLocation;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

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

            public void Handle(Ping message)
            {
                _writer.WriteLine(message.Message + " Pong");
            }
        }

        public class PungHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PungHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public void Handle(Ping message)
            {
                _writer.WriteLine(message.Message + " Pung");
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
                    scanner.AddAllTypesOf(typeof (INotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
            });

            var serviceLocator = new StructureMapServiceLocator(container);
            var serviceLocatorProvider = new ServiceLocatorProvider(() => serviceLocator);

            var mediator = new Mediator(serviceLocatorProvider);

            mediator.Publish(new Ping { Message = "Ping" });

            var result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            result.ShouldContain("Ping Pong");
            result.ShouldContain("Ping Pung");
        }
    }
}