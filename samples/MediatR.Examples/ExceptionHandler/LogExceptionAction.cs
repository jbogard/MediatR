using MediatR.Pipeline;
using System;
using System.IO;

namespace MediatR.Examples.ExceptionHandler
{
    public class LogExceptionAction : RequestExceptionAction<Ping>
    {
        private readonly TextWriter _writer;

        public LogExceptionAction(TextWriter writer) => _writer = writer;

        protected override void Execute(Ping request, Exception exception)
        {
            _writer.WriteLineAsync($"--- Exception: '{exception.GetType().FullName}'");
        }
    }
}
