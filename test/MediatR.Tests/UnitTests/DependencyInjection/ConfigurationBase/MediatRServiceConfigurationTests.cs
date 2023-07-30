using System;
using FluentAssertions;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using Xunit;

namespace MediatR.UnitTests;

public sealed class MediatRServiceConfigurationTests
{
    [Fact]
    public void MediatRServiceConfiguration_WithNoAssembliesToScan_ValidationThrowsArgumentException()
    {
        // Arrange
        var config = new DumyConfiguration();

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
    }
    
    private sealed class DumyConfiguration : MediatRServiceConfiguration
    {
    }
}