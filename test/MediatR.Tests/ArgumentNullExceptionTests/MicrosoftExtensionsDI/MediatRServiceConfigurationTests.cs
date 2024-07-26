using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Reflection;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.MicrosoftExtensionsDI;

public class MediatRServiceConfigurationTests
{
    private readonly MediatRServiceConfiguration _configuration = new();

    [Fact]
    public void Should_throw_for_register_services_from_assembly_containing_when_type_is_null()
    {
        Type type = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.RegisterServicesFromAssemblyContaining(type));

        Assert.Equal(nameof(type), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_register_services_from_assembly_when_assembly_is_null()
    {
        Assembly assembly = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.RegisterServicesFromAssembly(assembly));

        Assert.Equal(nameof(assembly), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_register_services_from_assemblies_when_assemblies_is_null()
    {
        Assembly[] assemblies = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.RegisterServicesFromAssemblies(assemblies));

        Assert.Equal(nameof(assemblies), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_behavior_when_type_is_null()
    {
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddBehavior(implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_behavior_with_two_type_when_service_type_is_null()
    {
        Type serviceType = null!;
        var implementationType = typeof(string);

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddBehavior(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(serviceType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_behavior_with_two_type_when_implementation_type_is_null()
    {
        var serviceType = typeof(string);
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddBehavior(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_open_behavior_when_open_behavior_type_is_null()
    {
        Type openBehaviorType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddOpenBehavior(openBehaviorType, default(ServiceLifetime)));

        Assert.Equal(nameof(openBehaviorType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_stream_behavior_with_one_type_when_implementation_type_is_null()
    {
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddStreamBehavior(implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_stream_behavior_with_two_type_when_service_type_is_null()
    {
        Type serviceType = null!;
        var implementationType = typeof(string);

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddStreamBehavior(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(serviceType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_stream_behavior_with_two_type_when_implementation_type_is_null()
    {
        var serviceType = typeof(string);
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddStreamBehavior(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_open_stream_behavior_with_two_type_when_open_behavior_type_is_null()
    {
        Type openBehaviorType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddOpenStreamBehavior(openBehaviorType, default(ServiceLifetime)));

        Assert.Equal(nameof(openBehaviorType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_pre_processor_with_one_type_when_implementation_type_is_null()
    {
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPreProcessor(implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_pre_processor_with_two_type_when_service_type_is_null()
    {
        Type serviceType = null!;
        var implementationType = typeof(string);

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPreProcessor(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(serviceType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_pre_processor_with_two_type_when_implementation_type_is_null()
    {
        var serviceType = typeof(string);
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPreProcessor(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_open_request_pre_processor_when_open_behavior_type_is_null()
    {
        Type openBehaviorType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddOpenRequestPreProcessor(openBehaviorType, default(ServiceLifetime)));

        Assert.Equal(nameof(openBehaviorType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_post_processor_with_one_type_when_implementation_type_is_null()
    {
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPostProcessor(implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_post_processor_with_two_type_when_service_type_is_null()
    {
        Type serviceType = null!;
        var implementationType = typeof(string);

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPostProcessor(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(serviceType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_request_post_processor_with_two_type_when_implementation_type_is_null()
    {
        var serviceType = typeof(string);
        Type implementationType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddRequestPostProcessor(serviceType, implementationType, default(ServiceLifetime)));

        Assert.Equal(nameof(implementationType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_open_request_post_processor_when_open_behavior_type_is_null()
    {
        Type openBehaviorType = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => _configuration.AddOpenRequestPostProcessor(openBehaviorType, default(ServiceLifetime)));

        Assert.Equal(nameof(openBehaviorType), exception.ParamName);
    }
}