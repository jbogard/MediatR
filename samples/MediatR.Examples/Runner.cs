using System;
using System.Linq;
using System.Text;

namespace MediatR.Examples
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Runner
    {
        public static async Task Run(IMediator mediator, WrappingWriter writer, string projectName)
        {
            await writer.WriteLineAsync("===============");
            await writer.WriteLineAsync(projectName);
            await writer.WriteLineAsync("===============");

            await writer.WriteLineAsync("Sending Ping...");
            var pong = await mediator.Send(new Ping { Message = "Ping" });
            await writer.WriteLineAsync("Received: " + pong.Message);

            await writer.WriteLineAsync("Publishing Pinged...");
            await mediator.Publish(new Pinged());

            await writer.WriteLineAsync("Sending Jing...");
            await mediator.Send(new Jing { Message = "Jing" });

            await writer.WriteLineAsync("---------------");
            var contents = writer.Contents;
            await writer.WriteLineAsync($"Request Handler...................{(contents.Contains("--- Handled Ping:") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Void Request Handler..............{(contents.Contains("--- Handled Jing:") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pipeline Behavior.................{(contents.Contains("-- Handling Request") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pre-Processor.....................{(contents.Contains("- Starting Up") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Post-Processor....................{(contents.Contains("- All Done") ? "Y" : "N")}");
            var order = new[] {
                contents.IndexOf("- Starting Up", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Handling Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("--- Handled Ping", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Finished Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("- All Done", StringComparison.OrdinalIgnoreCase),
                };
            var isOrdered = order.SequenceEqual(order.OrderBy(i => i));
            await writer.WriteLineAsync($"Ordered Behaviors.................{(isOrdered ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handler..............{(contents.Contains("Got pinged async") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handlers.............{(contents.Contains("Got pinged async") && contents.Contains("Got pinged also async") ? "Y" : "N")}");
            await writer.WriteLineAsync($"Covariant Notification Handler....{(contents.Contains("Got notified") ? "Y" : "N")}");
        }
    }

    public class WrappingWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;
        private readonly StringBuilder _stringWriter = new StringBuilder();

        public WrappingWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public override void Write(char value)
        {
            _stringWriter.Append(value);
            _innerWriter.Write(value);
        }

        public override Task WriteLineAsync(string value)
        {
            _stringWriter.AppendLine(value);
            return _innerWriter.WriteLineAsync(value);
        }

        public override Encoding Encoding => _innerWriter.Encoding;

        public string Contents => _stringWriter.ToString();
    }

}