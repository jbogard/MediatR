using System;
using FluentAssertions;
using MediatR.DependencyInjection;
using MediatR.DependencyInjection.Configuration;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.UnitTests.MicrosoftDependencyInjectionExtension;

public sealed class ServiceCollectionConfigurationTests
{
    [Fact]
    public void MediatRServiceConfiguration_WithEnabledCachingOfHandlersAndWithoutSingleHandlers_ValidationThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ServiceCollectionConfiguration
        {
            AssembliesToRegister = { new AssemblyConfiguration(typeof(MediatRServiceConfiguration).Assembly, AssemblyScannerOptions.All) },
            RegistrationStyle = RegistrationStyle.EachServiceOneInstance,
            DefaultServiceLifetime = ServiceLifetime.Singleton,
            EnableCachingOfHandlers = true
        };
        
        // Act
        var act = () => config.Validate();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("Caching is only possible if the handlers are registered as singletons. Currently they are '*' and that could break the application. Either set the option to '*' or disable the caching of the handlers.");
    }

    [Fact]
    public void MediatRServiceConfiguration_WithEnabledCachingOfHandlersAndWithSingleHandlers_ValidationThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ServiceCollectionConfiguration
        {
            AssembliesToRegister = { new AssemblyConfiguration(typeof(MediatRServiceConfiguration).Assembly, AssemblyScannerOptions.All) },
            RegistrationStyle = RegistrationStyle.OneInstanceForeachService,
            EnableCachingOfHandlers = true
        };
        
        // Act
        var act = () => config.Validate();

        act.Should().NotThrow<InvalidOperationException>();
    }
}