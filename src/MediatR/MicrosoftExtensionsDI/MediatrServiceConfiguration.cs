using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public class MediatRServiceConfiguration
{
    public Func<Type, bool> TypeEvaluator { get; set; } = t => true;
    public Type MediatorImplementationType { get; set; } = typeof(Mediator);
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
    public RequestExceptionActionProcessorStrategy RequestExceptionActionProcessorStrategy { get; set; }
        = RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions;

    internal List<Assembly> AssembliesToRegister { get; } = new();

    public List<ServiceDescriptor> BehaviorsToRegister { get; } = new();

    public MediatRServiceConfiguration RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    public MediatRServiceConfiguration RegisterServicesFromAssemblyContaining(Type type)
        => RegisterServicesFromAssembly(type.Assembly);

    public MediatRServiceConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);

        return this;
    }

    public MediatRServiceConfiguration RegisterServicesFromAssemblies(
        params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);

        return this;
    }

    public MediatRServiceConfiguration AddBehavior<TServiceType, TImplementationType>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient) =>
        AddBehavior(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    public MediatRServiceConfiguration AddBehavior(
        Type serviceType,
        Type implementationType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        BehaviorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));

        return this;
    }

    public MediatRServiceConfiguration AddOpenBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var serviceType = typeof(IPipelineBehavior<,>);

        BehaviorsToRegister.Add(new ServiceDescriptor(serviceType, openBehaviorType, serviceLifetime));

        return this;
    }
}