using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests;

using System;
using System.Linq;
using System.Reflection;
using MediatR.Pipeline;
using Shouldly;
using Xunit;

public class TypeResolutionTests
{
    private readonly IServiceProvider _provider;

    public TypeResolutionTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Ping)));
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void ShouldResolveMediator()
    {
        _provider.GetService<IMediator>().ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolveSender()
    {
        _provider.GetService<ISender>().ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolvePublisher()
    {
        _provider.GetService<IPublisher>().ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolveRequestHandler()
    {
        _provider.GetService<IRequestHandler<Ping, Pong>>().ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolveVoidRequestHandler()
    {
        _provider.GetService<IRequestHandler<Ding>>().ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolveNotificationHandlers()
    {
        _provider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldNotThrowWithMissingEnumerables()
    {
        Should.NotThrow(() => _provider.GetRequiredService<IEnumerable<IRequestExceptionAction<int, Exception>>>());
    }

    [Fact]
    public void ShouldResolveFirstDuplicateHandler()
    {
        _provider.GetService<IRequestHandler<DuplicateTest, string>>().ShouldNotBeNull();
        _provider.GetService<IRequestHandler<DuplicateTest, string>>()
            .ShouldBeAssignableTo<DuplicateHandler1>();
    }

    [Fact]
    public void ShouldResolveIgnoreSecondDuplicateHandler()
    {
        _provider.GetServices<IRequestHandler<DuplicateTest, string>>().Count().ShouldBe(1);
    }
}