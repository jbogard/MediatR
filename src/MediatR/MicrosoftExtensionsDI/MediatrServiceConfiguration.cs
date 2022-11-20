using System;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public class MediatRServiceConfiguration
{
    public Func<Type, bool> TypeEvaluator { get; private set; } = t => true;
    public Type MediatorImplementationType { get; private set; }
    public ServiceLifetime Lifetime { get; private set; }
    public RequestExceptionActionProcessorStrategy RequestExceptionActionProcessorStrategy { get; set; }

    public MediatRServiceConfiguration()
    {
        MediatorImplementationType = typeof(Mediator);
        Lifetime = ServiceLifetime.Transient;
    }

    public MediatRServiceConfiguration Using<TMediator>() where TMediator : IMediator
    {
        MediatorImplementationType = typeof(TMediator);
        return this;
    }

    public MediatRServiceConfiguration AsSingleton()
    {
        Lifetime = ServiceLifetime.Singleton;
        return this;
    }

    public MediatRServiceConfiguration AsScoped()
    {
        Lifetime = ServiceLifetime.Scoped;
        return this;
    }

    public MediatRServiceConfiguration AsTransient()
    {
        Lifetime = ServiceLifetime.Transient;
        return this;
    }

    public MediatRServiceConfiguration WithEvaluator(Func<Type, bool> evaluator)
    {
        TypeEvaluator = evaluator;
        return this;
    }
}