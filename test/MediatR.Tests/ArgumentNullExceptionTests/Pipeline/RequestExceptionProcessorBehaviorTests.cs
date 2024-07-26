using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Pipeline;

public class RequestExceptionProcessorBehaviorTests
{
    private sealed record Command : IRequest<string>;

    [Fact]
    public void Should_throw_for_ctor_when_service_provider_is_null()
    {
        IServiceProvider serviceProvider = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => new RequestExceptionProcessorBehavior<Command, string>(serviceProvider));

        Assert.Equal(nameof(serviceProvider), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_request_is_null()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var requestExceptionProcessorBehavior = new RequestExceptionProcessorBehavior<Command, string>(serviceProvider);
        Command request = null!;
        RequestHandlerDelegate<string> next = () => new Task<string>(() => string.Empty);

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestExceptionProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_next_is_null()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var requestExceptionProcessorBehavior = new RequestExceptionProcessorBehavior<Command, string>(serviceProvider);
        var request = new Command();
        RequestHandlerDelegate<string> next = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestExceptionProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(next), exception.ParamName);
    }
}