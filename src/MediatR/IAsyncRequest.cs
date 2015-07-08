namespace MediatR
{
    /// <summary>
    /// Marker interface to represent an asynchronous request with a void response
    /// </summary>
    public interface IAsyncRequest : IAsyncRequest<Unit> { }

    /// <summary>
    /// Marker interface to represent an asynchronous request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IAsyncRequest<out TResponse> { }
}