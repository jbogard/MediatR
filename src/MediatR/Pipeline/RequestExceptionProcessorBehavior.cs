namespace MediatR.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse}"/> instances after an exception is thrown by the request handler
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IRequestExceptionHandler<TRequest, TResponse>> _exceptionHandlers;

        public RequestExceptionProcessorBehavior(IEnumerable<IRequestExceptionHandler<TRequest, TResponse>> exceptionHandlers)
        {
            _exceptionHandlers = exceptionHandlers;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var state = new RequestExceptionHandlerState<TResponse>();

                foreach (var exceptionHandler in _exceptionHandlers)
                {
                    await exceptionHandler.Handle(request, exception, state, cancellationToken).ConfigureAwait(false);
                }

                if (!state.Handled)
                {
                    throw;
                }

                return state.Response;
            }
        }
    } 
}
