namespace MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

internal class StreamRequestMessage : StreamRequestBase, IStreamRequest<StreamResponse>
{
    public bool ShouldChangeRequest { get; set; }
}