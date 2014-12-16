namespace MediatR
{
    /// <summary>
    /// Marker interface to represent a request with a void response
    /// </summary>
    public interface IRequest : IRequest<Unit> { }

    /// <summary>
    /// Marker interface to represent an asynchronous request with a void response
    /// </summary>
    public interface IAsyncRequest : IAsyncRequest<Unit> { }

    /// <summary>
    /// Marker interface to represent a request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequest<out TResponse> { }

    /// <summary>
    /// Marker interface to represent an asynchronous request with a void response
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IAsyncRequest<out TResponse> { }

    /// <summary>
    /// Marker interface to represent a notification
    /// </summary>
    public interface INotification { }

    /// <summary>
    /// Marker interface to represent an asynchronous notification
    /// </summary>
    public interface IAsyncNotification { }
}