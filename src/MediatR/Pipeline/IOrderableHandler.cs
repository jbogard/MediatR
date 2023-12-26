namespace MediatR.Pipeline;

/// <summary>
/// Optional interface that can be used to decorate any
/// existing handlers to add an Order property
/// </summary>
public interface IOrderableHandler
{
    /// <summary>
    /// Defines the order this handler should execute relative to others.  Smaller numbers
    /// execute sooner, and negative numbers are supported.
    /// </summary>
    int Order { get; }
}
