using System.Threading;

namespace MediatR.Tests;

using System;
using System.Threading.Tasks;
using Shouldly;
using StructureMap;
using Xunit;

public class SendTests
{

    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class Pong
    {
        public string Message { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Pong { Message = request.Message + " Pong" });
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
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
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
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        object request = new Ping { Message = "Ping" };
        var response = await mediator.Send(request);

        var pong = response.ShouldBeOfType<Pong>();
        pong.Message.ShouldBe("Ping Pong");
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
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            cfg.For<ISender>().Use<Mediator>();
        });

        var mediator = container.GetInstance<ISender>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");
    }


    [Fact]
    public async Task Should_raise_execption_on_null_request()
    {
        var container = new Container(cfg =>
        {
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            cfg.For<ISender>().Use<Mediator>();
        });

        var mediator = container.GetInstance<ISender>();

        await Should.ThrowAsync<ArgumentNullException>(async () => await mediator.Send(null));
    }
}