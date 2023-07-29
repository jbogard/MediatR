namespace MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;

internal sealed class RootStreamRequestMessage : StreamRequestMessage
{
    public required StreamResponse StreamResponse { get; set; }
}