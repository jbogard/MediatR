using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.Wrapper.Core
{
    public class Mediate : IMediate
    {
        private readonly IMediator mediator;

        public Mediate(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Task Handle<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : ICommand
        {
            return mediator.Send(new CommandWrapper<TCommand> { Command = command }, cancellationToken);
        }

        public Task<TResponse> Handle<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default(CancellationToken))
            where TQuery : IQuery<TResponse>
        {
            return mediator.Send(new QueryWrapper<TQuery, TResponse> { Query = query }, cancellationToken);
        }
    }
}