namespace MediatR.ExecutionFlowTests.RequestResponse;

internal class RequestResponse : RequestResponseBase, IRequest<Response>
{
    public bool ShouldChangeRequest { get; set; }
}