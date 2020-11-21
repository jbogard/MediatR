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

    public class StreamRequestExceptionActionProcessorTests
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

        public class PingPongExceptionActionForType : StreamRequestExceptionAction<Ping, PingException>
        {
            protected override void Execute(Ping request, PingException exception)
            {
                // What here?
            }
        }

        public class PingPongExceptionAction : StreamRequestExceptionAction<Ping, Exception>
        {
            protected override void Execute(Ping request, Exception exception)
            {
                // What here?
            }
        }


        [Fact]
        public async Task Should_run_exception_handler_and_run_action()
        {
            var container = new Container(cfg =>
            {
                cfg.For<IStreamRequestHandler<Ping, Pong>>().Use<PingHandler>();
                cfg.For<IStreamRequestExceptionAction<Ping, Exception>>().Use<PingPongExceptionAction>();
                cfg.For<IStreamRequestExceptionAction<Ping, PingException>>().Use<PingPongExceptionActionForType>();
                cfg.For(typeof(IStreamPipelineBehavior<,>)).Add(typeof(StreamRequestExceptionActionProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var responses = mediator.CreateStream(new Ping { Message = "Ping" });

            int i = 0;
            try
            {
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
                        false.ShouldBeTrue("Should never reach here!");
                    }

                    (++i).ShouldBeLessThanOrEqualTo(2);
                }
            }
            catch ( Exception ex )
            {
                ex.Message.ShouldBe("Ping Oeps an Exception Thrown");
            }
        }

     }
}