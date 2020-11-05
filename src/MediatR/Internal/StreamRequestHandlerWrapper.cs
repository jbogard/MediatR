namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class StreamRequestHandlerWrapper<TResponse> : HandlerBase
    {
        public abstract IAsyncEnumerable<object?> Handle(object request, CancellationToken cancellationToken, ServiceFactory serviceFactory);

        public abstract IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    internal class StreamRequestHandlerWrapperImpl<TRequest, TResponse> : StreamRequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async override IAsyncEnumerable<object?> Handle(object request, [EnumeratorCancellation] CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            await foreach (var item in Handle((IRequest<TResponse>) request, cancellationToken, serviceFactory))
            {
                yield return item;
            }
        }

        public override IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            IAsyncEnumerable<TResponse> Handler() => GetHandler<IStreamRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);

            return serviceFactory
                .GetInstances<IStreamPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate((StreamHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest) request, cancellationToken, next))();
        }
    }
}
