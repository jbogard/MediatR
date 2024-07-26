using MediatR.Pipeline;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Pipeline;

public class RequestPostProcessorBehaviorTests
{
    private sealed record Command : IRequest<string>;

    [Fact]
    public void Should_throw_for_ctor_when_service_provider_is_null()
    {
        IEnumerable<IRequestPostProcessor<Command, string>> postProcessors = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => new RequestPostProcessorBehavior<Command, string>(postProcessors));

        Assert.Equal(nameof(postProcessors), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_request_is_null()
    {
        var postProcessors = new List<IRequestPostProcessor<Command, string>>();
        var requestPostProcessorBehavior = new RequestPostProcessorBehavior<Command, string>(postProcessors);
        Command request = null!;
        RequestHandlerDelegate<string> next = () => new Task<string>(() => string.Empty);

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestPostProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_next_is_null()
    {
        var postProcessors = new List<IRequestPostProcessor<Command, string>>();
        var requestPostProcessorBehavior = new RequestPostProcessorBehavior<Command, string>(postProcessors);
        var request = new Command();
        RequestHandlerDelegate<string> next = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestPostProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(next), exception.ParamName);
    }
}