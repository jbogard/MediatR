namespace MediatR.Tests
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class ExceptionTests
    {
        private readonly IMediator _mediator;

        public class Ping : IRequest<Pong>
        {
        }

        public class Pong
        {
        }

        public class VoidPing : IRequest
        {
        }

        public class Pinged : INotification
        {
        }

        public class AsyncPing : IRequest<Pong>
        {
        }

        public class AsyncVoidPing : IRequest
        {
        }

        public class AsyncPinged : INotification
        {
        }

        public ExceptionTests()
        {
            var container = new Container(cfg =>
            {
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });
            _mediator = container.GetInstance<IMediator>();
        }

        [Fact]
        public async Task Should_throw_for_send()
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => await _mediator.SendAsync(new Ping()));
        }

        [Fact]
        public async Task Should_throw_for_void_send()
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => await _mediator.SendAsync(new VoidPing()));
        }

        [Fact]
        public async Task Should_not_throw_for_publish()
        {
            Exception ex = null;
            try
            {
                await _mediator.PublishAsync(new Pinged());
            }
            catch (Exception e)
            {
                ex = e;
            }
            ex.ShouldBeNull();
        }

        [Fact]
        public async Task Should_throw_for_async_send()
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => await _mediator.SendAsync(new AsyncPing()));
        }

        [Fact]
        public async Task Should_throw_for_async_void_send()
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => await _mediator.SendAsync(new AsyncVoidPing()));
        }

        [Fact]
        public async Task Should_not_throw_for_async_publish()
        {
            Exception ex = null;
            try
            {
                await _mediator.PublishAsync(new AsyncPinged());
            }
            catch (Exception e)
            {
                ex = e;
            }
            ex.ShouldBeNull();
        }
    }
}