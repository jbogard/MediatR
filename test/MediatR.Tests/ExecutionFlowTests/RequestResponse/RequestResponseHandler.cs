using MediatR.Abstraction.Handlers;

namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class RequestResponseHandler :
    IRequestHandler<RequestResponse, Response>,
    IRequestHandler<RequestResponseBase, BaseResponse>,
    IRequestHandler<ThrowingExceptionRequest, Response>,
    IRequestHandler<AccessViolationRequest, Response>
{
    public int BaseClassCalls { get; private set; }
    public int FinalClassCalls { get; private set; }
    
    public ValueTask<Response> Handle(RequestResponse request, CancellationToken cancellationToken)
    {
        FinalClassCalls++;
        return new ValueTask<Response>(new Response());
    }

    public ValueTask<BaseResponse> Handle(RequestResponseBase request, CancellationToken cancellationToken)
    {
        BaseClassCalls++;
        return new ValueTask<BaseResponse>(new Response());
    }

    public ValueTask<Response> Handle(ThrowingExceptionRequest request, CancellationToken cancellationToken) => throw request.Exception;

    public ValueTask<Response> Handle(AccessViolationRequest request, CancellationToken cancellationToken) => throw request.Exception;
}