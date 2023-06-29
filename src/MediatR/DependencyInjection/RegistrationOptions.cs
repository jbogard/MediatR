using MediatR.Abstraction;

namespace MediatR.DependencyInjection;

public enum RegistrationOptions
{
    /// <summary>
    /// Registers everything as a transient except MediatR specific implementations like <see cref="IMediator"/>.
    /// </summary>
    Transient,

    /// <summary>
    /// Registers everything what is possible as singleton and maps the interfaces with the type.
    /// </summary>
    SingletonAndMapped,
}