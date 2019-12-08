namespace MediatR.Pipeline
{
    using MediatR.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
                    await ((Task)actionMethod.Invoke(actionForException, new object[] { request, exception, cancellationToken })).ConfigureAwait(false);
                }

                throw;
            }
        }

        private IList<object> GetActionsForException(Type exceptionType, TRequest request, out MethodInfo actionMethodInfo)
        {
            var exceptionActionInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);
            var enumerableExceptionActionInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionActionInterfaceType);
            actionMethodInfo = exceptionActionInterfaceType.GetMethod(nameof(IRequestExceptionAction<TRequest, Exception>.Execute));

            var actionsForException = (IEnumerable<object>)_serviceFactory.Invoke(enumerableExceptionActionInterfaceType);

            return HandlersOrderer.Prioritize(actionsForException.ToList(), request);
        }
    }
}
