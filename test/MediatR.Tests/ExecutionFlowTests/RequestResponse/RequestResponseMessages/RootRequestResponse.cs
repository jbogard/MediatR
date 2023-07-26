namespace MediatR.ExecutionFlowTests.RequestResponse;

internal sealed class RootRequestResponse : RequestResponse
{
    public required Response Response { get; set; }
}

internal sealed class RootResponse : Response
{
}