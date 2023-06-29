namespace MediatR.ExceptionHandling;

public abstract class RequestResponseExceptionHandlerState
{
    public bool IsHandled { get; private set; }

    public object? Response { get; private set; }

    public void SetHandled(object? response)
    {
        IsHandled = true;
        Response = response;
    }
}