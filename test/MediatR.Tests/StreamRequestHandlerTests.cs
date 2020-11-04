using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests
{
    public class StreamRequestHandlerTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : StreamRequestHandler<Ping, Pong>
        {
            protected override async Task<Pong> HandleAsync(Ping request)
            {
                return await Task<Pong>.Run(() => new Pong { Message = request.Message + " Pang" });
            }
        }

        [Fact]
        public async Task Should_call_abstract_handler()
        {
            IStreamRequestHandler<Ping, Pong> handler = new PingHandler();

            int i = 0;
            await foreach (Pong result in handler.HandleAsync(new Ping() { Message = "Ping" }, default))
            {
                if (i == 0)
                {
                    result.Message.ShouldBe("Ping Pang");
                }

                i++;
            }

            i.ShouldBe(1);
        }
    }
}
