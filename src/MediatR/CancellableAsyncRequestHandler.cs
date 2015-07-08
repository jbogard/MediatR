using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Helper class for cancellable, asynchronous requests that return a void response
    /// </summary>
    /// <typeparam name="TMessage">The type of void request being handled</typeparam>
    public abstract class CancellableAsyncRequestHandler<TMessage> : ICancellableAsyncRequestHandler<TMessage, Unit>
        where TMessage : ICancellableAsyncRequest
    {
        public async Task<Unit> Handle(TMessage message, CancellationToken cancellationToken)
        {
            await HandleCore(message, cancellationToken);

            return Unit.Value;
        }

        /// <summary>
        /// Handles a void request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the void response from the request</returns>
        protected abstract Task HandleCore(TMessage message, CancellationToken cancellationToken);
    }
}