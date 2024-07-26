using Lamar;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests;

public class MediatorTests
{
    private class Ping : IRequest<Pong> { }

    private class Pong { }

    private class NullPing : IRequest<Pong> { }

    private class VoidNullPing : IRequest { }

    private class NullPinged : INotification { }

    [Fact]
    public void Should_throw_for_ctor_mediator_when_service_provider_is_null()
    {
        IServiceProvider serviceProvider = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => new Mediator(serviceProvider));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_overload_ctor_mediator_with_notification_when_service_provider_is_null()
    {
        IServiceProvider serviceProvider = null!;
        INotificationPublisher publisher = new ForeachAwaitPublisher();

        var exception = Should.Throw<ArgumentNullException>(
            () => new Mediator(serviceProvider, publisher));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_overload_ctor_mediator_with_notification_when_publisher_is_null()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        INotificationPublisher publisher = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => new Mediator(serviceProvider, publisher));

        Assert.Equal(nameof(publisher), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_send_when_request_is_null()
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
            cfg.For<IMediator>().Use<Mediator>();
        });
        var mediator = container.GetInstance<IMediator>();

        NullPing request = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await mediator.Send(request));
        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_void_send_when_request_is_null()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(VoidNullPing));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });
        var mediator = container.GetInstance<IMediator>();

        VoidNullPing request = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await mediator.Send(request));
        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_publish_when_request_is_null()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(NullPinged));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });
        var mediator = container.GetInstance<IMediator>();

        NullPinged notification = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await mediator.Publish(notification));
        Assert.Equal(nameof(notification), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_publish_when_request_is_null_object()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(NullPinged));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });
        var mediator = container.GetInstance<IMediator>();

        object notification = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await mediator.Publish(notification));
        Assert.Equal(nameof(notification), exception.ParamName);
    }
}