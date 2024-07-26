using MediatR.Pipeline;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Pipeline;

public class RequestPreProcessorBehaviorTests
{
    private sealed record Command : IRequest<string>;

    [Fact]
    public void Should_throw_for_ctor_when_configuration_is_null()
    {
        IEnumerable<IRequestPreProcessor<Command>> preProcessors = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => new RequestPreProcessorBehavior<Command, string>(preProcessors));

        Assert.Equal(nameof(preProcessors), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_request_is_null()
    {
        IEnumerable<IRequestPreProcessor<Command>> preProcessors = new List<IRequestPreProcessor<Command>>();
        var requestPreProcessorBehavior = new RequestPreProcessorBehavior<Command, string>(preProcessors);
        Command request = null!;
        RequestHandlerDelegate<string> next = () => new Task<string>(() => string.Empty);

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestPreProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(request), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_next_is_null()
    {
        IEnumerable<IRequestPreProcessor<Command>> preProcessors = new List<IRequestPreProcessor<Command>>();
        var requestPreProcessorBehavior = new RequestPreProcessorBehavior<Command, string>(preProcessors);
        var request = new Command();
        RequestHandlerDelegate<string> next = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await requestPreProcessorBehavior.Handle(request, next, CancellationToken.None));

        Assert.Equal(nameof(next), exception.ParamName);
    }
}