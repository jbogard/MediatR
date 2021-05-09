namespace MediatR.Pipeline.Streams
{
    using MediatR.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IStreamRequestExceptionHandler{TRequest,TResponse,TException}"/>
    ///     or <see cref="StreamRequestExceptionHandler{TRequest,TResponse}"/> instances
    ///     after an exception is thrown by the following pipeline steps
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class StreamRequestExceptionActionProcessorBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ServiceFactory _serviceFactory;

        public StreamRequestExceptionActionProcessorBehavior(ServiceFactory serviceFactory) => _serviceFactory = serviceFactory;

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken, StreamHandlerDelegate<TResponse> next)
        {
            var asyncEnum = next().WithCancellation(cancellationToken).ConfigureAwait(false).GetAsyncEnumerator();

            try
            {
                bool stop = false;
                while (!stop)
                {
                    try
                    {
                        stop = !(await asyncEnum.MoveNextAsync());
                    }
                    catch (Exception exception)
                    {
                        var actionsForException = GetActionsForException(exception.GetType(), request, out MethodInfo actionMethod);

                        foreach (var actionForException in actionsForException)
                        {
                            await ((Task) actionMethod.Invoke(actionForException, new object[] { request, exception, cancellationToken })).ConfigureAwait(false);
                        }

                        throw;
                    }

                    yield return asyncEnum.Current;
                }
            }
            finally
            {
                await asyncEnum.DisposeAsync();
            }
        }

        private IList<object> GetActionsForException(Type exceptionType, TRequest request, out MethodInfo actionMethodInfo)
        {
            var exceptionActionInterfaceType = typeof(IStreamRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);
            var enumerableExceptionActionInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionActionInterfaceType);
            actionMethodInfo = exceptionActionInterfaceType.GetMethod(nameof(IStreamRequestExceptionAction<TRequest, Exception>.Execute));

            var actionsForException = (IEnumerable<object>) _serviceFactory.Invoke(enumerableExceptionActionInterfaceType);

            return HandlersOrderer.Prioritize(actionsForException.ToList(), request);
        }
    }
}
