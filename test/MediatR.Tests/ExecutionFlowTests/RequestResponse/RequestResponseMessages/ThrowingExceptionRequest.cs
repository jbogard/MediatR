using System;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal class ThrowingExceptionRequest : IRequest<Response>
{
    public required Exception Exception { get; init; }
}

internal sealed class AccessViolationRequest : ThrowingExceptionRequest
{
    public required AccessViolationException AccessViolationException { get; set; }
}