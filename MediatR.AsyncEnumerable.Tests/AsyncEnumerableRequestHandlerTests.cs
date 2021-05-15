using System;
using Xunit;

namespace MediatR.AsyncEnumerable.Tests
{
    using Shouldly;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    namespace MediatR.AsyncEnumerable.Tests
    {
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
                IAsyncEnumerableRequestHandler<Sing, Song> handler = new SingHandler();

                int i = 0;
                await foreach (Song result in handler.Handle(new Sing() { Message = "Sing" }, default))
                {
                    if (i == 0)
                    {
                        result.Message.ShouldBe("Sing do");
                    }
                    else if (i == 1)
                    {
                        result.Message.ShouldBe("Sing re");
                    }
                    else if (i == 2)
                    {
                        result.Message.ShouldBe("Sing mi");
                    }
                    else if (i == 3)
                    {
                        result.Message.ShouldBe("Sing fa");
                    }
                    else if (i == 4)
                    {
                        result.Message.ShouldBe("Sing so");
                    }
                    else if (i == 5)
                    {
                        result.Message.ShouldBe("Sing la");
                    }
                    else if (i == 6)
                    {
                        result.Message.ShouldBe("Sing ti");
                    }
                    else if (i == 7)
                    {
                        result.Message.ShouldBe("Sing do");
                    }
                    i++;
                }

                i.ShouldBe(8);
            }
        }
    }
}
