using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace MediatR.Tests
{
    public class RequestHandlerUnitTests
    {
        public class Ping : IRequest
        {
            public string Message { get; set; }
        }

        public class PingHandler : RequestHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PingHandler(TextWriter writer)
            {
                _writer = writer;
            }

            protected override void Handle(Ping request)
            {
                _writer.WriteLine(request.Message + " Pong");
            }
        }

        [Fact]
        public async Task Should_call_abstract_unit_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            IRequestHandler<Ping, Unit> handler = new PingHandler(writer);

            await handler.Handle(new Ping() { Message = "Ping" }, default);

            var result = builder.ToString();
            result.ShouldContain("Ping Pong");
        }
    }
}
