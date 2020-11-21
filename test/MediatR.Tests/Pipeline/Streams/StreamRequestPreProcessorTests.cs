using System.Threading;

namespace MediatR.Tests.Pipeline.Streams
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MediatR.Pipeline.Streams;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class StreamRequestPreProcessorTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class StreamPingHandler : IStreamRequestHandler<Ping, Pong>
        {
            public async IAsyncEnumerable<Pong> Handle(Ping request, [EnumeratorCancellation]CancellationToken cancellationToken)
            {
                yield return await Task.Run(() => new Pong { Message = request.Message + " Pong" });
                yield return await Task.Run(() => new Pong { Message = request.Message + " Pang" });
                yield return await Task.Run(() => new Pong { Message = request.Message + " Peng" });
            }
        }

        public class StreamPingPreProcessor : IStreamRequestPreProcessor<Ping>
        {
            public Task Process(Ping request, CancellationToken cancellationToken)
            {
                request.Message = "PrePing " + request.Message;

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
                    scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IStreamRequestPreProcessor<>));
                });
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestPreProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var responses = mediator.CreateStream(new Ping { Message = "Ping" });

            int i = 0;
            await foreach (var response in responses)
            {
                if (i == 0)
                {
                    response.Message.ShouldBe("PrePing Ping Pong");
                }
                else if ( i == 1)
                {
                    response.Message.ShouldBe("PrePing Ping Pang");
                }
                else if ( i == 2 )
                {
                    response.Message.ShouldBe("PrePing Ping Peng");
                }

                (++i).ShouldBeLessThanOrEqualTo(3);
            }
        }

    }
}