using MediatR.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.ExceptionHandler
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

    public class ConnectionExceptionHandler : IRequestExceptionHandler<PingResource, Pong, ConnectionException>
    {
        private readonly TextWriter _writer;

        public ConnectionExceptionHandler(TextWriter writer) => _writer = writer;

        public Task Handle(PingResource request,
            ConnectionException exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            _writer.WriteLineAsync($"---- Exception Handler: '{typeof(ConnectionExceptionHandler).FullName}'");
            state.SetHandled(new Pong());
            return Task.CompletedTask;
        }
    }

    public class AccessDeniedExceptionHandler : IRequestExceptionHandler<PingResource, Pong, ForbiddenException>
    {
        private readonly TextWriter _writer;

        public AccessDeniedExceptionHandler(TextWriter writer) => _writer = writer;

        public Task Handle(PingResource request,
            ForbiddenException exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            _writer.WriteLineAsync($"---- Exception Handler: '{typeof(AccessDeniedExceptionHandler).FullName}'");
            state.SetHandled(new Pong());
            return Task.CompletedTask;
        }
    }

    public class ServerExceptionHandler : IRequestExceptionHandler<PingNewResource, Pong, ServerException>
    {
        private readonly TextWriter _writer;

        public ServerExceptionHandler(TextWriter writer) => _writer = writer;

        public virtual Task Handle(PingNewResource request,
            ServerException exception,
            RequestExceptionHandlerState<Pong> state,
            CancellationToken cancellationToken)
        {
            _writer.WriteLineAsync($"---- Exception Handler: '{typeof(ServerExceptionHandler).FullName}'");
            state.SetHandled(new Pong());
            return Task.CompletedTask;
        }
    }
}
