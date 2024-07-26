using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.MicrosoftExtensionsDI;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Should_throw_for_add_mediatr_with_action_when_services_is_null()
    {
        IServiceCollection services = null!;
        Action<MediatRServiceConfiguration> configuration = (_) => { };

        var exception = Should.Throw<ArgumentNullException>(
            () => services.AddMediatR(configuration));

        Assert.Equal(nameof(services), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_with_action_when_configuration_is_null()
    {
        var services = new ServiceCollection();
        Action<MediatRServiceConfiguration> configuration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => services.AddMediatR(configuration));

        Assert.Equal(nameof(configuration), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_when_services_is_null()
    {
        IServiceCollection services = null!;
        var configuration = new MediatRServiceConfiguration();

        var exception = Should.Throw<ArgumentNullException>(
            () => services.AddMediatR(configuration));

        Assert.Equal(nameof(services), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_when_configuration_is_null()
    {
        var services = new ServiceCollection();
        MediatRServiceConfiguration configuration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => services.AddMediatR(configuration));

        Assert.Equal(nameof(configuration), exception.ParamName);
    }
}