namespace MediatR.Examples
{
    public class Ping : IRequest<Pong>
    {
        public string Message { get; set; }
    }

    public class BroadcastPing : IRequest<Pong>
    {
        public string Message { get; set; }
    }


    public class GenericResponse
    {
        public object Body { get; set; }
    }

    public class GenericRequest : IRequest<GenericResponse>
    {
        public object Body { get; set; }
    }
}