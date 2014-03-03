namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.ServiceLocation;

    public interface IMediator
    {
        TResult Send<TResult>(ICommand<TResult> message);
        TResponse Request<TResponse>(IQuery<TResponse> query);
    }

    public class Mediator : IMediator
    {
        private readonly ServiceLocatorProvider _serviceLocatorProvider;

        public Mediator(ServiceLocatorProvider serviceLocatorProvider)
        {
            _serviceLocatorProvider = serviceLocatorProvider;
        }

        public TResult Send<TResult>(ICommand<TResult> message)
        {
            var defaultHandler = GetHandler(message);

            TResult result = defaultHandler.Handle(message);

            var resultHandlers = GetCommandResultHandlers(message);

            foreach (var resultHandler in resultHandlers)
            {
                resultHandler.Handle(message, result);
            }

            return result;
        }

        public TResponse Request<TResponse>(IQuery<TResponse> query)
        {
            var defaultHandler = GetHandler(query);

            TResponse response = defaultHandler.Handle(query);

            var responseHandlers = GetQueryResponseHandlers(query);

            foreach (var responseHandler in responseHandlers)
            {
                responseHandler.Handle(query, response);
            }

            return response;
        }

        private QueryHandler<TResponse> GetHandler<TResponse>(IQuery<TResponse> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var wrapperType = typeof(QueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var handler = _serviceLocatorProvider().GetInstance(handlerType);
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (QueryHandler<TResponse>)wrapperHandler;
        }

        private CommandHandler<TResult> GetHandler<TResult>(ICommand<TResult> message)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(message.GetType(), typeof(TResult));
            var wrapperType = typeof(CommandHandler<,>).MakeGenericType(message.GetType(), typeof(TResult));
            var handler = _serviceLocatorProvider().GetInstance(handlerType);
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (CommandHandler<TResult>)wrapperHandler;
        }

        private IEnumerable<CommandResultHandler<TResult>> GetCommandResultHandlers<TResult>(ICommand<TResult> message)
        {
            var handlerType = typeof(ICommandResultHandler<,>).MakeGenericType(message.GetType(), typeof(TResult));
            var wrapperType = typeof(CommandResultHandler<,>).MakeGenericType(message.GetType(), typeof(TResult));
            var handlers = _serviceLocatorProvider().GetAllInstances(handlerType)
                .Cast<object>()
                .Select(handler => (CommandResultHandler<TResult>)Activator.CreateInstance(wrapperType, handler));
            return handlers;
        }

        private IEnumerable<QueryResponseHandler<TResponse>> GetQueryResponseHandlers<TResponse>(IQuery<TResponse> query)
        {
            var handlerType = typeof(IQueryResponseHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var wrapperType = typeof(QueryResponseHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var handlers = _serviceLocatorProvider().GetAllInstances(handlerType)
                .Cast<object>()
                .Select(handler => (QueryResponseHandler<TResponse>)Activator.CreateInstance(wrapperType, handler));
            return handlers;
        }

        private abstract class CommandHandler<TResult>
        {
            public abstract TResult Handle(ICommand<TResult> message);
        }

        private class CommandHandler<TCommand, TResult> : CommandHandler<TResult> where TCommand : ICommand<TResult>
        {
            private readonly ICommandHandler<TCommand, TResult> _inner;

            public CommandHandler(ICommandHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(ICommand<TResult> message)
            {
                return _inner.Handle((TCommand)message);
            }
        }

        private abstract class CommandResultHandler<TResult>
        {
            public abstract void Handle(ICommand<TResult> message, TResult result);
        }

        private class CommandResultHandler<TCommand, TResult> : CommandResultHandler<TResult>
            where TCommand : ICommand<TResult>
        {
            private readonly ICommandResultHandler<TCommand, TResult> _inner;

            public CommandResultHandler(ICommandResultHandler<TCommand, TResult> inner)
            {
                _inner = inner;
            }

            public override void Handle(ICommand<TResult> message, TResult result)
            {
                _inner.Handle((TCommand)message, result);
            }
        }

        private abstract class QueryHandler<TResult>
        {
            public abstract TResult Handle(IQuery<TResult> message);
        }

        private class QueryHandler<TQuery, TResult> : QueryHandler<TResult> where TQuery : IQuery<TResult>
        {
            private readonly IQueryHandler<TQuery, TResult> _inner;

            public QueryHandler(IQueryHandler<TQuery, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(IQuery<TResult> message)
            {
                return _inner.Handle((TQuery)message);
            }
        }

        private abstract class QueryResponseHandler<TResponse>
        {
            public abstract void Handle(IQuery<TResponse> query, TResponse response);
        }

        private class QueryResponseHandler<TQuery, TResponse> : QueryResponseHandler<TResponse>
            where TQuery : IQuery<TResponse>
        {
            private readonly IQueryResponseHandler<TQuery, TResponse> _inner;

            public QueryResponseHandler(IQueryResponseHandler<TQuery, TResponse> inner)
            {
                _inner = inner;
            }

            public override void Handle(IQuery<TResponse> query, TResponse response)
            {
                _inner.Handle((TQuery)query, response);
            }
        }

    }
}