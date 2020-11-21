namespace MediatR.Tests.Pipeline.Streams
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR.Pipeline.Streams;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class StreamRequestExceptionHandlerTests
    {
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

        public class PingHandler : IStreamRequestHandler<Ping, Pong>
        {
            public async IAsyncEnumerable<Pong> Handle(Ping request, [EnumeratorCancellation]CancellationToken cancellationToken)
            {
                yield return await Task.Run(() => new Pong { Message = request.Message + " Pong" });
                yield return await Task.Run(() => new Pong { Message = request.Message + " Pang" });

                throw new PingException(request.Message + " Oeps an Exception");
            }
        }

        public class PingPongExceptionHandlerForType : IStreamRequestExceptionHandler<Ping, Pong, PingException>
        {
            public Task Handle(Ping request, PingException exception, StreamRequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
            {
                state.SetHandled(new Pong() { Message = exception.Message + " Handled by Type" });

                return Task.CompletedTask;
            }
        }

        public class PingPongExceptionHandler : StreamRequestExceptionHandler<Ping, Pong>
        {
            protected override void Handle(Ping request, Exception exception, StreamRequestExceptionHandlerState<Pong> state)
            {
                state.SetHandled(new Pong() { Message = exception.Message + " Handled"});
            }
        }

        public class PingPongExceptionHandlerNotHandled : StreamRequestExceptionHandler<Ping, Pong>
        {
            protected override void Handle(Ping request, Exception exception, StreamRequestExceptionHandlerState<Pong> state)
            {
                request.Message = exception.Message + " Not Handled";
            }
        }

        [Fact]
        public async Task Should_run_exception_handler_and_allow_for_exception_not_to_throw()
        {
            var container = new Container(cfg =>
            {
                cfg.For<IStreamRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IStreamRequestExceptionHandler<Ping, Pong, Exception>>().Use<PingPongExceptionHandler>();
                cfg.For<IStreamRequestExceptionHandler<Ping, Pong, PingException>>().Use<PingPongExceptionHandlerForType>();
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestExceptionProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var responses = mediator.CreateStream(new Ping { Message = "Ping" });

            int i = 0;
            await foreach (var response in responses)
            {
                if (i == 0)
                {
                    response.Message.ShouldBe("Ping Pong");
                }
                else if (i == 1)
                {
                    response.Message.ShouldBe("Ping Pang");
                }
                else if (i == 2)
                {
                    response.Message.ShouldBe("Ping Oeps an Exception Thrown Handled by Type");
                }

                (++i).ShouldBeLessThanOrEqualTo(3);
            }
        }

        [Fact]
        public async Task Should_run_exception_handler_and_allow_for_exception_to_be_still_thrown()
        {
            var container = new Container(cfg =>
            {
                cfg.For<IStreamRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IStreamRequestExceptionHandler<Ping, Pong, Exception>>().Use<PingPongExceptionHandlerNotHandled>();
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestExceptionProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var request = new Ping { Message = "Ping" };
            await Should.ThrowAsync<PingException>(async () =>
            {
                int i = 0;
                await foreach (var response in mediator.CreateStream(request))
                {
                    if (i == 0)
                    {
                        response.Message.ShouldBe("Ping Pong");
                    }
                    else if (i == 1)
                    {
                        response.Message.ShouldBe("Ping Pang");
                    }
                    else if (i == 2)
                    {
                        response.Message.ShouldBe("Ping Oeps an Exception Thrown Handled by Type");
                    }

                    (++i).ShouldBeLessThanOrEqualTo(3);
                }
            });
        }

    }
}