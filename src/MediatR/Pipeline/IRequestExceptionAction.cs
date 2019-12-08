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
    public interface IRequestExceptionAction<TRequest> : IRequestExceptionAction<TRequest, Exception>
    {
    }

    /// <summary>
    /// Wrapper class that synchronously performs an action on a request for specific exception
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    public abstract class RequestExceptionAction<TRequest, TException> : IRequestExceptionAction<TRequest, TException>
        where TException : Exception
    {
        Task IRequestExceptionAction<TRequest, TException>.Execute(TRequest request, TException exception, CancellationToken cancellationToken)
        {
            Execute(request, exception);
            return Task.CompletedTask;
        }

        protected abstract void Execute(TRequest request, TException exception);
    }

    /// <summary>
    /// Wrapper class that synchronously performs an action on a request for base exception
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    public abstract class RequestExceptionAction<TRequest> : IRequestExceptionAction<TRequest>
    {
        Task IRequestExceptionAction<TRequest, Exception>.Execute(TRequest request, Exception exception, CancellationToken cancellationToken)
        {
            Execute(request, exception);
            return Task.CompletedTask;
        }

        protected abstract void Execute(TRequest request, Exception exception);
    }
}
