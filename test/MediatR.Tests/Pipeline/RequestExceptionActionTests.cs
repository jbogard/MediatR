namespace MediatR.Tests.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR.Pipeline;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class RequestExceptionActionTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public abstract class PingPongException : Exception
        {
            protected PingPongException(string message) : base(message + " Thrown")
            {
            }
        }

        public class PingException : PingPongException
        {
            public PingException(string message) : base(message)
            {
            }
        }

        public class PongException : PingPongException
        {
            public PongException(string message) : base(message)
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

        public class PingPongExceptionAction<TRequest> : IRequestExceptionAction<TRequest, PingPongException>
        {
            public bool Executed { get; private set; }

            public Task Execute(TRequest request, PingPongException exception, CancellationToken cancellationToken)
            {
                Executed = true;
                return Task.CompletedTask;
            }
        }

        public class PingExceptionAction : IRequestExceptionAction<Ping, PingException>
        {
            public bool Executed { get; private set; }

            public Task Execute(Ping request, PingException exception, CancellationToken cancellationToken)
            {
                Executed = true;
                return Task.CompletedTask;
            }
        }

        public class PongExceptionAction : IRequestExceptionAction<Ping, PongException>
        {
            public bool Executed { get; private set; }

            public Task Execute(Ping request, PongException exception, CancellationToken cancellationToken)
            {
                Executed = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Should_run_all_exception_handlers_that_match_base_type()
        {
            var pingExceptionAction = new PingExceptionAction();
            var pongExceptionAction = new PongExceptionAction();
            var pingPongExceptionAction = new PingPongExceptionAction<Ping>();
            var container = new Container(cfg =>
            {
                cfg.For<IRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IRequestExceptionAction<Ping, PingException>>().Use(_ => pingExceptionAction);
                cfg.For<IRequestExceptionAction<Ping, PingPongException>>().Use(_ => pingPongExceptionAction);
                cfg.For<IRequestExceptionAction<Ping, PongException>>().Use(_ => pongExceptionAction);
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestExceptionActionProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var request = new Ping { Message = "Ping!" };
            await Assert.ThrowsAsync<PingException>(() => mediator.Send(request));

            pingExceptionAction.Executed.ShouldBeTrue();
            pingPongExceptionAction.Executed.ShouldBeTrue();
            pongExceptionAction.Executed.ShouldBeFalse();
        }
    }
}