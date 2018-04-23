namespace MediatR.Tests
{
    using System;
    using System.Threading;
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

        public class NullPing : IRequest<Pong>
        {
        }

        public class VoidNullPing : IRequest
        {
        }

        public class NullPinged : INotification
        {
        }

        public class NullPingHandler : IRequestHandler<NullPing, Pong>
        {
            public Task<Pong> Handle(NullPing request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong());
            }
        }

        public class VoidNullPingHandler : IRequestHandler<VoidNullPing, Unit>
        {
            public Task<Unit> Handle(VoidNullPing request, CancellationToken cancellationToken)
            {
                return Unit.Task;
            }
        }

        public ExceptionTests()
        {
            var container = new Container();
            _mediator = new Mediator(container.GetInstance);
        }

        [Fact]
        public async Task Should_throw_for_send()
        {
            await Should.ThrowAsync<StructureMapConfigurationException>(async () => await _mediator.Send(new Ping()));
        }

        [Fact]
        public async Task Should_throw_for_void_send()
        {
            await Should.ThrowAsync<StructureMapConfigurationException>(async () => await _mediator.Send(new VoidPing()));
        }

        [Fact]
        public async Task Should_not_throw_for_publish()
        {
            Exception ex = null;
            try
            {
                await _mediator.Publish(new Pinged());
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
            await Should.ThrowAsync<StructureMapConfigurationException>(async () => await _mediator.Send(new AsyncPing()));
        }

        [Fact]
        public async Task Should_throw_for_async_void_send()
        {
            await Should.ThrowAsync<StructureMapConfigurationException>(async () => await _mediator.Send(new AsyncVoidPing()));
        }

        [Fact]
        public async Task Should_not_throw_for_async_publish()
        {
            Exception ex = null;
            try
            {
                await _mediator.Publish(new AsyncPinged());
            }
            catch (Exception e)
            {
                ex = e;
            }
            ex.ShouldBeNull();
        }

        [Fact]
        public async Task Should_throw_argument_exception_for_send_when_request_is_null()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(NullPing));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
            });

            var mediator = new Mediator(container.GetInstance);

            NullPing request = null;

            await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send(request));
        }

        [Fact]
        public async Task Should_throw_argument_exception_for_void_send_when_request_is_null()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(VoidNullPing));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                });
            });

            var mediator = new Mediator(container.GetInstance);

            VoidNullPing request = null;

            await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send(request));
        }

        [Fact]
        public async Task Should_throw_argument_exception_for_publish_when_notification_is_null()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(NullPinged));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                });
            });

            var mediator = new Mediator(container.GetInstance);

            NullPinged notification = null;

            await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Publish(notification));
        }
    }
}