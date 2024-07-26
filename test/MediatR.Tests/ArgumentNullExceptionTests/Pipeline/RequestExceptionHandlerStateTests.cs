using MediatR.Pipeline;
using Shouldly;
using System;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Pipeline;

public class RequestExceptionHandlerStateTests
{
    [Fact]
    public void Should_throw_for_ctor_when_service_provider_is_null()
    {
        var requestExceptionHandlerState = new RequestExceptionHandlerState<string>();
        string response = null!;

        var exception = Should.Throw<ArgumentNullException>(
            () => requestExceptionHandlerState.SetHandled(response));

        Assert.Equal(nameof(response), exception.ParamName);
    }
}