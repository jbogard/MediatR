using System.Threading;

namespace MediatR.Tests.Composite
{
    using MediatR.Composite;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;
    using System.Collections.Generic;
    using System.Dynamic;

    public class CompositePingTests
    {
        #region Ping_Pong
        public class Ping : IRequest<Pong>
        {
            public string Message => "Ping";
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong { Message = $"{request.Message} Pong" });
            }
        }
        #endregion

        #region Foo_Bar
        public class Foo : IRequest<Bar>
        {
            public int Value => 9000;
        }

        public class Bar
        {
            public int Total { get; set; }
        }

        public class FooHandler : IRequestHandler<Foo, Bar>
        {
            public Task<Bar> Handle(Foo request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Bar { Total = request.Value + 1 });
            }
        }
        #endregion

        public class CompositePingRequest : DynamicCompositeRequest
        {
            private bool _merge;
            public CompositePingRequest(bool merge = false)
            {
                _merge = merge;
            }

            public override IEnumerable<IBaseRequest> Requests()
            {
                yield return new Ping();
                yield return new Foo();
            }

            public override bool MergeRepsonses()
            {
                return _merge;
            }
        }

        public class CompositePingRequestHandler : AsyncDynamicCompositeRequestHandler<CompositePingRequest>
        {
            public CompositePingRequestHandler(IMediator mediator) : base(mediator)
            {
            }
        }



        [Fact]
        public async Task Should_resolve_composite_handler_no_merge()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(CompositePingTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var pongResponse = await mediator.Send(new Ping());
            var pongAgainResponse = await mediator.Send(new Foo());

            dynamic response = await mediator.Send(new CompositePingRequest());

            Assert.Equal(response.Ping.Message, "Ping Pong");
            Assert.Equal(response.Foo.Total, 9001);
        }



        [Fact]
        public async Task Should_resolve_composite_handler_with_merge()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(CompositePingTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var pongResponse = await mediator.Send(new Ping());
            var pongAgainResponse = await mediator.Send(new Foo());

            dynamic response = await mediator.Send(new CompositePingRequest(merge: true));

            Assert.Equal(response.Message, "Ping Pong");
            Assert.Equal(response.Total, 9001);
        }
    }
}
