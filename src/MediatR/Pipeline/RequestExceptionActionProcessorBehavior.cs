namespace MediatR.Pipeline
{
    using MediatR.Internal;
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
    public class RequestExceptionActionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ServiceFactory _serviceFactory;

        public RequestExceptionActionProcessorBehavior(ServiceFactory serviceFactory) => _serviceFactory = serviceFactory;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var actionsForException = GetActionsForException(exception.GetType(), request, out MethodInfo actionMethod);

                foreach (var actionForException in actionsForException)
                {
                    try
                    {
                        await ((Task)(actionMethod.Invoke(actionForException, new object[] { request, exception, cancellationToken })
                            ?? throw new InvalidOperationException($"Could not create task for action method {actionMethod}."))).ConfigureAwait(false);
                    }
                    catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
                    {
                        // Unwrap invocation exception to throw the actual error
                        ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
                    }
                }

                throw;
            }
        }

        private IList<object> GetActionsForException(Type exceptionType, TRequest request, out MethodInfo actionMethodInfo)
        {
            var exceptionActionInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);
            var enumerableExceptionActionInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionActionInterfaceType);
            actionMethodInfo = exceptionActionInterfaceType.GetMethod(nameof(IRequestExceptionAction<TRequest, Exception>.Execute))
                ?? throw new InvalidOperationException($"Could not find method {nameof(IRequestExceptionAction<TRequest, Exception>.Execute)} on type {exceptionActionInterfaceType}");

            var actionsForException = (IEnumerable<object>)_serviceFactory(enumerableExceptionActionInterfaceType);

            return HandlersOrderer.Prioritize(actionsForException.ToList(), request);
        }
    }
}
