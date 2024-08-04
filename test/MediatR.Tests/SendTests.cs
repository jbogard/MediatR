using System.Threading;

using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatR.Tests;
public class SendTests
{
    private readonly IServiceProvider _serviceProvider;
    private Dependency _dependency;
    private readonly IMediator _mediator;

    public SendTests()
    {
        _dependency = new Dependency();
        var services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly);
            cfg.RegisterGenericHandlers = true;
        });
        services.AddSingleton(_dependency);
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetService<IMediator>()!;
    }

    public class Ping : IRequest<Pong>
    {
        public string? Message { get; set; }
    }

    public class VoidPing : IRequest
    {
    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Pong { Message = request.Message + " Pong" });
        }
    }

    public class Dependency
    {
        public bool Called { get; set; }
        public bool CalledSpecific { get; set; }
    }

    public class VoidPingHandler : IRequestHandler<VoidPing>
    {
        private readonly Dependency _dependency;

        public VoidPingHandler(Dependency dependency) => _dependency = dependency;

        public Task Handle(VoidPing request, CancellationToken cancellationToken)
        {
            _dependency.Called = true;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_resolve_main_handler()
    {
        var response = await _mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_void_handler()
    {
        await _mediator.Send(new VoidPing());

        _dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_resolve_main_handler_via_dynamic_dispatch()
    {
        object request = new Ping { Message = "Ping" };
        var response = await _mediator.Send(request);

        var pong = response.ShouldBeOfType<Pong>();
        pong.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_void_handler_via_dynamic_dispatch()
    {
        object request = new VoidPing();
        var response = await _mediator.Send(request);

        response.ShouldBeOfType<Unit>();

        _dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_specific_interface()
    {
        var response = await _mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_given_interface()
    {
        // wrap requests in an array, so this test won't break on a 'replace with var' refactoring
        var requests = new IRequest[] { new VoidPing() };
        await _mediator.Send(requests[0]);

        _dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public Task Should_raise_execption_on_null_request() => Should.ThrowAsync<ArgumentNullException>(async () => await _mediator.Send(default!));
   
}