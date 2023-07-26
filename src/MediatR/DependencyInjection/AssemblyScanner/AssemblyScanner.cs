using System;
using System.Collections.Generic;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Processors;
using MediatR.DependencyInjection.ConfigurationBase;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal readonly ref struct AssemblyScanner
{
    private readonly IComparer<Type> _typeComparer = Comparer<Type>.Create(static (x, y) => x.GUID.CompareTo(y.GUID));
    private readonly List<(Type Interface, bool MustBeSingleRegistration)> _serviceInterfaceArrayBuilder = new();

    private readonly TypeWrapper[] _typesToScan;
    private readonly MediatRServiceConfiguration _configuration;

    public AssemblyScanner(MediatRServiceConfiguration configuration)
    {
        _configuration = configuration;
        var typeToScanCache = new Dictionary<Type, TypeWrapper>();
        var typeWrappers = new List<TypeWrapper>();

        foreach (var assembly in configuration.AssembliesToRegister)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (!type.IsAbstract && !type.IsEnum && configuration.TypeEvaluator(type))
                {
                    var wrapper = TypeWrapper.Create(type, configuration.AssembliesToRegister, typeToScanCache);
                    typeWrappers.Add(wrapper);
                }
            }
        }

        _typesToScan = typeWrappers.ToArray();
    }

    public void ScanForMediatRServices<TRegistrar, TConfiguration>(DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter)
        where TConfiguration : MediatRServiceConfiguration
    {
        foreach (var processorPipeline in InternalServiceRegistrar.GetInternalProcessorPipelines(_typesToScan, _typeComparer, _configuration))
        {
            var wrapper = new TypeWrapper(processorPipeline);
            ScanTypeForRelevantInterfaces(wrapper, _serviceInterfaceArrayBuilder, _configuration);
            RegisterServiceTypes(wrapper, _serviceInterfaceArrayBuilder.ToArray(), adapter, _configuration);
            _serviceInterfaceArrayBuilder.Clear();
        }

        _configuration.RequestBehaviors.Register(adapter, _configuration);
        _configuration.RequestResponseBehaviors.Register(adapter, _configuration);
        _configuration.StreamRequestBehaviors.Register(adapter, _configuration);
        _configuration.RequestPreProcessors.Register(adapter, _configuration);
        _configuration.RequestPostProcessors.Register(adapter, _configuration);
        _configuration.RequestResponsePreProcessors.Register(adapter, _configuration);
        _configuration.RequestResponsePostProcessors.Register(adapter, _configuration);

        foreach (var typeWrapper in _typesToScan)
        {
            // Checking for inherited types that are inherited by anyone results in errors in the registration and should also not be expected by the user.
            // Types with no interfaces can not have anything for us to look for.
            // To improves performance skip these types.
            if (typeWrapper.TypesInheritingThisType.Count is not 0 || typeWrapper.Interfaces.Length is 0)
                continue;

            ScanTypeForRelevantInterfaces(typeWrapper, _serviceInterfaceArrayBuilder, _configuration);

            if (_serviceInterfaceArrayBuilder.Count > 0)
            {
                RegisterServiceTypes(typeWrapper, _serviceInterfaceArrayBuilder.ToArray(), adapter, _configuration);
                _serviceInterfaceArrayBuilder.Clear();
            }
        }

        foreach (var exceptionHandlingPipeline in InternalServiceRegistrar.GetInternalExceptionHandlingPipelines(_typesToScan, _typeComparer, _configuration))
        {
            var wrapper = new TypeWrapper(exceptionHandlingPipeline);
            ScanTypeForRelevantInterfaces(wrapper, _serviceInterfaceArrayBuilder, _configuration);
            RegisterServiceTypes(wrapper, _serviceInterfaceArrayBuilder.ToArray(), adapter, _configuration);
            _serviceInterfaceArrayBuilder.Clear();
        }

        InternalServiceRegistrar.RegisterInternalServiceTypes(adapter, _configuration);
    }

    private static void ScanTypeForRelevantInterfaces(in TypeWrapper typeWrapper, List<(Type, bool)> implementingInterface, MediatRServiceConfiguration configuration)
    {
        foreach (var interfaceImpl in typeWrapper.OpenGenericInterfaces)
        {
            ScanForHandlers(implementingInterface, interfaceImpl);
            ScanForExceptionHandler(implementingInterface, interfaceImpl);
            ScanForProcessors(implementingInterface, typeWrapper.Type, interfaceImpl, configuration);
            ScanForBehaviors(implementingInterface, typeWrapper.Type, interfaceImpl, configuration);
        }
    }

    private static void RegisterServiceTypes<TRegistrar, TConfiguration>(
        in TypeWrapper typeWrapper,
        (Type, bool)[] implementingInterface,
        DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter,
        MediatRServiceConfiguration configuration)
        where TConfiguration : MediatRServiceConfiguration
    {
        if (typeWrapper.IsOpenGeneric)
        {
            adapter.RegisterOpenGeneric(configuration, typeWrapper.Type, implementingInterface);
        }
        else
        {
            adapter.Register(configuration, typeWrapper.Type, implementingInterface);
        }
    }

    private static void ScanForHandlers(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        (Type Interface, Type OpenGenericInterface) interfaceImpl)
    {
        if (interfaceImpl.OpenGenericInterface == typeof(INotificationHandler<>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestHandler<>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (interfaceImpl.OpenGenericInterface == typeof(IStreamRequestHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));
    }

    private static void ScanForExceptionHandler(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        (Type Interface, Type OpenGenericInterface) interfaceImpl)
    {
        if (interfaceImpl.OpenGenericInterface == typeof(IRequestExceptionAction<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestExceptionHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionAction<,,>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionHandler<,,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));
    }

    private static void ScanForProcessors(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        Type implementationType,
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        MediatRServiceConfiguration configuration)
    {
        if (interfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<>) &&
            !configuration.RequestPreProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<>) &&
            !configuration.RequestPostProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<,>) &&
            !configuration.RequestResponsePreProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (interfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<,>) &&
            !configuration.RequestResponsePostProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }
    }

    private static void ScanForBehaviors(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        Type implementationType,
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        MediatRServiceConfiguration configuration)
    {
        if (interfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<>) &&
            !configuration.RequestBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (interfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<,>) &&
            !configuration.RequestResponseBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (interfaceImpl.OpenGenericInterface == typeof(IStreamPipelineBehavior<,>) &&
            !configuration.StreamRequestBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }
    }
}