using MediatR.Examples.Wrapper.Core;

namespace MediatR.Examples.Wrapper.Commands
{
    //Custom ICommand interface to allow you to remove direct MediatR refs
    public class JingCommand : ICommand
    {
        public string Message { get; set; }
    }
}
