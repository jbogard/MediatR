using MediatR.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Wrappers;

public class RequestHandlerBaseTests
{
    private sealed record CommandVoid : IRequest;
    private sealed record Command : IRequest<string>;

    [Fact]
    public async Task Should_throw_for_handle_with_request_is_object_when_request_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<CommandVoid>();
        object request = null!;
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_with_request_is_object_when_service_provider_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<CommandVoid>();
        object request = new CommandVoid();
        IServiceProvider serviceProvider = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_request_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<CommandVoid>();
        IRequest request = null!;
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_service_provider_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<CommandVoid>();
        IRequest request = new CommandVoid();
        IServiceProvider serviceProvider = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_void_handle_with_request_is_object_when_request_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<Command, string>();
        object request = null!;
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_void_handle_with_request_is_object_when_service_provider_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<Command, string>();
        object request = new Command();
        IServiceProvider serviceProvider = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_void_handle_when_request_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<Command, string>();
        IRequest<string> request = null!;
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_void_handle_when_service_provider_is_null()
    {
        var requestHandlerWrapperImpl = new RequestHandlerWrapperImpl<Command, string>();
        IRequest<string> request = new Command();
        IServiceProvider serviceProvider = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestHandlerWrapperImpl.Handle(request, serviceProvider, CancellationToken.None));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }
}