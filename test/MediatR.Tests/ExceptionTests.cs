namespace MediatR.Tests
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;

    public class ExceptionTests
    {
        private readonly IMediator _mediator;

        public class Ping : IRequest<Pong> { }
        public class Pong {}
        public class VoidPing : IRequest { }
        public class Pinged : INotification { }
        public class AsyncPing : IAsyncRequest<Pong> { }
        public class AsyncVoidPing : IAsyncRequest { }
        public class AsyncPinged : IAsyncNotification { }

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

        public void Should_throw_for_send()
        {
            Should.Throw<InvalidOperationException>(() => _mediator.Send(new Ping()));
        }

        public void Should_throw_for_void_send()
        {
            Should.Throw<InvalidOperationException>(() => _mediator.Send(new VoidPing()));
        }

        public void Should_not_throw_for_publish()
        {
            Should.NotThrow(() => _mediator.Publish(new Pinged()));
        }

        public void Should_throw_for_async_send()
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                var response = _mediator.SendAsync(new AsyncPing());
                Task.WaitAll(response);
                return response;
            });
        }

        public void Should_throw_for_async_void_send()
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                var response = _mediator.SendAsync(new AsyncVoidPing());
                Task.WaitAll(response);
                return response;
            });
        }

        public void Should_not_throw_for_async_publish()
        {
            Should.NotThrow(() =>
            {
                var response = _mediator.PublishAsync(new AsyncPinged());
                Task.WaitAll(response);
                return response;
            });
        }
    }
}