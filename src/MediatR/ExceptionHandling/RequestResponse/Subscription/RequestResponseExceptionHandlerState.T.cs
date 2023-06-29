namespace MediatR.ExceptionHandling;

/// <summary>
/// Represents the result of handling an exception thrown by a request handler
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public sealed class RequestResponseExceptionHandlerState<TResponse> : RequestResponseExceptionHandlerState
{
    /// <summary>
    /// Gets the response when handled; else null.
    /// </summary>
    public new TResponse? Response => (TResponse?) ((RequestResponseExceptionHandlerState)this).Response;

    /// <summary>
    /// Call to indicate whether the current exception should be considered handled and the specified response should be returned.
    /// </summary>
    /// <param name="response">Set the response that will be returned.</param>
    public void SetHandled(TResponse response) =>
        ((RequestResponseExceptionHandlerState)this).SetHandled(response);
}