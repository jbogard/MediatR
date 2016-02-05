using System.Threading;

namespace MediatR.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class AsyncPublishCancelTests
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

            public async Task Handle(Ping message, CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<object>();
                cancellationToken.Register(() => tcs.SetCanceled());
                await tcs.Task;

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

            public async Task Handle(Ping message, CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<object>();
                cancellationToken.Register(() => tcs.SetCanceled());
                await tcs.Task;

                await _writer.WriteLineAsync(message.Message + " Pung");
            }
        }

        [Fact]
        public async Task Should_throw_cancelled()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishCancelTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncNotificationHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<IMediator>().Use<Mediator>();
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            });

            var mediator = container.GetInstance<IMediator>();

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            Exception exception = null;
            try
            {
                await mediator.PublishAsync(new Ping {Message = "Ping"}, tokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            exception.ShouldBeOfType<TaskCanceledException>();
            result.ShouldNotContain("Ping Pong");
            result.ShouldNotContain("Ping Pung");
        }
    }
}