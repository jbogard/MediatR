using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatR.DependencyInjection;

internal readonly ref partial struct AssemblyScanner<TRegistrar>
{
    private readonly TypeWrapper[] _typesToScan;
    private readonly TypeWrapper[] _notifications;
    private readonly TypeWrapper[] _requests;
    private readonly (TypeWrapper RequestType, Type ResponseType)[] _requestResponses;
    private readonly (TypeWrapper RequestType, Type ResponseType)[] _streamRequests;
    
    private readonly TypeComparerImplementation _typeComparer = new();

    private readonly Type[] _genericHandlerTypeCache = new Type[1];
    private readonly Type[] _genericRequestHandlerTypeCache = new Type[2];
    private readonly Type[] _genericRequestExceptionHandlerTypeCache = new Type[3];

    private readonly MediatRServiceConfiguration<TRegistrar> _configuration;

    public AssemblyScanner(MediatRServiceConfiguration<TRegistrar> configuration)
    {
        _configuration = configuration;
        var typeToScanCache = new Dictionary<Type, TypeWrapper>();
        var typeComparer = _typeComparer;

        _typesToScan = configuration.AssembliesToRegister
            .Distinct()
            .SelectMany(static a => a.DefinedTypes)
            .Where(t => )
            .Select(t => TypeWrapper.Create(t, configuration.AssembliesToRegister, typeToScanCache))
            .OfType<TypeWrapper>()
            .ToArray();


    }

    


}