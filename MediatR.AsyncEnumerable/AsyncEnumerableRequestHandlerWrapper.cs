#nullable enable

namespace MediatR.AsyncEnumerable
{
    using MediatR.Wrappers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class AsyncEnumerableRequestHandlerBase : HandlerBase
    {
        public abstract IAsyncEnumerable<object?> Handle(object request, CancellationToken cancellationToken, ServiceFactory serviceFactory);
    }

    internal abstract class AsyncEnumerableRequestHandlerWrapper<TResponse> : AsyncEnumerableRequestHandlerBase
    {
        public abstract IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    internal class StreamRequestHandlerWrapperImpl<TRequest, TResponse> : AsyncEnumerableRequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async override IAsyncEnumerable<object?> Handle(object request, [EnumeratorCancellation] CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            await foreach (var item in Handle((IRequest<TResponse>) request, cancellationToken, serviceFactory))
            {
                yield return item;
            }
        }

        public override async IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            IAsyncEnumerable<TResponse> Handler() => GetHandler<IAsyncEnumerableRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);

            var items = serviceFactory
                .GetInstances<IAsyncEnumerablePipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate(
                    (AsyncEnumerableHandlerDelegate<TResponse>) Handler,
                    (next, pipeline) => () => pipeline.Handle(
                                                        (TRequest) request,
                                                        cancellationToken,
                                                        new AsyncEnumerableHandlerDelegate<TResponse>(
                                                            () => NextWrapper(next(), cancellationToken))
                                )
                           )();

            await foreach (var item in items)
            {
                yield return item;
            }
        }


        private static async IAsyncEnumerable<T> NextWrapper<T>(
            IAsyncEnumerable<T> items,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var cancellable = items
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false);
            await foreach (var item in cancellable)
            {
                yield return item;
            }
        }

    }
}