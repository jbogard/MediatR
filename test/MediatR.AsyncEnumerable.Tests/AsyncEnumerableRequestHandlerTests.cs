namespace MediatR.AsyncEnumerable.Tests
{
    using Shouldly;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using Xunit;

    public class AsyncEnumerableRequestHandlerTests
    {
        public class Sing : IRequest<Song>
        {
            public string Message { get; set; }
        }

        public class Song
        {
            public string Message { get; set; }
        }

        public class SingHandler : IAsyncEnumerableRequestHandler<Sing, Song>
        {
            public async IAsyncEnumerable<Song> Handle(Sing request, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                yield return await Task.Run(() => new Song { Message = request.Message + " do" });
                yield return await Task.Run(() => new Song { Message = request.Message + " re" });
                yield return await Task.Run(() => new Song { Message = request.Message + " mi" });
                yield return await Task.Run(() => new Song { Message = request.Message + " fa" });
                yield return await Task.Run(() => new Song { Message = request.Message + " so" });
                yield return await Task.Run(() => new Song { Message = request.Message + " la" });
                yield return await Task.Run(() => new Song { Message = request.Message + " ti" });
                yield return await Task.Run(() => new Song { Message = request.Message + " do" });
            }
        }

        [Fact]
        public async Task Should_call_abstract_handler()
        {
            // Arrange
            IAsyncEnumerableRequestHandler<Sing, Song> handler = new SingHandler();

            // Act
            IAsyncEnumerable<Song> items = handler.Handle(new Sing() { Message = "Sing" }, default);

            // Assert
            (await items.ElementAtAsync(0)).Message.ShouldBe("Sing do");
            (await items.ElementAtAsync(1)).Message.ShouldBe("Sing re");
            (await items.ElementAtAsync(2)).Message.ShouldBe("Sing mi");
            (await items.ElementAtAsync(3)).Message.ShouldBe("Sing fa");
            (await items.ElementAtAsync(4)).Message.ShouldBe("Sing so");
            (await items.ElementAtAsync(5)).Message.ShouldBe("Sing la");
            (await items.ElementAtAsync(6)).Message.ShouldBe("Sing ti");
            (await items.ElementAtAsync(7)).Message.ShouldBe("Sing do");
        }
    }
}
