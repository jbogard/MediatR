namespace MediatR.ExceptionHandling;

public sealed class RequestExceptionHandlerState
{
    public bool IsHandled { get; private set; }

    public void SetHandled() =>
        IsHandled = true;
}