using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExceptionHandling.RequestResponse.Subscription;

namespace MediatR.Examples.ExceptionHandler.Overrides;

public class CommonExceptionHandler : IRequestResponseExceptionHandler<ExceptionHandler.PingResourceTimeout, Pong, Exception>
{
    private readonly TextWriter _writer;

    public CommonExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(
        ExceptionHandler.PingResourceTimeout request,
        Exception exception,
        RequestResponseExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        // Exception type name must be written in messages by LogExceptionAction before
        // Exception handler type name required because it is checked later in messages
        await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(CommonExceptionHandler).FullName}'").ConfigureAwait(false);

        state.SetHandled(new Pong
        {
            Message = string.Empty
        });
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

        state.SetHandled(new Pong
        {
            Message = string.Empty
        });
    }
}
