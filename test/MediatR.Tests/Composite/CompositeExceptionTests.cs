using MediatR.Composite;
using MediatR.Pipeline;
using Shouldly;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.Composite
{
    public class CompositeExceptionTests
    {

        #region PingPong
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingException : Exception
        {
            public PingException(string message) : base(message + " Thrown")
            {
            }
        }

        public class PingHandler : IRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            {
                throw new PingException(request.Message);
            }
        }

        public class PingPongExceptionHandlerForType : IRequestExceptionHandler<Ping, Pong, PingException>
        {
            public Task Handle(Ping request, PingException exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
            {
                state.SetHandled(new Pong() { Message = exception.Message + " Handled by Type" });

                return Task.CompletedTask;
            }
        }

        public class PingPongExceptionHandler : RequestExceptionHandler<Ping, Pong>
        {
            protected override void Handle(Ping request, Exception exception, RequestExceptionHandlerState<Pong> state)
            {
                state.SetHandled(new Pong() { Message = exception.Message + " Handled" });
            }
        }

        public class PingPongExceptionHandlerNotHandled : RequestExceptionHandler<Ping, Pong>
        {
            protected override void Handle(Ping request, Exception exception, RequestExceptionHandlerState<Pong> state)
            {
                request.Message = exception.Message + " Not Handled";
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
        public async Task Should_run_exception_handler_and_allow_for_exception_not_to_throw_for_composite_request()
        {
            var container = new Container(cfg =>
            {
                cfg.For<IRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IRequestExceptionHandler<Ping, Pong, Exception>>().Use<PingPongExceptionHandler>();
                cfg.For<IRequestExceptionHandler<Ping, Pong, PingException>>().Use<PingPongExceptionHandlerForType>();
                cfg.For<IRequestHandler<Foo, Bar>>().Use<FooHandler>();
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestExceptionProcessorBehavior<,>));
                cfg.For<IRequestHandler<AsyncCompositeRequest, ExpandoObject>>().Use<AsyncCompositeRequestHandler>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            dynamic response = await mediator.Send(new AsyncCompositeRequest(new List<IBaseRequest>
                {
                    new Ping { Message = "Ping" },
                    new Foo()
                }
            ));

            Assert.Equal("Ping Thrown Handled by Type", response.Ping.Message);
            Assert.Equal(9001, response.Foo.Total);
        }

        [Fact]
        public async Task Should_run_exception_handler_and_allow_for_exception_to_be_still_thrown_for_composite_request()
        {
            var container = new Container(cfg =>
            {
                cfg.For<IRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IRequestExceptionHandler<Ping, Pong, Exception>>().Use<PingPongExceptionHandlerNotHandled>();
                cfg.For<IRequestHandler<Foo, Bar>>().Use<FooHandler>();
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestExceptionProcessorBehavior<,>));
                cfg.For<IRequestHandler<AsyncCompositeRequest, ExpandoObject>>().Use<AsyncCompositeRequestHandler>();
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var compositeRequest = new AsyncCompositeRequest(new List<IBaseRequest>
                {
                    new Ping { Message = "Ping" },
                    new Foo()
                }
            );
            await Should.ThrowAsync<PingException>(async () =>
            {
                await mediator.Send(compositeRequest);
            });

            Assert.Equal("Ping Thrown Not Handled", (compositeRequest.Requests["Ping"] as Ping).Message);
        }
    }
}
