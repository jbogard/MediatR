namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.ServiceLocation;

    public interface IMediator
    {
        TResponse Send<TResponse>(IRequest<TResponse> request);
    }

    public class Mediator : IMediator
    {
        private readonly ServiceLocatorProvider _serviceLocatorProvider;

        public Mediator(ServiceLocatorProvider serviceLocatorProvider)
        {
            _serviceLocatorProvider = serviceLocatorProvider;
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            var defaultHandler = GetHandler(request);

            TResponse result = defaultHandler.Handle(request);

            var resultHandlers = GetPostRequestHandlers(request);

            foreach (var resultHandler in resultHandlers)
            {
                resultHandler.Handle(request, result);
            }

            return result;
        }

        private RequestHandler<TResponse> GetHandler<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(RequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceLocatorProvider().GetInstance(handlerType);
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (RequestHandler<TResponse>)wrapperHandler;
        }

        private IEnumerable<PostRequestHandler<TResponse>> GetPostRequestHandlers<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IPostRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var wrapperType = typeof(PostRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handlers = _serviceLocatorProvider().GetAllInstances(handlerType)
                .Cast<object>()
                .Select(handler => (PostRequestHandler<TResponse>)Activator.CreateInstance(wrapperType, handler));
            return handlers;
        }
        private abstract class RequestHandler<TResult>
        {
            public abstract TResult Handle(IRequest<TResult> message);
        }

        private class RequestHandler<TCommand, TResult> : RequestHandler<TResult> where TCommand : IRequest<TResult>
        {
            private readonly IRequestHandler<TCommand, TResult> _inner;

            public RequestHandler(IRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(IRequest<TResult> message)
            {
                return _inner.Handle((TCommand)message);
            }
        }

        private abstract class PostRequestHandler<TResult>
        {
            public abstract void Handle(IRequest<TResult> message, TResult result);
        }

        private class PostRequestHandler<TCommand, TResult> : PostRequestHandler<TResult>
            where TCommand : IRequest<TResult>
        {
            private readonly IPostRequestHandler<TCommand, TResult> _inner;

            public PostRequestHandler(IPostRequestHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override void Handle(IRequest<TResult> message, TResult result)
            {
                _inner.Handle((TCommand)message, result);
            }
        }
    }
}