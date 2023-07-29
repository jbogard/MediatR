using System;

namespace MediatR.ExecutionFlowTests.Requests.RequestMessages;

internal sealed class AccessViolationRequest : ThrowingExceptionRequest
{
    public required AccessViolationException AccessViolationException { get; set; }
}