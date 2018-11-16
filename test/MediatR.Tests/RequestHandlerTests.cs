using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace MediatR.Tests
{
    public class RequestHandlerTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : RequestHandler<Ping, Pong>
        {
            protected override Pong Handle(Ping request)
            {
                return new Pong { Message = request.Message + " Pong" };
            }
        }

        [Fact]
        public async Task Should_call_abstract_handler()
        {
            IRequestHandler<Ping, Pong> handler = new PingHandler();

            var response = await handler.Handle(new Ping() { Message = "Ping" }, default);

            response.Message.ShouldBe("Ping Pong");
        }
    }
}
