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
    using System;

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


        [Fact]
        public async Task Should_resolve_composite_handler_using_request_type_names()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(CompositePingTests));
                    scanner.IncludeNamespaceContainingType<CompositePingTests>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<IRequestHandler<AsyncCompositeRequest, ExpandoObject>>().Use<AsyncCompositeRequestHandler>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            // testing from param
            var pongResponse = await mediator.Send(new Ping());
            var pongAgainResponse = await mediator.Send(new Foo());

            dynamic response = await mediator.Send(new AsyncCompositeRequest(new List<IBaseRequest> { new Ping(), new Foo() } ));

            Assert.Equal(response.Ping.Message, "Ping Pong");
            Assert.Equal(response.Foo.Total, 9001);

            // Testing from function
            IEnumerable<IBaseRequest> GetRequestList()
            {
                yield return new Ping();
                yield return new Foo();
            }

            dynamic responseFromFunc = await mediator.Send(new AsyncCompositeRequest(GetRequestList));

            Assert.Equal(responseFromFunc.Ping.Message, "Ping Pong");
            Assert.Equal(responseFromFunc.Foo.Total, 9001);
        }


        [Fact]
        public async Task Should_resolve_composite_handler_dictionary_names()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(CompositePingTests));
                    scanner.IncludeNamespaceContainingType<CompositePingTests>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<IRequestHandler<AsyncCompositeRequest, ExpandoObject>>().Use<AsyncCompositeRequestHandler>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var pongResponse = await mediator.Send(new Ping());
            var pongAgainResponse = await mediator.Send(new Foo());

            // testing from param
            dynamic response = await mediator.Send(new AsyncCompositeRequest(
                new Dictionary<string, IBaseRequest>()
                {
                    { "Ning", new Ping() },
                    { "Noo",  new Foo()  }
                }
            ));

            Assert.Equal(response.Ning.Message, "Ping Pong");
            Assert.Equal(response.Noo.Total, 9001);

            // Testing from function
            Dictionary<string, IBaseRequest> GetRequestDict()
            {
                return new Dictionary<string, IBaseRequest>()
                {
                    { "Ning", new Ping() },
                    { "Noo",  new Foo()  }
                };
            }

            dynamic responseFromFunc = await mediator.Send(new AsyncCompositeRequest(GetRequestDict));

            Assert.Equal(responseFromFunc.Ning.Message, "Ping Pong");
            Assert.Equal(responseFromFunc.Noo.Total, 9001);
        }

    }
}
