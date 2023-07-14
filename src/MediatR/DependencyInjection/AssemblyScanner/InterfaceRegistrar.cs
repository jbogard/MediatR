using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Pipeline;

namespace MediatR.DependencyInjection.AssemblyScanner;

// Improve more. Single loop!!!!!!!!!!!!!!!!!!!!!
internal static class InterfaceRegistrar<TRegistrar>
{
    public static void AddInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant, AssemblyScannerContext<TRegistrar> context)
    {
        AddHandlerInterfaces(implementingInterfaces, typeVariant);
        AddProcessorInterfaces(implementingInterfaces, typeVariant);
        AddExceptionHandingInterfaces(implementingInterfaces, typeVariant);
        AddPipelineInterfaces(implementingInterfaces, typeVariant, context);
    }
    
    private static void AddExceptionHandingInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestExceptionAction<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestExceptionHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestResponseExceptionAction<,,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestResponseExceptionHandler<,,>), implementingInterfaces, true);
    }

    private static void AddProcessorInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPreProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPreProcessor<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPostProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestPostProcessor<,>), implementingInterfaces, false);
    }

    private static void AddHandlerInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant)
    {
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(INotificationHandler<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestHandler<>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IRequestHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeVariant, typeof(IStreamRequestHandler<,>), implementingInterfaces, true);
    }

    private static void AddPipelineInterfaces(List<(Type, bool)> implementingInterfaces, Type typeVariant, AssemblyScannerContext<TRegistrar> context)
    {
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IPipelineBehavior<>), implementingInterfaces, false, context);
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IPipelineBehavior<,>), implementingInterfaces, false, context);
        AddNoneGenericPipelineInterfaceImplementations(typeVariant, typeof(IStreamPipelineBehavior<,>), implementingInterfaces, false, context);
    }

    private static void AddNoneGenericInterfaceImplementations(
        Type type,
        Type openGenericInterface,
        List<(Type, bool)> implementingInterfaces,
        bool mustBeSingleRegistration)
        => implementingInterfaces.AddRange(
            type.GetInterfaces()
                .Where(t =>
                    !t.ContainsGenericParameters &&
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == openGenericInterface)
                .Select(t => (t, mustBeSingleRegistration)));

    private static void AddNoneGenericPipelineInterfaceImplementations(
        Type typeVariant,
        Type openGenericPipelines,
        List<(Type, bool)> implementingInterfaces,
        bool mustBeSingleRegistration,
        AssemblyScannerContext<TRegistrar> context)
    {
        var config = context.Configuration;
        var typeComparer = context.TypeComparerInstance;

        var noneGenericInter = Array.FindAll(typeVariant.GetInterfaces(), t =>
            !t.ContainsGenericParameters &&
            t.IsGenericType &&
            t.GetGenericTypeDefinition() == openGenericPipelines &&
            !IsPreRegisteredBehavior(typeVariant, t));
        implementingInterfaces.AddRange(Array.ConvertAll(noneGenericInter, t => (t, mustBeSingleRegistration)));

        bool IsPreRegisteredBehavior(Type type, Type interfaceType)
        {
            if (config.BehaviorsToRegister.TryGetValue(type, out var interfaces))
            {
                return Array.BinarySearch(interfaces, interfaceType, typeComparer) >= 0;
            }

            return false;
        }
    }
}