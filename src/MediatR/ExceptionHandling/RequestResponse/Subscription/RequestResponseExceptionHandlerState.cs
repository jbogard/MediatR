namespace MediatR.ExceptionHandling.RequestResponse.Subscription;

/// <summary>
/// Represents the result of handling an exception thrown by a request handler
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public sealed class RequestResponseExceptionHandlerState<TResponse>
{
    /// <summary>
    /// Gets the value indicating whenever the exception was handled by a handler or not.
    /// </summary>
    public bool IsHandled { get; private set; }
    
    /// <summary>
    /// Gets the response when handled; else null.
    /// </summary>
    public TResponse? Response { get; private set; }

    /// <summary>
    /// Sets the exception as handled.
    /// </summary>
    /// <param name="response">The alternative response for the request.</param>
    public void SetHandled(TResponse response)
    {
        IsHandled = true;
        Response = response;
    }
}