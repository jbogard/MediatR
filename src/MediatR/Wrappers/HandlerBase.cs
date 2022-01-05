namespace MediatR.Wrappers;

using System;

public abstract class HandlerBase
{
    protected HandlerBase() { }

    protected static THandler GetHandler<THandler>(ServiceFactory factory)
    {
        THandler handler;

        try
        {
            handler = factory.GetInstance<THandler>();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
        }

        if (handler == null)
        {
            throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
        }

        return handler;
    }
}