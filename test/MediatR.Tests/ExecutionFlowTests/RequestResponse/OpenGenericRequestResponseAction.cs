using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.Tests.ExecutionFlowTests;

// internal sealed class OpenGenericRequestResponseAction<TRequest, TResponse> : IRequestResponseExceptionAction<TRequest, TResponse>
//     where TRequest : IRequest<TResponse>
// {
//     public int Calls { get; private set; }
//
//     public Task Execute(TRequest request, Exception exception, CancellationToken cancellationToken)
//     {
//         Calls++;
//         return Task.CompletedTask;
//     }
// }