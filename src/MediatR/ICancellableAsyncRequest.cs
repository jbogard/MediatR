namespace MediatR
{
    /// <summary>
    /// Marker interface to represent a cancellable, asynchronous request with a void response
    /// </summary>
    public interface ICancellableAsyncRequest : ICancellableAsyncRequest<Unit> { }

    /// <summary>
    /// Marker interface to represent a cancellable, asynchronous request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface ICancellableAsyncRequest<out TResponse> { }
}