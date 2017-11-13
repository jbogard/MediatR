namespace MediatR
{
    /// <summary>
    /// Marker interface to represent a request
    /// </summary>
    public interface IRequest { }

    /// <summary>
    /// Marker interface to represent a request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequest<out TResponse> : IRequest { }
}