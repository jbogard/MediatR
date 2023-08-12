using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;
using MediatR.DependencyInjection.Configuration;
using MediatR.ExecutionFlowTests.StreamRequest.Handlers;
using MediatR.ExecutionFlowTests.StreamRequest.Pipelines;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.ExecutionFlowTests.StreamRequest;

public sealed class StreamRequestTests : IDisposable
{
    #region HandlerTests

    [Fact]
    public async Task PublishStreamRequest_WithOneInstancePerServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<StreamResponse>(request);

        // Assert
        var handler = (StreamRequestHandler)provider.GetRequiredService<IStreamRequestHandler<StreamRequestMessage, StreamResponse>>();

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task PublishStreamRequestWithCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();

        // Act
        var mediator = provider.GetRequiredService<IMediator>();
        var firstStream = mediator.CreateStreamAsync<StreamResponse>(request);
        var secondStream = mediator.CreateStreamAsync<StreamResponse>(request);

        // Assert
        var handler = provider.GetRequiredService<StreamRequestHandler>();

        await foreach (var response in firstStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        await foreach (var response in secondStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        handler.Calls.Should().Be(2);
    }

    [Fact]
    public async Task PublishObjectStreamRequest_WithOneInstancePerServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();
        var mediator = provider.GetRequiredService<IMediator>();
        var typedStream = mediator.CreateStreamAsync<StreamResponse>(request);

        // Act
        var dynamicStream = mediator.CreateStreamAsync(request);

        // Assert
        var handler = (StreamRequestHandler)provider.GetRequiredService<IStreamRequestHandler<StreamRequestMessage, StreamResponse>>();

        await foreach (var response in typedStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        await foreach (var response in dynamicStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        handler.Calls.Should().Be(2);
    }

    [Fact]
    public async Task PublishObjectStreamRequestWithCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();
        var mediator = provider.GetRequiredService<IMediator>();
        var firstStream = mediator.CreateStreamAsync<StreamResponse>(request);
        var secondStream = mediator.CreateStreamAsync<StreamResponse>(request);

        // Act
        var dynamicStream = mediator.CreateStreamAsync(request);

        // Assert
        var handler = provider.GetRequiredService<StreamRequestHandler>();

        await foreach (var response in firstStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        await foreach (var response in secondStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        await foreach (var response in dynamicStream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        handler.Calls.Should().Be(3);
    }

    [Fact]
    public async Task PublishStreamRequestWithoutCaching_WithOneInstanceForeachServiceRegistration_ReturnsDefaultResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<StreamResponse>(request);

        // Assert
        var handler = provider.GetRequiredService<StreamRequestHandler>();

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task PublishBaseRequest_WithOneInstanceForeachServiceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<BaseStreamResponse>(request);

        // Assert
        var handler = provider.GetRequiredService<BaseStreamRequestHandler>();

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeAssignableTo<BaseStreamResponse>(); 
        }

        handler.Calls.Should().Be(1);
    }
    
    [Fact]
    public async Task PublishBaseRequest_WithEachServiceOnceInstanceRegistration_ReturnsBaseResponse()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.EachServiceOneInstance;
            cfg.DefaultServiceLifetime = ServiceLifetime.Singleton;
        });
        var provider = collection.BuildServiceProvider();
        var request = new StreamRequestMessage();

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<BaseStreamResponse>(request);

        // Assert
        var handler = (BaseStreamRequestHandler)provider.GetRequiredService<IStreamRequestHandler<StreamRequestMessage, BaseStreamResponse>>();

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeAssignableTo<BaseStreamResponse>();
        }

        handler.Calls.Should().Be(1);
    }

    #endregion
    
    #region StreamRequestPipeline

    [Fact]
    public async Task StreamRequest_WithDefaultRegistration_InvokesPipelineHandlersAsResolved()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
        });
        var provider = collection.BuildServiceProvider();
        var request = new RootStreamRequestMessage
        {
            StreamResponse = new StreamResponse()
        };

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<StreamResponse>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RootStreamRequestMessage, StreamResponse>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<StreamRequestMessage, StreamResponse>(provider);

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }
        
        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    [Fact]
    public async Task StreamRequest_WithPreregisteredPipelineHandler_InvokesPreregisteredHandlerFirst()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.StreamRequestBehaviors.Add<SpecificPipelineHandler>();
        });
        var provider = collection.BuildServiceProvider();
        var request = new RootStreamRequestMessage
        {
            StreamResponse = new StreamResponse()
        };

        var genericHandler = GetGenericPipelineHandler<StreamRequestMessage, StreamResponse>(provider);
        var specificHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);
        specificHandler.InvocationValidation = () => genericHandler.Calls.Should().Be(0);

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<StreamResponse>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RootStreamRequestMessage, StreamResponse>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<StreamRequestMessage, StreamResponse>(provider);

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<StreamResponse>();
        }

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    [Fact]
    public async Task StreamRequest_WithRequestChangingPipelineHandler_ChangesRequestForThePipelineFlow()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<StreamRequestTests>();
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.StreamRequestBehaviors.Add<IStreamPipelineBehavior<StreamRequestMessage, StreamResponse>, RequestChangingPipelineHandler>();
        });
        var provider = collection.BuildServiceProvider();
        var request = new RootStreamRequestMessage
        {
            ShouldChangeRequest = true,
            StreamResponse = new RootStreamResponse()
        };

        var genericHandler = GetGenericPipelineHandler<StreamRequestMessage, StreamResponse>(provider);
        var specificHandler = provider.GetRequiredService<RequestChangingPipelineHandler>();
        genericHandler.InvocationValidation = () => specificHandler.Calls.Should().Be(1);

        // Act
        var stream = provider.GetRequiredService<IMediator>().CreateStreamAsync<StreamResponse>(request);

        // Assert
        var pipelineHandler = provider.GetRequiredService<SpecificPipelineHandler>();
        var genericPipelineHandler = GetGenericPipelineHandler<RootStreamRequestMessage, StreamResponse>(provider);
        var restrictedPipelineHandlerBase = GetRestrictedHandler<StreamRequestMessage, StreamResponse>(provider);
        var restrictedPipelineHandler = GetRestrictedHandler<StreamRequestMessage, StreamResponse>(provider);

        await foreach (var response in stream)
        {
            response.Should().NotBeNull();
            response.Should().BeOfType<RootStreamResponse>();
        }

        pipelineHandler.Calls.Should().Be(1);
        genericPipelineHandler.Calls.Should().Be(1);
        restrictedPipelineHandlerBase.Calls.Should().Be(0);
        restrictedPipelineHandler.Calls.Should().Be(0);
    }

    #endregion
    
    private static GenericPipelineHandler<TRequest, TResponse> GetGenericPipelineHandler<TRequest, TResponse>(IServiceProvider serviceProvider)
        where TRequest : IStreamRequest<TResponse>
        where TResponse : notnull =>
        serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>()
            .OfType<GenericPipelineHandler<TRequest, TResponse>>()
            .Single();

    private static RestrictedGenericPipelineHandler<TRequest, TResponse> GetRestrictedHandler<TRequest, TResponse>(IServiceProvider serviceProvider)
        where TRequest : StreamRequestMessage, IStreamRequest<TResponse>
        where TResponse : notnull =>
        serviceProvider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>()
            .OfType<RestrictedGenericPipelineHandler<TRequest, TResponse>>()
            .Single();


    public void Dispose() => TestCleaner.CleanUp();
}