using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.ExceptionHandler;

namespace MediatR.Examples.ExceptionHandler;

public class LogExceptionAction : IRequestResponseExceptionAction<Ping, Pong>
{
    private readonly TextWriter _writer;

    public LogExceptionAction(TextWriter writer) => _writer = writer;

    public Task Execute(Ping request, Exception exception, CancellationToken cancellationToken) 
        => _writer.WriteLineAsync($"--- Exception: '{exception.GetType().FullName}'");
}