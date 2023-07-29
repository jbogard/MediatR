using System;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class AccessViolationRequest : ThrowingExceptionRequest
{
    public required AccessViolationException AccessViolationException { get; set; }
}