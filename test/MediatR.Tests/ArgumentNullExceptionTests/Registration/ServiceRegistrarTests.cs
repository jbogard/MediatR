using MediatR.Registration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Registration;

public class ServiceRegistrarTests
{
    [Fact]
    public void Should_throw_for_set_generic_request_handler_registration_limitations_when_configuration_is_null()
    {
        MediatRServiceConfiguration configuration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.SetGenericRequestHandlerRegistrationLimitations(configuration));

        Assert.Equal(nameof(configuration), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_classes_with_timeout_when_services_is_null()
    {
        var configuration = new MediatRServiceConfiguration();
        IServiceCollection services = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddMediatRClassesWithTimeout(services, configuration));

        Assert.Equal(nameof(services), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_classes_with_timeout_when_configuration_is_null()
    {
        var services = new ServiceCollection();
        MediatRServiceConfiguration configuration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddMediatRClassesWithTimeout(services, configuration));

        Assert.Equal(nameof(configuration), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_classes_when_services_is_null()
    {
        IServiceCollection services = null!;
        var configuration = new MediatRServiceConfiguration();

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddMediatRClasses(services, configuration, default(CancellationToken)));

        Assert.Equal(nameof(services), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_mediatr_classes_when_configuration_is_null()
    {
        var services = new ServiceCollection();
        MediatRServiceConfiguration configuration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddMediatRClasses(services, configuration, default(CancellationToken)));

        Assert.Equal(nameof(configuration), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_generate_combinations_when_request_type_is_null()
    {
        Type requestType = null!;
        var lists = new List<List<Type>>();

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.GenerateCombinations(requestType, lists, 0, default(CancellationToken)));

        Assert.Equal(nameof(requestType), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_generate_combinations_when_request_lists_is_null()
    {
        Type requestType = typeof(string);
        List<List<Type>> lists = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.GenerateCombinations(requestType, lists, 0, default(CancellationToken)));

        Assert.Equal(nameof(lists), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_required_services_when_services_is_null()
    {
        IServiceCollection services = null!;
        var configuration = new MediatRServiceConfiguration();

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddRequiredServices(services, configuration));

        Assert.Equal(nameof(services), exception.ParamName);
    }

    [Fact]
    public void Should_throw_for_add_required_services_when_service_configuration_is_null()
    {
        var services = new ServiceCollection();
        MediatRServiceConfiguration serviceConfiguration = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => ServiceRegistrar.AddRequiredServices(services, serviceConfiguration));

        Assert.Equal(nameof(serviceConfiguration), exception.ParamName);
    }
}