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
using MediatR.ExecutionFlowTests.RequestResponse.Pipelines;
using MediatR.ExecutionFlowTests.RequestResponse.Processors;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.ExecutionFlowTests.RequestResponse;

public sealed class RequestResponseTests : IDisposable
{
    #region HandlerTests

    [Fact]
    public async Task PublishRequestResponse_WithOneInstancePerServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var handler = (RequestHandler)provider.GetRequiredService<IRequestHandler<RequestResponse, Response>>();

        handler.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishRequestResponseWithCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var mediator = provider.GetRequiredService<IMediator>();
        var response1 = await mediator.SendAsync<Response>(request);
        var response2 = await mediator.SendAsync<Response>(request);

        // Assert
        var handler = provider.GetRequiredService<RequestHandler>();

        handler.Calls.Should().Be(2);
        response1.Should().NotBeNull();
        response1.Should().BeOfType<Response>();
        response2.Should().NotBeNull();
        response2.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishObjectRequestResponse_WithOneInstancePerServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        var mediator = provider.GetRequiredService<IMediator>();
        var typedResponse = await mediator.SendAsync<Response>(request);

        // Act
        var dynamicResponse = await mediator.SendAsync(request);

        // Assert
        var handler = (RequestHandler)provider.GetRequiredService<IRequestHandler<RequestResponse, Response>>();

        handler.Calls.Should().Be(2);
        typedResponse.Should().NotBeNull();
        dynamicResponse.Should().NotBeNull();
        typedResponse.Should().BeOfType<Response>();
        dynamicResponse.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishObjectRequestResponseWithCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();
        var mediator = provider.GetRequiredService<IMediator>();
        var typedResponse1 = await mediator.SendAsync<Response>(request);
        var typedResponse2 = await mediator.SendAsync<Response>(request);

        // Act
        var dynamicResponse = await mediator.SendAsync(request);

        // Assert
        var handler = provider.GetRequiredService<RequestHandler>();

        handler.Calls.Should().Be(3);
        typedResponse1.Should().NotBeNull();
        typedResponse1.Should().BeOfType<Response>();
        typedResponse2.Should().NotBeNull();
        typedResponse2.Should().BeOfType<Response>();
        dynamicResponse.Should().NotBeNull();
        dynamicResponse.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishRequestResponseWithoutCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var handler = provider.GetRequiredService<RequestHandler>();

        handler.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishBaseRequest_WithOneInstanceForeachServiceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<BaseResponse>(request);

        // Assert
        var handler = provider.GetRequiredService<BaseRequestHandler>();

        handler.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }
    
    [Fact]
    public async Task PublishBaseRequest_WithEachServiceOnceInstanceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<BaseResponse>(request);

        // Assert
        var handler = (BaseRequestHandler)provider.GetRequiredService<IRequestHandler<RequestResponse, BaseResponse>>();

        handler.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
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
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();

        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(1);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(0);
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishRequestWithOnlyActedOnNotHandledException_HandlersFailsWithUnreachableException_ExceptionGetHandled()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var expectedException = new UnreachableException("Expected Exception");

        var request = new ThrowingExceptionRequest
        {
            Exception = expectedException
        };

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var exceptionHandler = provider.GetRequiredService<RequestResponseExceptionHandler>();

        exceptionHandler.InvalidOperationExceptionHandlerCalls.Should().Be(0);
        exceptionHandler.GeneralExceptionHandlerCalls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task PublishRequest_ExceptionIsThrownWithoutAnyExceptionHandler_RethrowsTheExceptionInThePipeline()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>(AssemblyScannerOptions.Handlers | AssemblyScannerOptions.Processor | AssemblyScannerOptions.PipelineBehaviors | AssemblyScannerOptions.ExceptionActionHandler);
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
    public async Task PublishRequestResponseWithoutCaching_HandlersFailsWithInvalidOperationException_ExceptionGetActed()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var invalidException = (RequestResponseAction)provider.GetRequiredService<IRequestResponseExceptionAction<ThrowingExceptionRequest, Response, InvalidOperationException>>();
        var exception = (RequestResponseAction) provider.GetRequiredService<IRequestResponseExceptionAction<ThrowingExceptionRequest, Response, Exception>>();

        invalidException.InvalidOperationExceptionActionCalls.Should().Be(1);
        invalidException.GeneralExceptionActionCalls.Should().Be(0);
        exception.InvalidOperationExceptionActionCalls.Should().Be(0);
        exception.GeneralExceptionActionCalls.Should().Be(1);
        response.Should().NotBeNull();
    }
    
    [Fact]
    public async Task PublishRequestResponseWithCaching_HandlersFailsWithInvalidOperationException_HandlersExceptionGetsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response1 = await mediator.SendAsync(request);
        var response2 = await mediator.SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(2);
        action.GeneralExceptionActionCalls.Should().Be(2);
        response1.Should().NotBeNull();
        response2.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishRequestResponse_HandlersFailsWithUnreachableException_OnlyExceptionHandlerGetsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(1);
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestResponseOnlyActedOnNotHandledException_ExceptionGetsHandled_NoActionIsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(0);
        action.GeneralExceptionActionCalls.Should().Be(0);
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestResponseActedOnAllException_ExceptionGetsHandled_ActionIsInvoked()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
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
        var response = await provider.GetRequiredService<IMediator>().SendAsync(request);

        // Assert
        var action = provider.GetRequiredService<RequestResponseAction>();

        action.InvalidOperationExceptionActionCalls.Should().Be(1);
        action.GeneralExceptionActionCalls.Should().Be(1);
        response.Should().NotBeNull();
    }

    #endregion

    #region RequestResponsePipeline

    [Fact]
    public async Task RequestResponse_WithDefaultRegistration_InvokesPipelineHandlersAsResolved()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RequestResponse, Response>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest, Response>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest, Response>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task RequestResponse_WithPreregisteredPipelineHandler_InvokesPreregisteredHandlerFirst()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RequestResponseBehaviors.Add<SpecificPipelineHandler>();
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        var genericHandler = GetGenericPipelineHandler<RequestResponse, Response>(provider);
        var specificHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);
        specificHandler.InvocationValidation = () => genericHandler.Calls.Should().Be(0);

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RequestResponse, Response>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest, Response>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest, Response>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    [Fact]
    public async Task RequestResponse_WithRequestChangingPipelineHandler_ChangesRequestForThePipelineFlow()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RequestResponseBehaviors.Add(typeof(IPipelineBehavior<RequestResponse, Response>), typeof(RequestChangingPipelineHandler));
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse
        {
            ShouldChangeRequest = true
        };

        var genericHandler = GetGenericPipelineHandler<RequestResponse, Response>(provider);
        var specificHandler = provider.GetRequiredService<RequestChangingPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RequestResponse, Response>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<ThrowingExceptionRequest, Response>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<AccessViolationRequest, Response>(provider);

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
        response.Should().NotBeNull();
        response.Should().BeOfType<RootResponse>();
    }

    #endregion

    #region RequestResponseProcessor

    [Fact]
    public async Task RequestResponse_WithDefaultProcessor_InvokesAllProcessorsInOrder()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        var preProcessor = GetPreProcessor<PreProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var postProcessor = GetPostProcessor<PostProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        preProcessor.InvocationCheck = () => postProcessor.Calls.Should().Be(0);
        postProcessor.InvocationCheck = () => preProcessor.Calls.Should().BeGreaterThan(0);

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var genericPreProcessor = GetPreProcessor<PreProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var genericPostProcessor = GetPostProcessor<PostProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);

        genericPreProcessor.Calls.Should().Be(1);
        genericPostProcessor.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }
    
    [Fact]
    public async Task RequestResponse_WithpRreRegisteredProcessor_InvokesAllProcessorsInOrder()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RequestResponseTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestResponsePreProcessors.AddOpenGeneric(typeof(PreRegisteredPreProcessor<,>));
            cfg.RequestResponsePostProcessors.AddOpenGeneric(typeof(PreRegisteredPostProcessor<,>));
        });
        var provider = collection.BuildServiceProvider();
        var request = new RequestResponse();

        var preProcessor = GetPreProcessor<PreRegisteredPreProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var postProcessor = GetPostProcessor<PreRegisteredPostProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        preProcessor.InvocationCheck = () => postProcessor.Calls.Should().Be(0);
        postProcessor.InvocationCheck = () => preProcessor.Calls.Should().BeGreaterThan(0);

        // Act
        var response = await provider.GetRequiredService<IMediator>().SendAsync<Response>(request);

        // Assert
        var genericPreRegisteredPreProcessor = GetPreProcessor<PreRegisteredPreProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var genericPreRegisteredPostProcessor = GetPostProcessor<PreRegisteredPostProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var genericPreProcessor = GetPreProcessor<PreProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);
        var genericPostProcessor = GetPostProcessor<PostProcessor<RequestResponse, Response>, RequestResponse, Response>(provider);

        genericPreRegisteredPreProcessor.Calls.Should().Be(1);
        genericPreProcessor.Calls.Should().Be(1);
        genericPreRegisteredPostProcessor.Calls.Should().Be(1);
        genericPostProcessor.Calls.Should().Be(1);
        response.Should().NotBeNull();
        response.Should().BeOfType<Response>();
    }

    #endregion

    private static GenericPipelineHandler<TRequest, TResponse> GetGenericPipelineHandler<TRequest, TResponse>(IServiceProvider serviceProvider)
        where TRequest : IRequest<TResponse> =>
        serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .OfType<GenericPipelineHandler<TRequest, TResponse>>()
            .Single();

    private static RestrictedGenericPipelineHandler<TRequest, TResponse> GetRestrictedHandler<TRequest, TResponse>(IServiceProvider serviceProvider)
        where TRequest : ThrowingExceptionRequest, IRequest<TResponse> =>
        serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .OfType<RestrictedGenericPipelineHandler<TRequest, TResponse>>()
            .Single();

    private static TPostProcessor GetPostProcessor<TPostProcessor, TRequest, TResponse>(IServiceProvider serviceProvider)
        where TPostProcessor : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse> =>
        serviceProvider.GetServices<IRequestPostProcessor<TRequest, TResponse>>()
            .OfType<TPostProcessor>()
            .Single();

    private static TPreProcessor GetPreProcessor<TPreProcessor, TRequest, TResponse>(IServiceProvider serviceProvider)
        where TPreProcessor : IRequestPreProcessor<TRequest, TResponse>
        where TRequest : IRequest<TResponse> =>
        serviceProvider.GetServices<IRequestPreProcessor<TRequest, TResponse>>()
            .OfType<TPreProcessor>()
            .Single();

    public void Dispose() => TestCleaner.CleanUp();
}