namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;
    using Xunit;

    public class SendAllTests
    {
        public class TreeRequest : IRequest<string> { }
        public class NutTreeRequest : TreeRequest { }
        public class FruitTreeRequest : TreeRequest { }

        public class PecanTree : IAsyncRequestHandler<NutTreeRequest, string>, IRequestHandler<TreeRequest, string>
        {
            private const string Tree = "Pecan";
            public Task<string> Handle(NutTreeRequest request) => Task.FromResult(Tree);
            public string Handle(TreeRequest request) => Tree;
        }

        public class AlmondTree : IAsyncRequestHandler<NutTreeRequest, string>, IRequestHandler<TreeRequest, string>
        {
            private const string Tree = "Almond";
            public Task<string> Handle(NutTreeRequest request) => Task.FromResult(Tree);
            public string Handle(TreeRequest request) => Tree;
        }

        public class PeachTree : IAsyncRequestHandler<FruitTreeRequest, string>, IRequestHandler<TreeRequest, string>
        {
            private const string Tree = "Peach";
            public Task<string> Handle(FruitTreeRequest request) => Task.FromResult(Tree);
            public string Handle(TreeRequest request) => Tree;
        }

        public class AppleTree : IAsyncRequestHandler<FruitTreeRequest, string>, IRequestHandler<TreeRequest, string>
        {
            private const string Tree = "Apple";
            public Task<string> Handle(FruitTreeRequest request) => Task.FromResult(Tree);
            public string Handle(TreeRequest request) => Tree;
        }

        public class PearTree : IAsyncRequestHandler<FruitTreeRequest, string>, IRequestHandler<TreeRequest, string>
        {
            private const string Tree = "Pear";
            public Task<string> Handle(FruitTreeRequest request) => Task.FromResult(Tree);
            public string Handle(TreeRequest request) => Tree;
        }

        [Fact]
        public async Task Should_resolve_all_trees()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<TreeRequest>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAll(new TreeRequest());

            response
                .ShouldSatisfyAllConditions(
                    () => response.Length.ShouldBe(5),
                    () => response.ShouldBeSubsetOf(new[] { "Almond", "Pecan", "Apple", "Peach", "Pear" })
                );
        }

        [Fact]
        public async Task Should_resolve_all_fruit_trees()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<TreeRequest>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAll(new FruitTreeRequest());

            response
                .ShouldSatisfyAllConditions(
                    () => response.Length.ShouldBe(3),
                    () => response.ShouldBeSubsetOf(new[] { "Apple", "Peach", "Pear" })
                );
        }

        [Fact]
        public async Task Should_resolve_all_nut_trees()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(AsyncPublishTests));
                    scanner.IncludeNamespaceContainingType<TreeRequest>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.SendAll(new NutTreeRequest());

            response
                .ShouldSatisfyAllConditions(
                    () => response.Length.ShouldBe(2),
                    () => response.ShouldBeSubsetOf(new[] { "Almond", "Pecan" })
                );
        }
    }
}
