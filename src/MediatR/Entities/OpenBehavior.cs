using System;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Entities;
/// <summary>
/// Creates open behavior entity.
/// </summary>
public class OpenBehavior
{
    public OpenBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        OpenBehaviorType = openBehaviorType;
        ServiceLifetime = serviceLifetime;
    }

    public Type? OpenBehaviorType { get; } 
    public ServiceLifetime ServiceLifetime { get; }

    
}