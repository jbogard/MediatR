using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExceptionHandling;

namespace MediatR.Tests.ExecutionFlowTests;

// internal sealed class OpenGenericAccessViolationExceptionHandler<TRequest, TResponse> : IRequestResponseExceptionHandler<TRequest, TResponse, AccessViolationException>
//     where TRequest : IRequest<TResponse>
//     where TResponse : new()
// {
//     public int Calls { get; private set; }
//     
//     public Task Handle(TRequest request, AccessViolationException exception, RequestResponseExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
//     {
//         Calls++;
//         state.SetHandled(new TResponse());
//         return Task.CompletedTask;
//     }
// }