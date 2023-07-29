using System;
using FluentAssertions;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using Xunit;

namespace MediatR.UnitTests;

public sealed class MediatRServiceConfigurationTests
{
    [Fact]
    public void MediatRServiceConfiguration_WithEnabledCachingOfHandlersAndWithoutSingleHandlers_ValidationThrowsInvalidOperationException()
    {
        // Arrange
        var config = new DumyConfiguration
        {
            AssembliesToRegister = { new AssemblyConfiguration(typeof(MediatRServiceConfiguration).Assembly, AssemblyScannerOptions.All) },
            RegistrationStyle = RegistrationStyle.EachServiceOneInstance,
            EnableCachingOfHandlers = true
        };
        
        // Act
        var act = () => config.Validate();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("Caching is only possible if the handlers are registered as singletons. Currently they are '*' and that could break the application. Either set the option to '*' or disable the caching of the handlers.");
    }

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