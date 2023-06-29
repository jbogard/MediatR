using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Abstraction.ExceptionHandler;

/// <summary>
/// Defines an exception action for a request with a response
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestResponseExceptionAction<in TRequest, TResponse, in TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
}

/// <summary>
/// Defines the base exception action for a request.
///     You do not need to register this interface explicitly
///     with a container as it inherits from the base
///     <see cref="IRequestResponseExceptionAction{TRequest, TResponse, TException}"/> interface.
/// </summary>
/// <typeparam name="TRequest">The type of failed request</typeparam>
/// <typeparam name="TResponse">The response type of the failed request</typeparam>
public interface IRequestResponseExceptionAction<in TRequest, TResponse> : IRequestResponseExceptionAction<TRequest, TResponse, Exception>
    where TRequest : IRequest<TResponse>
{
}