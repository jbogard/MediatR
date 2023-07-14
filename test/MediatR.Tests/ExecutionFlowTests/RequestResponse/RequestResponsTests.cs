using System.Diagnostics;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MediatR.Tests.ExecutionFlowTests;

[TestFixture]
internal sealed class RequestResponsTests
{
    [Test]
    public async Task PublishRequest_WithDefaultRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new RequestResponse();
        
        // Act
        var response = await mediator.Send<RequestResponse, Response>(request);
        
        // Assert
        var handler = provider.GetRequiredService<RequestResponseHandler>();

        handler.FinalClassCalls.Should().Be(1); // Was called once by the final class types.
        response.Should().NotBeNull(); // Must returns a default response.
        response.Should().BeOfType<Response>();
    }
    
    [Test]
    public async Task PublishBaseRequest_WithDefaultRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new RequestResponse();
        
        // Act
        var response = await mediator.Send<RequestResponseBase, BaseResponse>(request);
        
        // Assert
        var handler = provider.GetRequiredService<RequestResponseHandler>();

        handler.BaseClassCalls.Should().Be(1); // Was called once by the final class types.
        response.Should().NotBeNull(); // Must returns a default response.
        response.Should().BeOfType<Response>(); // It is the final type of the base response.
    }

    [Test]
    public async Task PublishRequest_HandlersFailsWithInvalidOperationException_ExceptionGetActedAndHandled()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Expected Exception");
        
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl, RequestExceptionActionProcessorStrategy.ApplyForAllExceptions).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };
        
        // Act
        var response = await mediator.Send<ThrowingExceptionRequest, Response>(request);
        
        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(1);
        action.GeneralExceptionActionCalls.Should().Be(1);
        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(1);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(0); // It was already handled by the invalid operation exception handler.
        response.Should().NotBeNull();
    }
    
    [Test]
    public async Task PublishRequest_HandlersFailsWithUnreachableException_ExceptionGetActedAndHandled()
    {
        // Arrange
        var expectedException = new UnreachableException("Expected Exception");
        
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl, RequestExceptionActionProcessorStrategy.ApplyForAllExceptions).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };
        
        // Act
        var response = await mediator.Send<ThrowingExceptionRequest, Response>(request);
        
        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(1);
        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(0);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(1);
        response.Should().NotBeNull();
    }
    
    [Test]
    public async Task PublishRequestWithOnlyActedOnNotHandledException_HandlersFailsWithInvalidOperationException_ExceptionGetActedAndHandled()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Expected Exception");
        
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl, RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };
        
        // Act
        var response = await mediator.Send<ThrowingExceptionRequest, Response>(request);
        
        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(0);
        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(1);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(0);
        response.Should().NotBeNull();
    }
    
    [Test]
    public async Task PublishRequestWithOnlyActedOnNotHandledException_HandlersFailsWithUnreachableException_ExceptionGetActedAndHandled()
    {
        // Arrange
        var expectedException = new UnreachableException("Expected Exception");
        
        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl, RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };
        
        // Act
        var response = await mediator.Send<ThrowingExceptionRequest, Response>(request);
        
        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(0);
        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(0);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(1);
        response.Should().NotBeNull();
    }

    [Test]
    public async Task PublishRequest_HandlerFailsWithAccessViolationException_ExceptionGetsAcedOnAndHandled()
    {
        // Arrange
        var expectedException = new AccessViolationException("Expected Exception");

        var serviceColl = new ServiceCollection();
        var provider = ConfigureMediatR(serviceColl).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new AccessViolationRequest
        {
            Exception = expectedException
        };
        
        // Act
        var response = await mediator.Send<AccessViolationRequest, Response>(request);
        
        // Assert
        var action = provider.GetRequiredService<RequestResponseAction>();
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();

        var openGenericAction = provider.GetRequiredService<OpenGenericRequestResponseAction<AccessViolationRequest, Response>>();
        var openGenericHandler = provider.GetRequiredService<OpenGenericAccessViolationExceptionHandler<AccessViolationRequest, Response>>();

        action.GeneralExceptionActionCalls.Should().Be(0);
        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(0);
        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(0);
        openGenericAction.Calls.Should().Be(1);
        openGenericHandler.Calls.Should().Be(1);
        response.Should().NotBeNull();
    }

    private static IServiceCollection ConfigureMediatR(IServiceCollection serviceCollection, RequestExceptionActionProcessorStrategy strategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions) =>
        serviceCollection.ConfigureMediatR(c =>
        {
            c.RegistrationOptions = RegistrationOptions.SingletonAndMapped;
            c.RequestExceptionActionProcessorStrategy = strategy;
            c.RegisterServicesFromAssemblyContaining<RequestResponsTests>();
        });
}