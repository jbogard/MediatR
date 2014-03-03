namespace MediatR
{
    public interface ICommandHandler<in TMessage, out TResult>
        where TMessage : ICommand<TResult>
    {
        TResult Handle(TMessage message);
    }

    public abstract class CommandHandler<TMessage> : ICommandHandler<TMessage, Unit>
        where TMessage : ICommand
    {
        public Unit Handle(TMessage message)
        {
            HandleCore(message);

            return Unit.Value;
        }

        protected abstract void HandleCore(TMessage message);
    }

    public interface IQueryHandler<in TQuery, out TResponse>
        where TQuery : IQuery<TResponse>
    {
        TResponse Handle(TQuery query);
    }

    public interface ICommandResultHandler<in TMessage, in TResult>
    {
        void Handle(TMessage message, TResult result);
    }

    public interface IQueryResponseHandler<in TQuery, in TResponse>
    {
        void Handle(TQuery message, TResponse result);
    }
}