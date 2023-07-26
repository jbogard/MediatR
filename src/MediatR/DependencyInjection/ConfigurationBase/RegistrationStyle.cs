namespace MediatR.DependencyInjection.ConfigurationBase;

/// <summary>
/// Defines ways how the Assembly scanner should register MediatR services that where found.
/// </summary>
public enum RegistrationStyle
{
    /// <summary>
    /// Indicates that all services should have there own instance.
    /// </summary>
    EachServiceOneInstance,

    /// <summary>
    /// Indicates that all services should be registered as singletons. When a type has multiple services then there will be mapped to the single instance.
    /// </summary>
    OneInstanceForeachService
}