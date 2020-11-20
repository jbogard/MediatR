namespace MediatR.Pipeline.Streams
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an exception handler for a request and response
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TException">Exception type</typeparam>
    public interface IStreamRequestExceptionHandler<in TRequest, TResponse, TException>
        where TRequest : notnull
        where TException : Exception
    {
        /// <summary>
        /// Called when the request handler throws an exception
        /// </summary>
        /// <param name="request">Request instance</param>
        /// <param name="exception">The thrown exception</param>
        /// <param name="state">The current state of handling the exception</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An awaitable task</returns>
        Task Handle(TRequest request, TException exception, StreamRequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines the base exception handler for a request and response
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IStreamRequestExceptionHandler<in TRequest, TResponse> : IStreamRequestExceptionHandler<TRequest, TResponse, Exception>
        where TRequest : notnull
    {
    }

    /// <summary>
    /// Wrapper class that asynchronously handles a base exception from request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class StreamAsyncStreamRequestExceptionHandler<TRequest, TResponse> : IStreamRequestExceptionHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        async Task IStreamRequestExceptionHandler<TRequest, TResponse, Exception>.Handle(TRequest request, Exception exception, StreamRequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
        {
            await Handle(request, exception, state, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">The thrown exception</param>
        /// <param name="state">The current state of handling the exception</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task Handle(TRequest request, Exception exception, StreamRequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Wrapper class that synchronously handles an exception from request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TException">Exception type</typeparam>
    public abstract class StreamRequestExceptionHandler<TRequest, TResponse, TException> : IStreamRequestExceptionHandler<TRequest, TResponse, TException>
        where TRequest : notnull
        where TException : Exception
    {
        Task IStreamRequestExceptionHandler<TRequest, TResponse, TException>.Handle(TRequest request, TException exception, StreamRequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
        {
            Handle(request, exception, state);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">The thrown exception</param>
        /// <param name="state">The current state of handling the exception</param>
        protected abstract void Handle(TRequest request, TException exception, StreamRequestExceptionHandlerState<TResponse> state);
    }

    /// <summary>
    /// Wrapper class that synchronously handles a base exception from request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class StreamRequestExceptionHandler<TRequest, TResponse> : IStreamRequestExceptionHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        Task IStreamRequestExceptionHandler<TRequest, TResponse, Exception>.Handle(TRequest request, Exception exception, StreamRequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
        {
            Handle(request, exception, state);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override in a derived class for the handler logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">The thrown exception</param>
        /// <param name="state">The current state of handling the exception</param>
        protected abstract void Handle(TRequest request, Exception exception, StreamRequestExceptionHandlerState<TResponse> state);
    }
}
