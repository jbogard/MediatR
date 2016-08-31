using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Helper class for asynchronous requests that return a void response
    /// </summary>
    /// <typeparam name="TMessage">The type of void request being handled</typeparam>
    public abstract class AsyncRequestHandler<TMessage> : IAsyncRequestHandler<TMessage>
        where TMessage : IAsyncRequest
    {
        public async Task Handle(TMessage message)
        {
            await HandleCore(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a void request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <returns>A task representing the void response from the request</returns>
        protected abstract Task HandleCore(TMessage message);
    }
}
