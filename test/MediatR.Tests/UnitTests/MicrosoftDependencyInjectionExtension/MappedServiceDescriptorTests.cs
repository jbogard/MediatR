using System;
using FluentAssertions;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.UnitTests.MicrosoftDependencyInjectionExtension;

public sealed class MappedServiceDescriptorTests
{
    [Fact]
    public void CreateNewMappingServiceDescriptor_WithTheInstanceConstructor_ThrowsNotSupportedException()
    {
        // Arrange
        var testInstance = new object();

        // Act
        var act = () => new MappedServiceDescriptor(typeof(MappedServiceDescriptorTests), testInstance);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void CreateNewMappingServiceDescriptorWithImplementationType_GetImplementationType_ReturnsImplementationType()
    {
        // Arrange
        var implementationType = typeof(MappedServiceDescriptorTests);
        var mappedServiceDescriptor = new MappedServiceDescriptor(typeof(MappedServiceDescriptor), implementationType, ServiceLifetime.Singleton);

        // Act
        var result = mappedServiceDescriptor.ImplementationType;

        // Assert
        result.Should().Be(implementationType);
    }
}