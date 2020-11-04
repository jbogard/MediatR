namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class StreamRequestHandlerBase
    {
        public abstract IAsyncEnumerable<object?> Handle(object request, CancellationToken cancellationToken, ServiceFactory serviceFactory);

        protected static THandler GetHandler<THandler>(ServiceFactory factory)
        {
            THandler handler;

            try
            {
                handler = factory.GetInstance<THandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
            }

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
            }

            return handler;
        }
    }

    internal abstract class StreamRequestHandlerWrapper<TResponse> : StreamRequestHandlerBase
    {
        public abstract IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
            ServiceFactory serviceFactory);
    }

    internal class StreamRequestHandlerWrapperImpl<TRequest, TResponse> : StreamRequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async override IAsyncEnumerable<object?> Handle(object request, [EnumeratorCancellation] CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            await foreach (TResponse item in this.Handle((IRequest<TResponse>) request, cancellationToken, serviceFactory))
            {
                yield return item;
            }
        }

        public override IAsyncEnumerable<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken, ServiceFactory serviceFactory)
        {
            var streamHandler = GetHandler<IStreamRequestHandler<TRequest, TResponse>>(serviceFactory);
            return streamHandler.Handle((TRequest) request, cancellationToken);
        }
    }
}
