using System;
using FluentAssertions;
using MediatR.Abstraction;
using Moq;
using Xunit;

namespace MediatR.UnitTests;

public sealed class ServiceProviderExtensionTests
{
    [Fact]
    public void ServiceProvider_GetNotRegisteredService_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>();

        // Act
        var act = () => serviceProvider.GetRequiredService<IMediator>();
    
        // Assert
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"*{typeof(IMediator)}*");
    }

    [Fact]
    public void ServiceProvider_GetMultipleServicesWithoutRegisteredServices_ReturnsEmptyArray()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>();

        // Act
        var services = serviceProvider.GetServices<ServiceProviderExtensionTests>();

        // Assert
        services.Should().BeEmpty();
    }
}