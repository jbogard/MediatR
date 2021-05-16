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
    using System.Linq;

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
            // Arrange
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

            // Act
            var items = mediator.ToAsyncEnumerable(new Sing { Message = "Sing" });

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

        [Fact]
        public async Task Should_resolve_main_handler_via_dynamic_dispatch()
        {
            // Arrange
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

            // Act
            var items = mediator.ToAsyncEnumerable(request);

            // Assert
            ((await items.ElementAtAsync(0)) as Song).Message.ShouldBe("Sing do");
            ((await items.ElementAtAsync(1)) as Song).Message.ShouldBe("Sing re");
            ((await items.ElementAtAsync(2)) as Song).Message.ShouldBe("Sing mi");
            ((await items.ElementAtAsync(3)) as Song).Message.ShouldBe("Sing fa");
            ((await items.ElementAtAsync(4)) as Song).Message.ShouldBe("Sing so");
            ((await items.ElementAtAsync(5)) as Song).Message.ShouldBe("Sing la");
            ((await items.ElementAtAsync(6)) as Song).Message.ShouldBe("Sing ti");
            ((await items.ElementAtAsync(7)) as Song).Message.ShouldBe("Sing do");
        }

        [Fact]
        public async Task Should_resolve_main_handler_by_specific_interface()
        {
            // Arrange
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

            // Act
            var items = mediator.ToAsyncEnumerable(new Sing { Message = "Sing" });

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