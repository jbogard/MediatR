using System.Reflection;

namespace MediatR.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/> or <see cref="IRequestExceptionHandler{TRequest,TResponse}"/> instances after an exception is thrown by the request handler
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ServiceFactory _serviceFactory;
        private readonly IEnumerable<IRequestExceptionHandler<TRequest, TResponse>> _exceptionHandlers;

        public RequestExceptionProcessorBehavior(ServiceFactory serviceFactory, IEnumerable<IRequestExceptionHandler<TRequest, TResponse>> exceptionHandlers)
        {
            _serviceFactory = serviceFactory;
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

                Type exceptionType = exception.GetType();

                do
                {
                    Type exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
                    Type enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
                    MethodInfo handleMethod = exceptionHandlerInterfaceType.GetMethod(nameof(IRequestExceptionHandler<TRequest, TResponse>.Handle));
                    IEnumerable<object> exceptionHandlers = (IEnumerable<object>)_serviceFactory.Invoke(enumerableExceptionHandlerInterfaceType);

                    foreach (object exceptionHandler in exceptionHandlers)
                    {
                        await ((Task)handleMethod.Invoke(exceptionHandler, new object[] { request, exception, state, cancellationToken })).ConfigureAwait(false);
                    }

                    exceptionType = exceptionType.BaseType;
                } while (exceptionType != typeof(object));

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
