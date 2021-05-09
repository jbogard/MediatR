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
    public class StreamRequestExceptionProcessorBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ServiceFactory _serviceFactory;

        public StreamRequestExceptionProcessorBehavior(ServiceFactory serviceFactory) => _serviceFactory = serviceFactory;

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken, StreamHandlerDelegate<TResponse> next)
        {
            var asyncEnum = next().WithCancellation(cancellationToken).ConfigureAwait(false).GetAsyncEnumerator();

            StreamRequestExceptionHandlerState<TResponse>? state = null;

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
                        state = new StreamRequestExceptionHandlerState<TResponse>();
                        Type? exceptionType = null;

                        while (!state.Handled && exceptionType != typeof(Exception))
                        {
                            exceptionType = exceptionType == null ? exception.GetType() : exceptionType.BaseType;
                            var exceptionHandlers = GetExceptionHandlers(request, exceptionType, out MethodInfo handleMethod);

                            foreach (var exceptionHandler in exceptionHandlers)
                            {
                                await ((Task) handleMethod.Invoke(exceptionHandler, new object[] { request, exception, state, cancellationToken })).ConfigureAwait(false);

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
                    }

                    if (state != null && state.Response != null)
                    {
                        yield return state.Response;
                        yield break;
                    }
                    else
                    {
                        yield return asyncEnum.Current;
                    }
                }
            }
            finally
            {
                await asyncEnum.DisposeAsync();
            }
        }


        private IList<object> GetExceptionHandlers(TRequest request, Type exceptionType, out MethodInfo handleMethodInfo)
        {
            var exceptionHandlerInterfaceType = typeof(IStreamRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
            var enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
            handleMethodInfo = exceptionHandlerInterfaceType.GetMethod(nameof(IStreamRequestExceptionHandler<TRequest, TResponse, Exception>.Handle));

            var exceptionHandlers = (IEnumerable<object>) _serviceFactory.Invoke(enumerableExceptionHandlerInterfaceType);

            return HandlersOrderer.Prioritize(exceptionHandlers.ToList(), request);
        }
    }
}
