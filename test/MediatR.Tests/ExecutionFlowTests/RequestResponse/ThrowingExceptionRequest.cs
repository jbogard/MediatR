namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class ThrowingExceptionRequest : IRequest<Response>
{
    public required Exception Exception { get; init; }
}

internal sealed class AccessViolationRequest : IRequest<Response>
{
    public required AccessViolationException Exception { get; set; }
}