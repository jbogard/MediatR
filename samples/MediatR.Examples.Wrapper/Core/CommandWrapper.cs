namespace MediatR.Examples.Wrapper.Core
{
    public class CommandWrapper<TCommand> : IRequest
        where TCommand : ICommand
    {
        public TCommand Command { get; set; }
    }
}