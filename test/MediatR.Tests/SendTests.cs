using System.Threading;

namespace MediatR.Tests;

using System;
using System.Threading.Tasks;
using Shouldly;
using Lamar;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

public class SendTests
{
    private readonly IServiceProvider _serviceProvider;
    private Dependency _dependency;
    private readonly IMediator _mediator;
    
    public SendTests()
    {
        _dependency = new Dependency();
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly));
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

    public class GenericPing<T> : IRequest<T>
        where T : Pong
    {
        public T? Pong { get; set; }
    }

    public class GenericPingHandler<T> : IRequestHandler<GenericPing<T>, T>
        where T : Pong
    {
        private readonly Dependency _dependency;

        public GenericPingHandler(Dependency dependency) => _dependency = dependency;

        public Task<T> Handle(GenericPing<T> request, CancellationToken cancellationToken)
        {
            _dependency.Called = true;
            request.Pong!.Message += " Pong";
            return Task.FromResult(request.Pong!);
        }
    }

    public class VoidGenericPing<T> : IRequest
        where T : Pong
    { }

    public class VoidGenericPingHandler<T> : IRequestHandler<VoidGenericPing<T>>
        where T : Pong
    {
        private readonly Dependency _dependency;
        public VoidGenericPingHandler(Dependency dependency) => _dependency = dependency;

        public Task Handle(VoidGenericPing<T> request, CancellationToken cancellationToken)
        {
            _dependency.Called = true;

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_resolve_main_handler()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_void_handler()
    {
        var dependency = new Dependency();

        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.ForSingletonOf<Dependency>().Use(dependency);
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        await mediator.Send(new VoidPing());

        dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_resolve_main_handler_via_dynamic_dispatch()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        object request = new Ping { Message = "Ping" };
        var response = await mediator.Send(request);

        var pong = response.ShouldBeOfType<Pong>();
        pong.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_void_handler_via_dynamic_dispatch()
    {
        var dependency = new Dependency();

        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.ForSingletonOf<Dependency>().Use(dependency);
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        object request = new VoidPing();
        var response = await mediator.Send(request);

        response.ShouldBeOfType<Unit>();

        dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_specific_interface()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<ISender>().Use<Mediator>();
        });

        var mediator = container.GetInstance<ISender>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_given_interface()
    {
        var dependency = new Dependency();
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<VoidPing>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
            });
            cfg.ForSingletonOf<Dependency>().Use(dependency);
            cfg.For<ISender>().Use<Mediator>();
        });

        var mediator = container.GetInstance<ISender>();

        // wrap requests in an array, so this test won't break on a 'replace with var' refactoring
        var requests = new IRequest[] { new VoidPing() };
        await mediator.Send(requests[0]);

        dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_raise_execption_on_null_request()
    {
        var container = new Container(cfg =>
        {
            cfg.For<ISender>().Use<Mediator>();
        });

        var mediator = container.GetInstance<ISender>();

        await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send(default!));
    }

    [Fact]
    public async Task Should_resolve_generic_handler_by_given_interface()
    {
        var request = new GenericPing<Pong> { Pong = new Pong { Message = "Ping" } };
        var result = await _mediator.Send(request);

        var pong = result.ShouldBeOfType<Pong>();
        pong.Message.ShouldBe("Ping Pong");

        _dependency.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_resolve_generic_void_handler_by_given_interface()
    {
        var request = new VoidGenericPing<Pong>();
        await _mediator.Send(request);

        _dependency.Called.ShouldBeTrue();
    }
}