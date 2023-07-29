using System;

namespace MediatR.ExecutionFlowTests.RequestResponse;

internal class ThrowingExceptionRequest : IRequest<Response>
{
    public required Exception Exception { get; init; }
}