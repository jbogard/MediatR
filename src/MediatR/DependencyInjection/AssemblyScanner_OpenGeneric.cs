using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;
using MediatR.Abstraction.Pipeline;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private void AddGenericTypeVariants(TypeWrapper typeWrapper, List<Type> typeVariants)
    {
        foreach (var openGenericInterface in typeWrapper.OpenGenericInterfaces.OrderByDescending(static tuple => tuple.OpenGenericInterface.GenericTypeArguments.Length))
        {
            AddNotificationVariants(typeWrapper, openGenericInterface, typeVariants);
        }
    }

    private void AddNotificationVariants(TypeWrapper typeWrapper, (Type Interface, Type OpenGenericInterface) openGenericInterface, List<Type> typeVariants)
    {
        if (openGenericInterface.OpenGenericInterface != typeof(INotificationHandler<>) &&
            typeWrapper.GenericTypeArguments?.Length > 1)
        {
            return;
        }

        foreach (var notification in _notifications)
        {
            _genericHandlerTypeCache[0] = notification.Type;
            var concreteType = CreateGenericType(typeWrapper, _genericRequestHandlerTypeCache);
            if (concreteType is not null)
            {
                typeVariants.Add(concreteType);
            }
        }
    }

    private void AddRequestTypeVariants((Type Interface, Type OpenGenericInterface) openGenericInterface, TypeWrapper typeWrapper, List<Type> typeVariants)
    {
        if (typeWrapper.GenericTypeArguments!.Length > 2)
        {
            return;
        }
    }

    private void AddRequestTypeVariants(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var genericHandlerTypeCache = _genericHandlerTypeCache;
        var genericRequestHandlerTypeCache = _genericRequestHandlerTypeCache;
        var typeComparer = _typeComparer;

        foreach (var openGenericInterfaceImpl in typeWrapper.OpenGenericInterfaces)
        {
            if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestHandler<>))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<>) &&
                     !_configuration.BehaviorsToRegister.Any(i => i.Key == typeWrapper.Type &&
                                                                  Array.BinarySearch(i.Value, typeof(IPipelineBehavior<>), typeComparer) >= 0))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<>))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<>))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestExceptionHandler<>))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestExceptionAction<>))
            {
                ResolveGenericType(_requests);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestExceptionHandler<,>))
            {
                ResolveGenericExceptionHandlerType(this, _configuration, _requests, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestExceptionAction<,>))
            {
                ResolveGenericExceptionHandlerType(this, _configuration, _requests, openGenericInterfaceImpl);
            }
        }

        void ResolveGenericType(TypeWrapper[] messages)
        {
            foreach (var message in messages)
            {
                genericHandlerTypeCache[0] = message.Type;
                var result = CreateGenericType(typeWrapper, genericHandlerTypeCache);
                if (result is not null)
                {
                    typeVariants.Add(result);
                }
            }
        }

        void ResolveGenericExceptionHandlerType(AssemblyScanner<TRegistrar> scanner, MediatRServiceConfiguration<TRegistrar> configuration, TypeWrapper[] requests, (Type, Type) interfaceImpl)
        {
            foreach (var request in requests)
            {
                foreach (var exceptionType in configuration.ExceptionTypes)
                {
                    genericRequestHandlerTypeCache[0] = request.Type;
                    genericRequestHandlerTypeCache[1] = exceptionType;
                    var result = scanner.CreateGenericType(typeWrapper, genericRequestHandlerTypeCache, interfaceImpl);
                    if (result is not null)
                    {
                        typeVariants.Add(result);
                    }
                }
            }
        }
    }

    private void AddRequestResponseTypeVariants(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var typeComparer = _typeComparer;

        foreach (var openGenericInterfaceImpl in typeWrapper.OpenGenericInterfaces)
        {
            if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionHandler<,,>))
            {
                ResolveExceptionRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionAction<,,>))
            {
                ResolveExceptionRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestHandler<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IPipelineBehavior<,>) &&
                     !_configuration.BehaviorsToRegister.Any(i => i.Key == typeWrapper.Type && 
                                                                  Array.BinarySearch(i.Value, typeof(IPipelineBehavior<,>), typeComparer) >= 0))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestPreProcessor<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestPostProcessor<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionAction<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionHandler<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IRequestResponseExceptionAction<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
        }
    }

    private void AddStreamRequestTypeVariants(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var typeComparer = _typeComparer;
        
        foreach (var openGenericInterfaceImpl in typeWrapper.OpenGenericInterfaces)
        {
            if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IStreamRequestHandler<,>))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _streamRequests, openGenericInterfaceImpl);
            }
            else if (openGenericInterfaceImpl.OpenGenericInterface == typeof(IStreamPipelineBehavior<,>) && 
                     !_configuration.BehaviorsToRegister.Any(i => i.Key == typeWrapper.Type && 
                                                                  Array.BinarySearch(i.Value, typeof(IPipelineBehavior<,>), typeComparer) >= 0))
            {
                ResolveRequestResponseType(typeWrapper, typeVariants, _requestResponses, openGenericInterfaceImpl);
            }
        }
    }

    private void ResolveRequestResponseType(
        TypeWrapper typeWrapper,
        List<Type> typeVariants,
        (TypeWrapper, Type)[] messages,
        (Type Interface, Type OpenGenericInterface) interfaceImpl)
    {
        foreach (var (request, response) in messages)
        {
            _genericRequestHandlerTypeCache[0] = request.Type;
            _genericRequestHandlerTypeCache[1] = response;
            var result = CreateGenericType(typeWrapper, _genericRequestHandlerTypeCache, interfaceImpl);
            if (result is not null)
            {
                typeVariants.Add(result);
            }
        }
    }

    private void ResolveExceptionRequestType(
        TypeWrapper typeWrapper,
        List<Type> typeVariants,
        TypeWrapper[] messages,
        (Type Interface, Type OpenGenericInterface) interfaceImpl)
    {
        if (interfaceImpl.Interface.GetGenericArguments()[1].GUID == Guid.Empty)
        {
            
        }
    }

    private void ResolveExceptionRequestResponseType(
        TypeWrapper typeWrapper,
        List<Type> typeVariants,
        (TypeWrapper, Type)[] messages,
        (Type Interface, Type OpenGenericInterface) interfaceImpl)
    {
        if (interfaceImpl.Interface.GetGenericArguments()[2].GUID == Guid.Empty)
        {
            foreach (var (request, response) in messages)
            {
                foreach (var exceptionType in _configuration.ExceptionTypes)
                {
                    _genericRequestExceptionHandlerTypeCache[0] = request.Type;
                    _genericRequestExceptionHandlerTypeCache[1] = response;
                    _genericRequestExceptionHandlerTypeCache[2] = exceptionType;
                    CreateType(this, _genericRequestExceptionHandlerTypeCache);
                }
            }
        }
        else
        {
            foreach (var (request, response) in messages)
            {
                _genericRequestExceptionHandlerTypeCache[0] = request.Type;
                _genericRequestExceptionHandlerTypeCache[1] = response;
                CreateType(this, _genericRequestExceptionHandlerTypeCache);
            }
        }

        void CreateType(AssemblyScanner<TRegistrar> scanner, Type[] typeArgsCache)
        {
            var result = scanner.CreateGenericType(typeWrapper, typeArgsCache, interfaceImpl);
            if (result is not null)
            {
                typeVariants.Add(result);
            }
        }
    }

    private Type? CreateGenericType(TypeWrapper typeWrapper, Type[] parameters, (Type Interface, Type OpenGenericInterface) openGenericInterfaceImpl)
    {
        const int MaxSupportedOpenGenerics = 3;
        var typeWrapperGenericArguments = typeWrapper.Type.GetGenericArguments();
        if (typeWrapperGenericArguments.Length == parameters.Length)
        {
            return CreateGenericType(typeWrapper, parameters);
        }

        if (typeWrapperGenericArguments.Length > MaxSupportedOpenGenerics)
        {
            if (_configuration.ThrowOnNotSupportedOpenGenerics)
            {
                ThrowInvalidOpenGenericException(typeWrapper.Type);
            }
            else
            {
                return null;
            }
        }

        var interfaceGenericParameter = openGenericInterfaceImpl.Interface.GetGenericArguments();
        var newTypeGenericParameter = new Type[typeWrapperGenericArguments.Length];
        var newTypeGenericParameterCounter = 0;

        Debug.Assert(interfaceGenericParameter.Length == parameters.Length, $"The interface implementation open generic parameter must match the provided {nameof(parameters)}");

        for (var i = 0; i < parameters.Length; i++)
        {
            var interfaceImplArg = interfaceGenericParameter[i];
            var parameterArg = parameters[i];

            // An Empty guid means that the interfaceImplArg is a generic placeholder type for any type.
            if (interfaceImplArg.GUID == Guid.Empty)
            {
                newTypeGenericParameter[newTypeGenericParameterCounter] = parameterArg;
                newTypeGenericParameterCounter++;
            }
        }

        Debug.Assert(newTypeGenericParameterCounter > typeWrapper.Type.GenericTypeArguments.Length, "Can not have more generic type parameter than the original type.");

        return CreateGenericType(typeWrapper, newTypeGenericParameter);
    }

    private static void ThrowInvalidOpenGenericException(Type type) =>
        throw new InvalidOperationException($"The type '{type}' has more then the max allowed open generic parameters.");
}