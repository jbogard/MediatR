using System;

namespace MediatR.ExecutionFlowTests.Requests.RequestMessages;

internal class ThrowingExceptionRequest : IRequest
{
    public required Exception Exception { get; init; }
}