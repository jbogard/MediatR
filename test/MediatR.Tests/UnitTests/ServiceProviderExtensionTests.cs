using FluentAssertions;
using MediatR.Abstraction;
using Moq;
using NUnit.Framework;

namespace MediatR.Tests.UnitTests;

[TestFixture]
internal sealed class ServiceProviderExtensionTests
{
    [Test]
    public void ServiceProvider_GetNotRegisteredService_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = Mock.Of<IServiceProvider>();
        
        // Act
        var act = () => ServiceProviderExtension.GetRequiredService<IMediator>(serviceProvider);
        
        // Assert
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"*{typeof(IMediator)}*");
    }
}