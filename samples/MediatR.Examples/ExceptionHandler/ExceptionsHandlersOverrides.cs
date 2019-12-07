using MediatR.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.ExceptionHandler.Overrides
{
    public class CommonExceptionHandler : RequestExceptionHandler<PingResource, Pong>
    {
        private readonly TextWriter _writer;

        public CommonExceptionHandler(TextWriter writer) => _writer = writer;

        public override Task Handle(PingResource request,
            Exception exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            _writer.WriteLineAsync($"---- Exception Handler: '{typeof(CommonExceptionHandler).FullName}'");
            state.SetHandled(new Pong());
            return Task.CompletedTask;
        }
    }
}
