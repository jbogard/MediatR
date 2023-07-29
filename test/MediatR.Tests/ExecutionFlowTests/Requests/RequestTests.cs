using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Processors;
using MediatR.DependencyInjection.Configuration;
using MediatR.ExecutionFlowTests.Requests.ExceptionActions;
using MediatR.ExecutionFlowTests.Requests.ExceptionHandlers;
using MediatR.ExecutionFlowTests.Requests.Handlers;
using MediatR.ExecutionFlowTests.Requests.Pipelines;
using MediatR.ExecutionFlowTests.Requests.Processors;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.ExecutionFlowTests.Requests;

public sealed class RequestTests : IDisposable
{
    #region HandlerTests

    [Fact]
    public async Task PublishRequest_WithOneInstancePerServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var handler = (RequestHandler)provider.GetRequiredService<IRequestHandler<Request>>();

        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task PublishRequestWithCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.SendAsync(request);
        await mediator.SendAsync(request);

        // Assert
        var handler = provider.GetRequiredService<RequestHandler>();

        handler.Calls.Should().Be(2);
    }

    [Fact]
    public async Task PublishRequestWithoutCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var handler = provider.GetRequiredService<RequestHandler>();

        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task PublishBaseRequest_WithOneInstanceForeachServiceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.MappingLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync<RequestBase>(request);

        // Assert
        var handler = provider.GetRequiredService<BaseRequestHandler>();

        handler.Calls.Should().Be(1);
    }
    
    [Fact]
    public async Task PublishBaseRequest_WithEachServiceOnceInstanceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync<RequestBase>(request);

        // Assert
        var handler = (BaseRequestHandler)provider.GetRequiredService<IRequestHandler<RequestBase>>();

        handler.Calls.Should().Be(1);
    }

    #endregion

    #region ExceptionHandlerTests

    [Fact]
    public async Task PublishRequestWithOnlyActedOnNotHandledException_HandlersFailsWithInvalidOperationException_ExceptionGetActedAndHandled()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new InvalidOperationException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestExceptionHandler>();

        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(1);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(0);
    }

    [Fact]
    public async Task PublishRequestWithOnlyActedOnNotHandledException_HandlersFailsWithUnreachableException_ExceptionGetHandled()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new UnreachableException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestExceptionHandler>();

        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(0);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(1);
    }

    [Fact]
    public async Task PublishRequest_ExceptionIsThrownWithoutAnyExceptionHandler_RethrowsTheExceptionInThePipeline()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>(AssemblyScannerOptions.Handlers | AssemblyScannerOptions.Processor | AssemblyScannerOptions.PipelineBehaviors | AssemblyScannerOptions.ExceptionActionHandler);
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new UnreachableException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        var act = async () => await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        await act.Should().ThrowExactlyAsync<UnreachableException>().WithMessage(expectedException.Message);
    }

    #endregion

    #region ExceptionActionTests

    [Fact]
    public async Task PublishRequestWithoutCaching_HandlersFailsWithInvalidOperationException_ExceptionGetActed()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new InvalidOperationException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var invalidException = (RequestAction)provider.GetRequiredService<IRequestExceptionAction<ThrowingExceptionRequest, InvalidOperationException>>();
        var exception = (RequestAction) provider.GetRequiredService<IRequestExceptionAction<ThrowingExceptionRequest, Exception>>();

        invalidException.InvalidOperationExceptionActionCalls.Should().Be(1);
        invalidException.GeneralExceptionActionCalls.Should().Be(0);
        exception.InvalidOperationExceptionActionCalls.Should().Be(0);
        exception.GeneralExceptionActionCalls.Should().Be(1);
    }
    
    [Fact]
    public async Task PublishRequestWithCaching_HandlersFailsWithInvalidOperationException_HandlersExceptionGetsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new InvalidOperationException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.SendAsync(request);
        await mediator.SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(2);
        action.GeneralExceptionActionCalls.Should().Be(2);
    }

    [Fact]
    public async Task PublishRequest_HandlersFailsWithUnreachableException_OnlyExceptionHandlerGetsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new UnreachableException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(1);
    }

    [Fact]
    public async Task RequestOnlyActedOnNotHandledException_ExceptionGetsHandled_NoActionIsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new InvalidOperationException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(0);
    }

    [Fact]
    public async Task RequestActedOnAllException_ExceptionGetsHandled_ActionIsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new InvalidOperationException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(1);
        action.GeneralExceptionActionCalls.Should().Be(1);
    }

    #endregion

    #region RequestPipeline

    [Fact]
    public async Task Request_WithDefaultRegistration_InvokesPipelineHandlersAsResolved()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<Request>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    [Fact]
    public async Task Request_WithPreregisteredPipelineHandler_InvokesPreregisteredHandlerFirst()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RequestBehaviors.Add<SpecificPipelineHandler>();
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        var genericHandler = GetGenericPipelineHandler<Request>(provider);
        var specificHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);
        specificHandler.InvocationValidation = () => genericHandler.Calls.Should().Be(0);

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<Request>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    [Fact]
    public async Task Request_WithRequestChangingPipelineHandler_ChangesRequestForThePipelineFlow()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RequestBehaviors.Add(typeof(IPipelineBehavior<Request>), typeof(RequestChangingPipelineHandler));
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request
        {
            ShouldChangeRequest = true
        };

        var genericHandler = GetGenericPipelineHandler<Request>(provider);
        var specificHandler = provider.GetRequiredService<RequestChangingPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<Request>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    #endregion

    #region RequestProcessor

    [Fact]
    public async Task Request_WithDefaultProcessor_InvokesAllProcessorsInOrder()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        var preProcessor = GetPreProcessor<PreProcessor<Request>, Request>(provider);
        var postProcessor = GetPostProcessor<PostProcessor<Request>, Request>(provider);
        preProcessor.InvocationCheck = () => postProcessor.Calls.Should().Be(0);
        postProcessor.InvocationCheck = () => preProcessor.Calls.Should().BeGreaterThan(0);

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var genericPreProcessor = GetPreProcessor<PreProcessor<Request>, Request>(provider);
        var genericPostProcessor = GetPostProcessor<PostProcessor<Request>, Request>(provider);

        genericPreProcessor.Calls.Should().Be(1);
        genericPostProcessor.Calls.Should().Be(1);
    }
    
    [Fact]
    public async Task Request_WithpRreRegisteredProcessor_InvokesAllProcessorsInOrder()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestPreProcessors.AddOpenGeneric(typeof(PreRegisteredPreProcessor<>));
            cfg.RequestPostProcessors.AddOpenGeneric(typeof(PreRegisteredPostProcessor<>));
        });
        var provider = collection.BuildServiceProvider();
        var request = new Request();

        var preProcessor = GetPreProcessor<PreRegisteredPreProcessor<Request>, Request>(provider);
        var postProcessor = GetPostProcessor<PreRegisteredPostProcessor<Request>, Request>(provider);
        preProcessor.InvocationCheck = () => postProcessor.Calls.Should().Be(0);
        postProcessor.InvocationCheck = () => preProcessor.Calls.Should().BeGreaterThan(0);

        // Act
        await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var genericPreRegisteredPreProcessor = GetPreProcessor<PreRegisteredPreProcessor<Request>, Request>(provider);
        var genericPreRegisteredPostProcessor = GetPostProcessor<PreRegisteredPostProcessor<Request>, Request>(provider);
        var genericPreProcessor = GetPreProcessor<PreProcessor<Request>, Request>(provider);
        var genericPostProcessor = GetPostProcessor<PostProcessor<Request>, Request>(provider);

        genericPreRegisteredPreProcessor.Calls.Should().Be(1);
        genericPreProcessor.Calls.Should().Be(1);
        genericPreRegisteredPostProcessor.Calls.Should().Be(1);
        genericPostProcessor.Calls.Should().Be(1);
    }

    #endregion

    private static GenericPipelineHandler<TRequest> GetGenericPipelineHandler<TRequest>(IServiceProvider serviceProvider)
        where TRequest : IRequest =>
        serviceProvider.GetServices<IPipelineBehavior<TRequest>>()
            .OfType<GenericPipelineHandler<TRequest>>()
            .Single();

    private static RestrictedGenericPipelineHandler<TRequest> GetRestrictedHandler<TRequest>(IServiceProvider serviceProvider)
        where TRequest : ThrowingExceptionRequest, IRequest =>
        serviceProvider.GetServices<IPipelineBehavior<TRequest>>()
            .OfType<RestrictedGenericPipelineHandler<TRequest>>()
            .Single();

    private static TPostProcessor GetPostProcessor<TPostProcessor, TRequest>(IServiceProvider serviceProvider)
        where TPostProcessor : IRequestPostProcessor<TRequest>
        where TRequest : IRequest =>
        serviceProvider.GetServices<IRequestPostProcessor<TRequest>>()
            .OfType<TPostProcessor>()
            .Single();

    private static TPreProcessor GetPreProcessor<TPreProcessor, TRequest>(IServiceProvider serviceProvider)
        where TPreProcessor : IRequestPreProcessor<TRequest>
        where TRequest : IRequest =>
        serviceProvider.GetServices<IRequestPreProcessor<TRequest>>()
            .OfType<TPreProcessor>()
            .Single();

    public void Dispose() => TestCleaner.CleanUp();
}