﻿using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExceptionHandling;

namespace MediatR.Tests.ExecutionFlowTests;

internal sealed class RequestResponseExceptionHandler :
    IRequestResponseExceptionHandler<ThrowingExceptionRequest, Response, InvalidOperationException>,
    IRequestResponseExceptionHandler<ThrowingExceptionRequest, Response>
{
    public int InvalidOperationExceptionHandlerCalls { get; private set; }
    public int GeneralExceptionHandlerCalls { get; private set; }
    
    public Task Handle(ThrowingExceptionRequest request, InvalidOperationException exception, RequestResponseExceptionHandlerState<Response> state, CancellationToken cancellationToken)
    {
        if (request.Exception == exception)
        {
            state.SetHandled(new Response());
        }

        InvalidOperationExceptionHandlerCalls++;
        
        return Task.CompletedTask;
    }

    public Task Handle(ThrowingExceptionRequest request, Exception exception, RequestResponseExceptionHandlerState<Response> state, CancellationToken cancellationToken)
    {
        state.SetHandled(new Response());

        GeneralExceptionHandlerCalls++;
        return Task.CompletedTask;
    }
}