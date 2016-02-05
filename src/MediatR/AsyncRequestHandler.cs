using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Helper class for asynchronous requests that return a void response
    /// </summary>
    /// <typeparam name="TMessage">The type of void request being handled</typeparam>
    public abstract class AsyncRequestHandler<TMessage> : IAsyncRequestHandler<TMessage, Unit>
        where TMessage : IAsyncRequest
    {
        public async Task<Unit> Handle(TMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await HandleCore(message, cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        /// <summary>
        /// Handles a void request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the void response from the request</returns>
        protected abstract Task HandleCore(TMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
