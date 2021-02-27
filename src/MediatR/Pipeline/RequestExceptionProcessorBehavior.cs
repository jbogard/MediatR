namespace MediatR.Pipeline
{
    using Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/>
    ///     or <see cref="RequestExceptionHandler{TRequest,TResponse}"/> instances
    ///     after an exception is thrown by the following pipeline steps
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ServiceFactory _serviceFactory;

        public RequestExceptionProcessorBehavior(ServiceFactory serviceFactory) => _serviceFactory = serviceFactory;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var state = new RequestExceptionHandlerState<TResponse>();
                Type? exceptionType = null;

                while (!state.Handled && exceptionType != typeof(Exception))
                {
                    exceptionType = exceptionType == null ? exception.GetType() : exceptionType.BaseType
                        ?? throw new InvalidOperationException("Could not determine exception base type.");
                    var exceptionHandlers = GetExceptionHandlers(request, exceptionType, out MethodInfo handleMethod);

                    foreach (var exceptionHandler in exceptionHandlers)
                    {
                        try
                        {
                            await ((Task)(handleMethod.Invoke(exceptionHandler, new object[] { request, exception, state, cancellationToken })
                                ?? throw new InvalidOperationException("Did not return a Task from the exception handler."))).ConfigureAwait(false);
                        }
                        catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
                        {
                            // Unwrap invocation exception to throw the actual error
                            ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
                        }

                        if (state.Handled)
                        {
                            break;
                        }
                    }
                }

                if (!state.Handled)
                {
                    throw;
                }

                if (state.Response is null)
                {
                    throw;
                }

                return state.Response; //cannot be null if Handled
            }
        }

        private IList<object> GetExceptionHandlers(TRequest request, Type exceptionType, out MethodInfo handleMethodInfo)
        {
            var exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
            var enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
            handleMethodInfo = exceptionHandlerInterfaceType.GetMethod(nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle))
                               ?? throw new InvalidOperationException($"Could not find method {nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle)} on type {exceptionHandlerInterfaceType}");

            var exceptionHandlers = (IEnumerable<object>)_serviceFactory.Invoke(enumerableExceptionHandlerInterfaceType);

            return HandlersOrderer.Prioritize(exceptionHandlers.ToList(), request);
        }
    }
}
