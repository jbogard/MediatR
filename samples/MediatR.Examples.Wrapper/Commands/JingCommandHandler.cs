using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Examples.Wrapper.Core;

namespace MediatR.Examples.Wrapper.Commands
{
    public class JingCommandHandler : CommandHandler<JingCommand>
    {
        private readonly TextWriter _writer;

        public JingCommandHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public override Task Handle(JingCommand command, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync($"--- Handled JingCommand: {command.Message}, no Jong");
        }
    }
}
