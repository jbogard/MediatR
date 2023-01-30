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

    public MediatRServiceConfiguration RegisterHandlersFromAssemblyContaining<T>() 
        => RegisterHandlersFromAssemblyContaining(typeof(T));

    public MediatRServiceConfiguration RegisterHandlersFromAssemblyContaining(Type type) 
        => RegisterHandlersFromAssembly(type.Assembly);

    public MediatRServiceConfiguration RegisterHandlersFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);
        return this;
    }

    public MediatRServiceConfiguration RegisterHandlersFromAssemblies(
        params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);
        return this;
    }
}