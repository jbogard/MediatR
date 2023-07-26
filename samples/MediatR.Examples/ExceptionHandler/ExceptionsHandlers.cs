using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.ExceptionHandling.RequestResponse.Subscription;

namespace MediatR.Examples.ExceptionHandler;

public class CommonExceptionHandler : IRequestResponseExceptionHandler<PingResource, Pong, Exception>
{
    private readonly TextWriter _writer;

    public CommonExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(PingResource request,
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

public class ConnectionExceptionHandler : IRequestResponseExceptionHandler<PingResource, Pong, ConnectionException>
{
    private readonly TextWriter _writer;

    public ConnectionExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(PingResource request,
        ConnectionException exception,
        RequestResponseExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        // Exception type name must be written in messages by LogExceptionAction before
        // Exception handler type name required because it is checked later in messages
        await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(ConnectionExceptionHandler).FullName}'").ConfigureAwait(false);
        
        state.SetHandled(new Pong
        {
            Message = string.Empty
        });
    }
}

public class AccessDeniedExceptionHandler : IRequestResponseExceptionHandler<PingResource, Pong, ForbiddenException>
{
    private readonly TextWriter _writer;

    public AccessDeniedExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(PingResource request,
        ForbiddenException exception,
        RequestResponseExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        // Exception type name must be written in messages by LogExceptionAction before
        // Exception handler type name required because it is checked later in messages
        await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(AccessDeniedExceptionHandler).FullName}'").ConfigureAwait(false);
        
        state.SetHandled(new Pong
        {
            Message = string.Empty
        });
    }
}

public class ServerExceptionHandler : IRequestResponseExceptionHandler<PingNewResource, Pong, ServerException>
{
    private readonly TextWriter _writer;

    public ServerExceptionHandler(TextWriter writer) => _writer = writer;

    public virtual async Task Handle(PingNewResource request,
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
