namespace MediatR.DependencyInjection.Configuration;

/// <summary>
/// Defines when the exception action handlers should be invoked.
/// </summary>
public enum RequestExceptionActionProcessorStrategy
{
    /// <summary>
    /// Defines that the exception action handlers should only be called for unhandled exception from the exception handlers.
    /// </summary>
    ApplyForUnhandledExceptions,

    /// <summary>
    /// Defines that the exception action handlers should be called for every exception.
    /// </summary>
    ApplyForAllExceptions
}