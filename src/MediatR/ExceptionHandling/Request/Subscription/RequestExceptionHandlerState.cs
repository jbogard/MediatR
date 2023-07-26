namespace MediatR.ExceptionHandling.Request.Subscription;

/// <summary>
/// Defines the state for the current exception that is handled.
/// </summary>
public sealed class RequestExceptionHandlerState
{
    /// <summary>
    /// Get the value indicating whenever the exception was handled.
    /// </summary>
    public bool IsHandled { get; private set; }

    /// <summary>
    /// Sets the exception as handled.
    /// </summary>
    public void SetHandled() =>
        IsHandled = true;
}