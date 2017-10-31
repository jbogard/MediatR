using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR
{
    /// <summary>
    /// Defines a context supported, asynchronous handler for a request
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IContextualAsyncRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles a cancellable, asynchronous request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="context">Context</param>
        /// <returns>A task representing the response from the request</returns>
        Task<TResponse> Handle(TRequest message, IMediatorContext context);
    }
    /// <summary>
    /// Defines a context supported, asynchronous handler for a request without a response
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    public interface IContextualAsyncRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// Handles a cancellable, asynchronous request
        /// </summary>
        /// <param name="message">The request message</param>
        /// <param name="context">Context</param>
        Task Handle(TRequest message, IMediatorContext context);
    }
}
