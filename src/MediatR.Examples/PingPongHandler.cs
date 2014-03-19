namespace MediatR.Examples
{
    using System.IO;

    public class PingPongHandler : IPostRequestHandler<Ping, Pong>
    {
        public void Handle(Ping request, Pong response)
        {
            response.Message = response.Message + " Pung";
        }
    }
    public class GenericPostRequestHandler<TRequest, TResponse> : IPostRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericPostRequestHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void Handle(TRequest request, TResponse response)
        {
            _writer.WriteLine("Inside generic post request handler...");
        }
    }
}