using System.Threading;

namespace MediatR.Tests.Pipeline.Streams;

using Shouldly;
using Lamar;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

public class StreamPipelineBehaviorTests
{
    public class Sing : IStreamRequest<Song>
    {
        public string? Message { get; set; }
    }

    public class Song
    {
        public string? Message { get; set; }
    }

    public class SingHandler : IStreamRequestHandler<Sing, Song>
    {
        public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return await Task.Run(() => new Song { Message = request.Message + " Song" });
            yield return await Task.Run(() => new Song { Message = request.Message + " Sang" });
            yield return await Task.Run(() => new Song { Message = request.Message + " Seng" });
        }
    }

    public class SingSongPipelineBehavior : IStreamPipelineBehavior<Sing, Song>
    {
        public async IAsyncEnumerable<Song> Handle(Sing request, StreamHandlerDelegate<Song> next, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new Song { Message = "Start behaving..." };

            await foreach (var item in next().WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }

            yield return new Song { Message = "...Ready behaving" };
        }
    }

    [Fact]
    public async Task Should_run_pipeline_behavior()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Sing>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IStreamRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IStreamPipelineBehavior<,>));
            });
            cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(SingSongPipelineBehavior));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var responses = mediator.CreateStream(new Sing { Message = "Sing" });

        int i = 0;
        await foreach (var response in responses)
        {
            if (i == 0)
            {
                response.Message.ShouldBe("Start behaving...");
            }
            else if (i == 1)
            {
                response.Message.ShouldBe("Sing Song");
            }
            else if (i == 2)
            {
                response.Message.ShouldBe("Sing Sang");
            }
            else if (i == 3)
            {
                response.Message.ShouldBe("Sing Seng");
            }
            else if (i == 4)
            {
                response.Message.ShouldBe("...Ready behaving");
            }

            (++i).ShouldBeLessThanOrEqualTo(5);
        }
    }

}