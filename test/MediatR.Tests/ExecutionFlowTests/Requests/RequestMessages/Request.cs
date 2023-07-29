namespace MediatR.ExecutionFlowTests.Requests.RequestMessages;

internal class Request : RequestBase
{
    public bool ShouldChangeRequest { get; set; }
}