using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal static class AssemblyScanner<TRegistrar>
{
    public static void RegisterMediatRServices(AssemblyScannerContext<TRegistrar> context)
    {
        var implementingInterfaces = new List<(Type, bool)>();
        var typeVariants = new List<Type>();

        foreach (var internalProcessorPipeline in InternalServiceRegistrar.GetInternalProcessorPipelines())
        {
            ScanTypeAndRegisterImplementations(new TypeWrapper(internalProcessorPipeline), typeVariants, implementingInterfaces, context);
        }

        foreach (var kvp in context.Configuration.BehaviorsToRegister)
        {
            context.Configuration.DependencyInjectionRegistrarAdapter
                .Register(context.Configuration, kvp.Key, kvp.Value, false);
        }

        foreach (var typeWrapper in context.TypesToScan)
        {
            if (typeWrapper.TypesInheritingThisType.Count > 0 || typeWrapper.Interfaces.Length is 0)
                continue;

            ScanTypeAndRegisterImplementations(typeWrapper, typeVariants, implementingInterfaces, context);
        }

        foreach (var exceptionHandlingPipeline in InternalServiceRegistrar.GetInternalExceptionHandlingPipelines(context.Configuration))
        {
            ScanTypeAndRegisterImplementations(new TypeWrapper(exceptionHandlingPipeline), typeVariants, implementingInterfaces, context);
        }
    }

    private static void ScanTypeAndRegisterImplementations(TypeWrapper typeWrapper, List<Type> typeVariants, List<(Type, bool)> implementingInterfaces, AssemblyScannerContext<TRegistrar> context)
    {
        if (typeWrapper.IsOpenGeneric)
        {
            AddRequestTypeVariants(typeVariants, typeWrapper);
            AddRequestResponseTypeVariants(typeVariants, typeWrapper);
            AddStreamRequestTypeVariants(typeVariants, typeWrapper);
        }
        else
        {
            typeVariants.Add(typeWrapper.Type);
        }

        foreach (var typeVariant in typeVariants)
        {
            AddHandlerInterfaces(implementingInterfaces, typeVariant);
            AddProcessorInterfaces(implementingInterfaces, typeVariant);
            AddExceptionHandingInterfaces(implementingInterfaces, typeVariant);
            AddPipelineInterfaces(implementingInterfaces, typeVariant);

            RegisterType(typeVariant, implementingInterfaces.ToArray(), context);

            implementingInterfaces.Clear();
        }

        typeVariants.Clear();
    }

    private static void RegisterType(Type implementingType, (Type ImplementingInterface, bool MustBeSingleRegistration)[] implementingInterfaces, AssemblyScannerContext<TRegistrar> context)
    {
        if (implementingInterfaces.Length < 1)
        {
            return;
        }
        
        var adapter = context.Configuration.DependencyInjectionRegistrarAdapter;

        foreach (var grouping in implementingInterfaces.GroupBy(x => x.MustBeSingleRegistration))
        {
            adapter.Register(
                context.Configuration,
                implementingType,
                grouping.Select(i => i.ImplementingInterface),
                grouping.Key);
        }
    }
}