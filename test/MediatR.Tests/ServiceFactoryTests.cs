using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests
{
    public class ServiceFactoryTests
    {
        public class Ping : IRequest<Pong>
        {

        }

        public class Pong
        {
            public string Message { get; set; }
        }

        [Fact]
        public async Task Should_throw_given_no_handler()
        {
            var serviceFactory = new ServiceFactory(type =>
                typeof(IEnumerable).IsAssignableFrom(type)
                    ? Array.CreateInstance(type.GetGenericArguments().First(), 0)
                    : null);

            var mediator = new Mediator(serviceFactory);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => mediator.Send(new Ping())
            );

            Assert.StartsWith("Handler was not found for request", exception.Message);
        }
    }
}
