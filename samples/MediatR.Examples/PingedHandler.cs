namespace MediatR.Examples
{
    using System.IO;

    public class PingedHandler : INotificationHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void Handle(Pinged notification)
        {
            _writer.WriteLine("Got pinged.");
        }
    }

    public class PingedAlsoHandler : INotificationHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedAlsoHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void Handle(Pinged notification)
        {
            _writer.WriteLine("Got pinged also.");
        }
    }
}