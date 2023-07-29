using System;
using FluentAssertions;
using MediatR.DependencyInjection.Configuration;
using Xunit;

namespace MediatR.UnitTests;

public sealed class TypeRegistrarTests
{
    private readonly TypeRegistrar _sut = new(typeof(ITestServiceInterface<>), false);

    [Fact]
    public void DefaultTypeRegistrar_AddsWithWrongImplementation_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var act = () => _sut.Add(typeof(ITestServiceInterface<int>), typeof(TypeRegistrarTests));

        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Service Type '*' or implementing Type '*' must inherit from any covariant of Type '*' to be registered.");
    }

    [Fact]
    public void DefaultTypeRegistrar_AddOpenGenericWithNoRelevantImplementation_ThrowsInvalidOperationException()
    {
        // Arrange
        
        // Act
        var act = () => _sut.AddOpenGeneric(typeof(TestImplementationClass<>));
        
        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Type '*' must implement '*' to be registered.");
    }

    private sealed class TestImplementationClass<TGeneric>
    {
    }

    private interface ITestServiceInterface<TGeneric>
    {
    }
}