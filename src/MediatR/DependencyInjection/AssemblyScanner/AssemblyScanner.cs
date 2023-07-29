using System;
using System.Collections.Generic;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Processors;
using MediatR.DependencyInjection.Configuration;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal readonly ref struct AssemblyScanner
{
    private readonly List<(Type Interface, bool MustBeSingleRegistration)> _serviceInterfaceArrayBuilder = new();

    private readonly (TypeWrapper Wrapper, AssemblyScannerOptions Option)[] _typesToScan;
    private readonly MediatRServiceConfiguration _configuration;

    public AssemblyScanner(MediatRServiceConfiguration configuration)
    {
        _configuration = configuration;
        var typeToScanCache = new Dictionary<Type, TypeWrapper>();
        var typeWrappers = new List<(TypeWrapper, AssemblyScannerOptions)>();

        foreach (var assemblyConfiguration in configuration.AssembliesToRegister)
        {
            foreach (var type in assemblyConfiguration.Assembly.DefinedTypes)
            {
                if (!type.IsAbstract && !type.IsEnum && configuration.TypeEvaluator(type))
                {
                    var wrapper = TypeWrapper.Create(type, configuration.AssembliesToRegister, typeToScanCache);
                    typeWrappers.Add((wrapper, assemblyConfiguration.ScannerOptions));
                }
            }
        }

        _typesToScan = typeWrappers.ToArray();
    }

    public void ScanForMediatRServices<TRegistrar, TConfiguration>(DependencyInjectionRegistrarAdapter<TRegistrar, TConfiguration> adapter)
        where TConfiguration : MediatRServiceConfiguration
    {
        foreach (var processorPipeline in InternalServiceRegistrar.GetInternalProcessorPipelines(_typesToScan, _configuration))
        {
            var wrapper = new TypeWrapper(processorPipeline);
            ScanTypeForRelevantInterfaces((wrapper, AssemblyScannerOptions.All), _serviceInterfaceArrayBuilder, _configuration);
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

        foreach (var (typeWrapper, options) in _typesToScan)
        {
            // Checking for inherited types that are inherited by any other type, results in errors in the registration and should also not be expected by the user.
            // Types with no interfaces can not have anything for us to look for.
            // To improves performance skip these types.
            if (typeWrapper.TypesInheritingThisType.Count is not 0 || typeWrapper.Interfaces.Length is 0)
                continue;

            ScanTypeForRelevantInterfaces((typeWrapper, options), _serviceInterfaceArrayBuilder, _configuration);

            if (_serviceInterfaceArrayBuilder.Count > 0)
            {
                RegisterServiceTypes(typeWrapper, _serviceInterfaceArrayBuilder.ToArray(), adapter, _configuration);
                _serviceInterfaceArrayBuilder.Clear();
            }
        }

        foreach (var exceptionHandlingPipeline in InternalServiceRegistrar.GetInternalExceptionHandlingPipelines(_typesToScan, _configuration))
        {
            var wrapper = new TypeWrapper(exceptionHandlingPipeline);
            ScanTypeForRelevantInterfaces((wrapper, AssemblyScannerOptions.All), _serviceInterfaceArrayBuilder, _configuration);
            RegisterServiceTypes(wrapper, _serviceInterfaceArrayBuilder.ToArray(), adapter, _configuration);
            _serviceInterfaceArrayBuilder.Clear();
        }

        InternalServiceRegistrar.RegisterInternalServiceTypes(adapter, _configuration);
    }

    private static void ScanTypeForRelevantInterfaces(in (TypeWrapper TypeWrapper, AssemblyScannerOptions Option) wrapper, List<(Type, bool)> implementingInterface, MediatRServiceConfiguration configuration)
    {
        var typeWrapper = wrapper.TypeWrapper;
        var scannerOption = wrapper.Option;
        foreach (var interfaceImpl in typeWrapper.OpenGenericInterfaces)
        {
            ScanForHandlers(implementingInterface, interfaceImpl, scannerOption);
            ScanForExceptionHandler(implementingInterface, interfaceImpl, scannerOption);
            ScanForProcessors(implementingInterface, typeWrapper.Type, interfaceImpl, scannerOption, configuration);
            ScanForBehaviors(implementingInterface, typeWrapper.Type, interfaceImpl, scannerOption, configuration);
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
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        AssemblyScannerOptions option)
    {
        if (option.HasFlag(AssemblyScannerOptions.NotificationHandler) && interfaceImpl.OpenGenericInterface == typeof(INotificationHandler<>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (option.HasFlag(AssemblyScannerOptions.RequestHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestHandler<>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (option.HasFlag(AssemblyScannerOptions.RequestResponseHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (option.HasFlag(AssemblyScannerOptions.StreamRequestHandler) && interfaceImpl.OpenGenericInterface == typeof(IStreamRequestHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));
    }

    private static void ScanForExceptionHandler(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        AssemblyScannerOptions option)
    {
        if (option.HasFlag(AssemblyScannerOptions.RequestExceptionActionHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestExceptionAction<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (option.HasFlag(AssemblyScannerOptions.RequestExceptionHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestExceptionHandler<,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));

        if (option.HasFlag(AssemblyScannerOptions.RequestResponseExceptionActionHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionAction<,,>))
            implementingInterfaces.Add((interfaceImpl.Interface, false));

        if (option.HasFlag(AssemblyScannerOptions.RequestResponseExceptionHandler) && interfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionHandler<,,>))
            implementingInterfaces.Add((interfaceImpl.Interface, true));
    }

    private static void ScanForProcessors(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        Type implementationType,
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        AssemblyScannerOptions option,
        MediatRServiceConfiguration configuration)
    {
        if (option.HasFlag(AssemblyScannerOptions.RequestPreProcessor) &&
            interfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<>) &&
            !configuration.RequestPreProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (option.HasFlag(AssemblyScannerOptions.RequestPostProcessor) &&
            interfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<>) &&
            !configuration.RequestPostProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (option.HasFlag(AssemblyScannerOptions.RequestResponsePreProcessor) &&
            interfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<,>) &&
            !configuration.RequestResponsePreProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (option.HasFlag(AssemblyScannerOptions.RequestResponsePostProcessor) &&
            interfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<,>) &&
            !configuration.RequestResponsePostProcessors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }
    }

    private static void ScanForBehaviors(
        List<(Type Interface, bool MustBeSingleRegistration)> implementingInterfaces,
        Type implementationType,
        (Type Interface, Type OpenGenericInterface) interfaceImpl,
        AssemblyScannerOptions option,
        MediatRServiceConfiguration configuration)
    {
        if (option.HasFlag(AssemblyScannerOptions.RequestPipelineBehavior) &&
            interfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<>) &&
            !configuration.RequestBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (option.HasFlag(AssemblyScannerOptions.RequestResponsePipelineBehavior) &&
            interfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<,>) &&
            !configuration.RequestResponseBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }

        if (option.HasFlag(AssemblyScannerOptions.StreamRequestPipelineBehavior) &&
            interfaceImpl.OpenGenericInterface == typeof(IStreamPipelineBehavior<,>) &&
            !configuration.StreamRequestBehaviors.ContainsRegistration(interfaceImpl.Interface, implementationType))
        {
            implementingInterfaces.Add((interfaceImpl.Interface, false));
        }
    }
}