namespace MediatR.AsyncEnumerable.Tests
{
    using MediatR.AsyncEnumerable;
    using System.Threading;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ToAsyncEnumerableTests
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
        public async Task Should_resolve_main_handler()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(ToAsyncEnumerableTests));
                    scanner.IncludeNamespaceContainingType<Sing>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncEnumerableRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = mediator.ToAsyncEnumerable(new Sing { Message = "Sing" });

            int i = 0;
            await foreach (Song result in response)
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

        [Fact]
        public async Task Should_resolve_main_handler_via_dynamic_dispatch()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(ToAsyncEnumerableTests));
                    scanner.IncludeNamespaceContainingType<Sing>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncEnumerableRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            object request = new Sing { Message = "Sing" };
            var response = mediator.ToAsyncEnumerable(request);

            int i = 0;
            await foreach (Song result in response)
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

        [Fact]
        public async Task Should_resolve_main_handler_by_specific_interface()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(ToAsyncEnumerableTests));
                    scanner.IncludeNamespaceContainingType<Sing>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncEnumerableRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            int i = 0;
            await foreach (Song result in mediator.ToAsyncEnumerable(new Sing { Message = "Sing" }))
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