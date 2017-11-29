namespace MediatR.Examples
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Runner
    {
        public static async Task Run(IMediator mediator, TextWriter writer, string projectName)
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
        }
    }
}