using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.Wrapper.Core
{
    public interface ICommandMediator
    {
        Task Handle<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : ICommand;
    }
}