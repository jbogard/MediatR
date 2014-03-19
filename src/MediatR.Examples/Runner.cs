namespace MediatR.Examples
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Runner
    {
        public static void Run(IMediator mediator, TextWriter writer)
        {
            writer.WriteLine("Sample mediator implementation using send, publish and post-request handlers in sync and async version.");
            writer.WriteLine("---------------");
            
            writer.WriteLine("Sending Ping...");
            var pong = mediator.Send(new Ping { Message = "Ping" });
            writer.WriteLine("Received: " + pong.Message);

            writer.WriteLine("Sending Ping async...");
            var response = mediator.SendAsync(new PingAsync { Message = "Ping" });
            Task.WaitAll(response);
            writer.WriteLine("Received async: " + response.Result.Message);

            writer.WriteLine("Publishing Pinged...");
            mediator.Publish(new Pinged());

            writer.WriteLine("Publishing Pinged async...");
            var publishResponse = mediator.PublishAsync(new PingedAsync());
            Task.WaitAll(publishResponse);
        }
    }
}