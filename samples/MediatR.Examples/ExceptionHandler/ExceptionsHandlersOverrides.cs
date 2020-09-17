using MediatR.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.ExceptionHandler.Overrides
{
    public class CommonExceptionHandler : AsyncRequestExceptionHandler<PingResourceTimeout, Pong>
    {
        private readonly TextWriter _writer;

        public CommonExceptionHandler(TextWriter writer) => _writer = writer;

        protected override async Task Handle(PingResourceTimeout request,
            Exception exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(CommonExceptionHandler).FullName}'").ConfigureAwait(false);
            state.SetHandled(new Pong());
        }
    }

    public class ServerExceptionHandler : ExceptionHandler.ServerExceptionHandler
    {
        private readonly TextWriter _writer;

        public ServerExceptionHandler(TextWriter writer) : base(writer) => _writer = writer;

        public override async Task Handle(PingNewResource request,
            ServerException exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(ServerExceptionHandler).FullName}'").ConfigureAwait(false);
            state.SetHandled(new Pong());
        }
    }
}
