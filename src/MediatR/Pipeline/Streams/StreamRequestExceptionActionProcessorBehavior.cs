namespace MediatR.Pipeline.Streams
{
    using MediatR.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

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

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation]CancellationToken cancellationToken, StreamHandlerDelegate<TResponse> next)
        {
            // Bumping into Error	CS1626	Cannot yield a value in the body of a try block with a catch clause
            // See https://stackoverflow.com/questions/346365/why-cant-yield-return-appear-inside-a-try-block-with-a-catch
            await foreach (var result in next())
                {
                    yield return result;
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
