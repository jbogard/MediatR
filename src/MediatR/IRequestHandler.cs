using System.Threading;
using System.Threading.Tasks;

namespace MediatR;

/// <summary>
/// Defines a handler for a request
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a handler for a request with a void (<see cref="Unit" />) response.
/// You do not need to register this interface explicitly with a container as it inherits from the base <see cref="IRequestHandler{TRequest, TResponse}" /> interface.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}

/// <summary>
/// Wrapper class for a handler that asynchronously handles a request and does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public abstract class AsyncRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    async Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
    {
        await Handle(request, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Response</returns>
    protected abstract Task Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Wrapper class for a handler that synchronously handles a request and returns a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> IRequestHandler<TRequest, TResponse>.Handle(TRequest request, CancellationToken cancellationToken)
        => Task.FromResult(Handle(request));

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="request">Request</param>
    /// <returns>Response</returns>
    protected abstract TResponse Handle(TRequest request);
}

/// <summary>
/// Wrapper class for a handler that synchronously handles a request and does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
    {
        Handle(request);
        return Unit.Task;
    }

    protected abstract void Handle(TRequest request);
}
