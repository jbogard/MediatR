using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.Wrapper.Core
{
    public abstract class CommandHandler<TCommand> : AsyncRequestHandler<CommandWrapper<TCommand>>
        where TCommand : ICommand
    {
        public abstract Task Handle(TCommand command, CancellationToken cancellationToken);

        protected override Task Handle(CommandWrapper<TCommand> request, CancellationToken cancellationToken)
        {
            return Handle(request.Command, cancellationToken);
        }
    }
}