using System;
using FluentAssertions;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MediatR.UnitTests.MicrosoftDependencyInjectionExtension;

public sealed class ServiceRegistrarAdapterTests
{
    private readonly ServiceCollection _serviceCollection;
    private readonly ServiceCollectionAdapter _sut;

    public ServiceRegistrarAdapterTests()
    {
        _serviceCollection = new ServiceCollection();
        _sut = new ServiceCollectionAdapter(_serviceCollection, new ServiceCollectionConfiguration
        {
            DefaultServiceLifetime = ServiceLifetime.Singleton
        });
    }

    [Fact]
    public void ServiceCollectionAdapterRegistersOnlyOnceService_RegistrationExists_ServicesDoesNotGetRegistered()
    {
        // Arrange
        var registrationType = typeof(ServiceRegistrarAdapterTests);
        _serviceCollection.AddSingleton(registrationType);

        // Act
        _sut.RegisterOnlyOnce(registrationType, registrationType);
        
        // Assert
        _serviceCollection.Should().HaveCount(1).And
            .ContainSingle(d => d.ServiceType == registrationType &&
                                d.ImplementationType == registrationType &&
                                d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void ServiceCollectionAdapterRegisterOpenGenericImplementation_ServiceTypeHasDifferentArityThenImplementationType_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var act = () => _sut.RegisterOpenGeneric(typeof(IServiceInterface<,>), typeof(OpenGenericImplementation<>));

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("The service type '*' must have the same generic arguments length as the implementation type '*'.");
    }

    [Fact]
    public void ServiceCollectionAdapterRegistersInstanceService_RegistrationExists_ServiceDoesNotGetRegistered()
    {
        // Arrange
        var implementationObject = new object();
        _serviceCollection.AddSingleton(implementationObject);

        // Act
        _sut.RegisterOnlyOnce(implementationObject.GetType(), implementationObject.GetType());

        // Assert
        _serviceCollection.Should().HaveCount(1).And
            .ContainSingle(d => d.ServiceType == implementationObject.GetType() &&
                                d.ImplementationInstance == implementationObject &&
                                d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void ServiceCollectionRegistersMapping_MappingHasAlreadyBeenRegistered_ThrowsNotSupportedException()
    {
        // Arrange
        var implementationType = typeof(OpenGenericImplementation<,>);
        var serviceType = typeof(IServiceInterface<,>);
        _serviceCollection.Add(new MappedServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton));

        // Act
        var act = () => _sut.RegisterOpenGenericMapping(serviceType, implementationType);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>().WithMessage("Service Collection does not support mapping a single open generic instance to more then one open generic service types. Can not register Implementation Type '*' for Service Type '*'. Consider implementing '*' in a new class to resolve this exception.");
    }
    
    private sealed class OpenGenericImplementation<TGeneric, TFixGeneric> : IServiceInterface<TGeneric, TFixGeneric>
    {
    }
    
    private sealed class OpenGenericImplementation<TGeneric> : IServiceInterface<TGeneric, Exception>
    {
    }

    private interface IServiceInterface<TGeneric, TFixGeneric>
    {
    }
}