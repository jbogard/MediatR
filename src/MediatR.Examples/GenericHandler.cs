namespace MediatR.Examples
{
    using System.IO;

    public class GenericHandler : INotificationHandler<INotification>
    {
        private readonly TextWriter _writer;

        public GenericHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void Handle(INotification notification)
        {
            _writer.WriteLine("Got notified.");
        }
    }
}