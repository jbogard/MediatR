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
            //var pong = await mediator.Send(new Ping { Message = "Ping" });
            var pong = await mediator.Send<Ping, Pong>(new Ping { Message = "<Ping to Pong>" });

            await writer.WriteLineAsync("Received: " + pong.Message);

            await writer.WriteLineAsync("Publishing Pinged...");
            await mediator.Publish(new Pinged());

            await writer.WriteLineAsync("Publishing Ponged...");
            var failedPong = false;
            try
            {
                await mediator.Publish(new Ponged());
            }
            catch (Exception e)
            {
                failedPong = true;
                await writer.WriteLineAsync(e.ToString());
            }

            bool failedJing = false;
            await writer.WriteLineAsync("Sending Jing...");
            try
            {
                await mediator.Send(new Jing { Message = "Jing" });
            }
            catch (Exception e)
            {
                failedJing = true;
                await writer.WriteLineAsync(e.ToString());
            }

            await writer.WriteLineAsync("---------------");
            var contents = writer.Contents;
            var order = new[] {
                contents.IndexOf("- Starting Up", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Handling Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("--- Handled Ping", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Finished Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("- All Done", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("- All Done with Ping", StringComparison.OrdinalIgnoreCase),
            };

            var results = new RunResults
            {
                RequestHandlers = contents.Contains("--- Handled Ping:"),
                VoidRequestsHandlers = contents.Contains("--- Handled Jing:"),
                PipelineBehaviors = contents.Contains("-- Handling Request"),
                RequestPreProcessors = contents.Contains("- Starting Up"),
                RequestPostProcessors = contents.Contains("- All Done"),
                ConstrainedGenericBehaviors = contents.Contains("- All Done with Ping") && !failedJing,
                OrderedPipelineBehaviors = order.SequenceEqual(order.OrderBy(i => i)),
                NotificationHandler = contents.Contains("Got pinged async"),
                MultipleNotificationHandlers = contents.Contains("Got pinged async") && contents.Contains("Got pinged also async"),
                ConstrainedGenericNotificationHandler = contents.Contains("Got pinged constrained async") && !failedPong,
                CovariantNotificationHandler = contents.Contains("Got notified")
            };

            await writer.WriteLineAsync($"Request Handler...................{(results.RequestHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Void Request Handler..............{(results.VoidRequestsHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pipeline Behavior.................{(results.PipelineBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pre-Processor.....................{(results.RequestPreProcessors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Post-Processor....................{(results.RequestPostProcessors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Constrained Post-Processor........{(results.ConstrainedGenericBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Ordered Behaviors.................{(results.OrderedPipelineBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handler..............{(results.NotificationHandler ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handlers.............{(results.MultipleNotificationHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Constrained Notification Handler..{(results.ConstrainedGenericNotificationHandler ? "Y" : "N")}");
            await writer.WriteLineAsync($"Covariant Notification Handler....{(results.CovariantNotificationHandler ? "Y" : "N")}");
        }
    }

    public class RunResults
    {
        public bool RequestHandlers { get; set; }
        public bool VoidRequestsHandlers { get; set; }
        public bool PipelineBehaviors { get; set; }
        public bool RequestPreProcessors { get; set; }
        public bool RequestPostProcessors { get; set; }
        public bool OrderedPipelineBehaviors { get; set; }
        public bool ConstrainedGenericBehaviors { get; set; }
        public bool NotificationHandler { get; set; }
        public bool MultipleNotificationHandlers { get; set; }
        public bool CovariantNotificationHandler { get; set; }
        public bool ConstrainedGenericNotificationHandler { get; set; }
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