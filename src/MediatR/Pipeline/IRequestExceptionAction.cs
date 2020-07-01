namespace MediatR.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Defines an exception action for a request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TException">Exception type</typeparam>
    public interface IRequestExceptionAction<in TRequest, in TException>
        where TRequest : notnull
        where TException : Exception
    {
        /// <summary>
        /// Called when the request handler throws an exception
        /// </summary>
        /// <param name="request">Request instance</param>
        /// <param name="exception">The thrown exception</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An awaitable task</returns>
        Task Execute(TRequest request, TException exception, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines the base exception action for a request.
    ///     You do not need to register this interface explicitly
    ///     with a container as it inherits from the base
    ///     <see cref="IRequestExceptionAction{TRequest, TException}" /> interface.
    /// </summary>
    /// <typeparam name="TRequest">The type of failed request</typeparam>
    public interface IRequestExceptionAction<in TRequest> : IRequestExceptionAction<TRequest, Exception>
        where TRequest : notnull
    {
    }

    /// <summary>
    /// Wrapper class that asynchronously performs an action on a request for base exception
    /// </summary>
    /// <typeparam name="TRequest">The type of failed request</typeparam>
    public abstract class AsyncRequestExceptionAction<TRequest> : IRequestExceptionAction<TRequest>
        where TRequest : IRequest
    {
        async Task IRequestExceptionAction<TRequest, Exception>.Execute(TRequest request, Exception exception, CancellationToken cancellationToken)
        {
            await Execute(request, exception, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Override in a derived class for the action logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">Original exception from request handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task Execute(TRequest request, Exception exception, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Wrapper class that synchronously performs an action on a request for specific exception
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TException">Exception type</typeparam>
    public abstract class RequestExceptionAction<TRequest, TException> : IRequestExceptionAction<TRequest, TException>
        where TRequest : notnull
        where TException : Exception
    {
        Task IRequestExceptionAction<TRequest, TException>.Execute(TRequest request, TException exception, CancellationToken cancellationToken)
        {
            Execute(request, exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override in a derived class for the action logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">Original exception from request handler</param>
        protected abstract void Execute(TRequest request, TException exception);
    }

    /// <summary>
    /// Wrapper class that synchronously performs an action on a request for base exception
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    public abstract class RequestExceptionAction<TRequest> : IRequestExceptionAction<TRequest>
        where TRequest : notnull
    {
        Task IRequestExceptionAction<TRequest, Exception>.Execute(TRequest request, Exception exception, CancellationToken cancellationToken)
        {
            Execute(request, exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override in a derived class for the action logic
        /// </summary>
        /// <param name="request">Failed request</param>
        /// <param name="exception">Original exception from request handler</param>
        protected abstract void Execute(TRequest request, Exception exception);
    }
}
