using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MediatR.Abstraction.Behaviors;

namespace MediatR.DependencyInjection;

internal readonly ref partial struct AssemblyScanner<TRegistrar>
{
    private static readonly Type ObjectType = typeof(object);
    
    private readonly TypeWrapper[] _typesToScan;
    private readonly TypeWrapper[] _notifications;
    private readonly TypeWrapper[] _requests;
    private readonly (TypeWrapper RequestType, Type ResponseType)[] _requestResponses;
    private readonly (TypeWrapper RequestType, Type ResponseType)[] _streamRequests;

    private readonly MediatRServiceConfiguration<TRegistrar> _configuration;

    public AssemblyScanner(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        _configuration = configuration;
        var typeToScanCache = new Dictionary<Type, TypeWrapper>();

        _typesToScan = configuration.AssembliesToRegister
            .Distinct()
            .SelectMany(static a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface && configuration.TypeEvaluator(t))
            .Select(t => TypeWrapper.Create(t, configuration.AssembliesToRegister, typeToScanCache))
            .OfType<TypeWrapper>()
            .ToArray();
        
        _notifications = Array.FindAll(_typesToScan,static wrapper => Array.BinarySearch(wrapper.Interfaces, typeof(INotification)) >= 0);
        _requests = Array.FindAll(_typesToScan,static wrapper => Array.BinarySearch(wrapper.Interfaces, typeof(IRequest)) >= 0);

        _requestResponses = GetRequestAndResponseFromType(
            _typesToScan, 
            static wrapper => Array.BinarySearch(wrapper.OpenGenericInterfaces, typeof(IRequest<>)) >= 0,
            static interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IRequest<>));

        _streamRequests = GetRequestAndResponseFromType(
            _typesToScan,
            static wrapper => Array.BinarySearch(wrapper.OpenGenericInterfaces, typeof(IStreamRequest<>)) >= 0,
            interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IStreamRequest<>));
    }

    public void ScanAssembliesAndRegisterServices()
    {
        var implementingInterfaces = new List<(Type, bool)>();
        var typeVariants = new List<Type>();

        // Make sure that the processor pipelines are registered before anything else is registered.
        InternalServiceRegistrar.RegisterInternalProcessorPipelines(_configuration);

        foreach (var (implementingType, serviceTypes) in _configuration.BehaviorsToRegister)
        {
            _configuration.DependencyInjectionRegistrarAdapter
                .Register(_configuration, implementingType, serviceTypes, false);
        }

        foreach (var typeWrapper in _typesToScan)
        {
            if (typeWrapper.TypesInheritingThisType.Count > 0 || typeWrapper.Interfaces.Length is 0)
                continue;

            if (typeWrapper.IsOpenGeneric)
            {
                switch (typeWrapper.Type.GetGenericArguments().Length)
                {
                    case 1:
                        AddSingleArityTypeVariants(typeVariants, typeWrapper);
                        AddRequestPipelineBehaviors(typeVariants, typeWrapper);
                        break;
                    case 2:
                        AddHandlerExceptionHandler(typeVariants, typeWrapper, _configuration);
                        AddRequestTypeVariants(typeVariants, typeWrapper);
                        AddRequestResponsePipelineBehaviors(typeVariants, typeWrapper);
                        break;
                    case 3:
                        AddRequestExceptionHandler(typeVariants, typeWrapper, _configuration);
                        break;
                }
            }
            else
            {
                typeVariants.Add(typeWrapper.Type);
            }

            foreach (var typeVariant in typeVariants)
            {
                AddHandlerInterfaces(implementingInterfaces, typeWrapper);
                AddProcessorInterfaces(implementingInterfaces, typeWrapper);
                AddExceptionHandingInterfaces(implementingInterfaces, typeWrapper);

                RegisterType(typeVariant, implementingInterfaces.ToArray());

                implementingInterfaces.Clear();
            }

            typeVariants.Clear();
        }

        // Make sure that The 
        InternalServiceRegistrar.RegisterInternalExceptionHandlingPipelines(_configuration);
    }
    
    private void RegisterType(Type implementingType, (Type ImplementingInterface, bool MustBeSingleRegistration)[] implementingInterfaces)
    {
        if (implementingInterfaces.Length < 1)
        {
            return;
        }
        
        var adapter = _configuration.DependencyInjectionRegistrarAdapter;

        foreach (var grouping in implementingInterfaces.GroupBy(x => x.MustBeSingleRegistration))
        {
            adapter.Register(
                _configuration,
                implementingType,
                grouping.Select(i => i.ImplementingInterface),
                grouping.Key);
        }
    }

    private void RegisterPipelines(List<TypeWrapper> pipelines)
    {
        foreach (var pipeline in pipelines)
        {
            Debug.Assert(!pipeline.IsOpenGeneric, "Pipeline must not be generic when registered.");
        }
    }
    
    private static (TypeWrapper RequestType, Type ResponseType)[] GetRequestAndResponseFromType(TypeWrapper[] types, Predicate<TypeWrapper> searchForType, Predicate<Type> searchForInterface)
    {
        var allTypes = Array.FindAll(types, searchForType);
        var resultArray = new (TypeWrapper, Type)[allTypes.Length];
        for (var i = 0; i < allTypes.Length; i++)
        {
            var type = allTypes[i];
            var indexOfSearchedArray = Array.FindIndex(type.Interfaces, searchForInterface);
            if (indexOfSearchedArray < 0)
            {
                throw new InvalidOperationException($"Could not find the request interface for type '{allTypes[i]}'");
            }

            // Request types must always have an request interface with at least one type generic which is the respose.
            resultArray[i] = (type, type.Interfaces[indexOfSearchedArray].GetGenericArguments()[0]);
        }

        return resultArray;
    }
}