namespace MediatR
{
    /// <summary>
    /// Helper class for requests that return a void response
    /// </summary>
    /// <typeparam name="TMessage">The type of void request being handled</typeparam>
    public abstract class RequestHandler<TMessage> : IRequestHandler<TMessage>
        where TMessage : IRequest
    {
        public void Handle(TMessage message)
        {
            HandleCore(message);
        }

        /// <summary>
        /// Handles a void request
        /// </summary>
        /// <param name="message">The request message</param>
        protected abstract void HandleCore(TMessage message);
    }
}