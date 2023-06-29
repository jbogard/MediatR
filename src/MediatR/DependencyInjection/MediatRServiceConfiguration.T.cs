namespace MediatR.DependencyInjection;

public abstract class MediatRServiceConfiguration<TRegistrar> : MediatRServiceConfiguration
{
    /// <summary>
    /// Gets the Dependency Injection Adapter.
    /// </summary>
    public DependencyInjectionRegistrarAdapter<TRegistrar> DependencyInjectionRegistrarAdapter { get; }

    public MediatRServiceConfiguration(DependencyInjectionRegistrarAdapter<TRegistrar> dependencyInjectionRegistrarRegistrarAdapter) =>
        DependencyInjectionRegistrarAdapter = dependencyInjectionRegistrarRegistrarAdapter;
}