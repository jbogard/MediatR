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

    public class StreamRequestCombinePreAndPostProcessorTests
    {
        public class Sing : IRequest<Song>
        {
            public string Message { get; set; }
        }

        public class Song
        {
            public string Message { get; set; }
        }

        public class SingHandler : IStreamRequestHandler<Sing, Song>
        {
            public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation]CancellationToken cancellationToken)
            {
                yield return await Task.Run(() => new Song { Message = request.Message + " Song" });
                yield return await Task.Run(() => new Song { Message = request.Message + " Sang" });
                yield return await Task.Run(() => new Song { Message = request.Message + " Seng" });
            }
        }

        public class StreamSingPreProcessor : IStreamRequestPreProcessor<Sing>
        {
            static int preCounter = 0;

            public Task Process(Sing request, CancellationToken cancellationToken)
            {
                request.Message = $"Pre {++preCounter} {request.Message}";

                return Task.FromResult(0);
            }
        }


        public class SingSongPostProcessor : IStreamRequestPostProcessor<Sing, Song>
        {
            static int postCounter = 0;

            public Task Process(Sing request, Song response, CancellationToken cancellationToken)
            {
                response.Message += $" Post {++postCounter}";

                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task Should_run_processors()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Sing>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IStreamRequestPreProcessor<>));
                    scanner.AddAllTypesOf(typeof(IStreamRequestPostProcessor<,>));
                });
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestPreProcessorBehavior<,>));
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestPostProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var responses = mediator.CreateStream(new Sing { Message = "Sing" });

            int i = 0;
            await foreach (var response in responses)
            {
                if (i == 0)
                {
                    response.Message.ShouldBe("Pre 1 Sing Song Post 1");
                }
                else if (i == 1)
                {
                    response.Message.ShouldBe("Pre 1 Sing Sang Post 2");
                }
                else if (i == 2)
                {
                    response.Message.ShouldBe("Pre 1 Sing Seng Post 3");
                }

                (++i).ShouldBeLessThanOrEqualTo(3);
            }
        }

    }
}