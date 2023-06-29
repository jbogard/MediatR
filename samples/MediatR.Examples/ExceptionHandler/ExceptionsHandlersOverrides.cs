using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExceptionHandling;

namespace MediatR.Examples.ExceptionHandler.Overrides;

public class CommonExceptionHandler : IRequestResponseExceptionHandler<PingResourceTimeout, Pong>
{
    private readonly TextWriter _writer;

    public CommonExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(PingResourceTimeout request,
        Exception exception,
        RequestResponseExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        // Exception type name must be written in messages by LogExceptionAction before
        // Exception handler type name required because it is checked later in messages
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
        RequestResponseExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        // Exception type name must be written in messages by LogExceptionAction before
        // Exception handler type name required because it is checked later in messages
        await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(ServerExceptionHandler).FullName}'").ConfigureAwait(false);
        
        state.SetHandled(new Pong());
    }
}
