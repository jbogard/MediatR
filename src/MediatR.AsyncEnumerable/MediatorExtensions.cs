#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MediatR.AsyncEnumerable
{
    public static class MediatorExtensions
    {
        private static readonly ConcurrentDictionary<Type, AsyncEnumerableRequestHandlerBase> _asyncEnumerableRequestHandlers = new();

        public static async IAsyncEnumerable<TResponse> ToAsyncEnumerable<TResponse>(
            this IMediator mediator,
            IRequest<TResponse> request, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            var handler = (AsyncEnumerableRequestHandlerWrapper<TResponse>) _asyncEnumerableRequestHandlers.GetOrAdd(requestType,
                t => (AsyncEnumerableRequestHandlerBase) Activator.CreateInstance(typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse))));

            await foreach ( var item in  handler.Handle(request, cancellationToken, mediator.ServiceFactory) )
            {
                yield return item;
            }
        }


        public static async IAsyncEnumerable<object?> ToAsyncEnumerable(
            this IMediator mediator,
            object request, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            var handler = _asyncEnumerableRequestHandlers.GetOrAdd(requestType,
                requestTypeKey =>
                {
                    var requestInterfaceType = requestTypeKey
                        .GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                    var isValidRequest = requestInterfaceType != null;

                    if (!isValidRequest)
                    {
                        throw new ArgumentException($"{requestType.Name} does not implement {nameof(IRequest)}", nameof(requestTypeKey));
                    }

                    var responseType = requestInterfaceType!.GetGenericArguments()[0];
                    return (AsyncEnumerableRequestHandlerBase) Activator.CreateInstance(typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestTypeKey, responseType));
                });

            await foreach (var item in  handler.Handle(request, cancellationToken, mediator.ServiceFactory) )
            {
                yield return item;
            }
        }

    }
}
