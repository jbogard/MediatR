using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Pipeline;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private void AddExceptionHandingInterfaces(List<(Type, bool)> implementingInterfaces, TypeWrapper typeWrapper)
    {
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestExceptionAction<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestExceptionHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestResponseExceptionAction<,,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestResponseExceptionHandler<,,>), implementingInterfaces, true);
    }

    private void AddProcessorInterfaces(List<(Type, bool)> implementingInterfaces, TypeWrapper typeWrapper)
    {
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestPostProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestPostProcessor<,>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestPostProcessor<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestPostProcessor<,>), implementingInterfaces, false);
    }

    private void AddHandlerInterfaces(List<(Type, bool)> implementingInterfaces, TypeWrapper typeWrapper)
    {
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(INotificationHandler<>), implementingInterfaces, false);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestHandler<>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IRequestHandler<,>), implementingInterfaces, true);
        AddNoneGenericInterfaceImplementations(typeWrapper, typeof(IStreamRequestHandler<,>), implementingInterfaces, true);
    }
    
    private static void AddNoneGenericInterfaceImplementations(
        TypeWrapper typeWrapper,
        Type openGenericInterface,
        List<(Type,bool)> implementingInterfaces,
        bool mustBeSingleRegistration)
        => implementingInterfaces.AddRange(
            typeWrapper.Interfaces
                .Where(t =>
                    !t.ContainsGenericParameters &&
                    t.IsGenericType &&
                    t.GetGenericTypeDefinition() == openGenericInterface)
                .Select(t => (t, mustBeSingleRegistration)));
}