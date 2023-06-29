using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.ExceptionHandler;
using MediatR.Abstraction.Handlers;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private void AddSingleArityTypeVariants(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var genericArgsCache = new Type[1];

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces,typeof(INotificationHandler<>)) > 0)
        {
            ResolveGenericType(_notifications);
        }
        else if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestHandler<>)) > 0)
        {
            ResolveGenericType(_requests);
        }
        else if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IPipelineBehavior<>)) > 0 &&
                 _configuration.BehaviorsToRegister.All(i => i.ImplementingType != typeWrapper.Type ||
                                                             i.ServiceType.Contains(typeof(IPipelineBehavior<>))))
        {
            ResolveGenericType(_requests);
        }

        void ResolveGenericType(TypeWrapper[] messages)
        {
            foreach (var message in messages)
            {
                genericArgsCache[0] = message.Type;
                var result = CreateGenericType(typeWrapper, genericArgsCache);
                if (result is not null)
                {
                    typeVariants.Add(result);
                }
            }
        }
    }

    private void AddRequestTypeVariants(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var genericTypeCache = new Type[2];

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestHandler<,>)) > 0)
        {
            ResolveGenericType(_requestResponses);
        }
        else if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IStreamRequestHandler<,>)) > 0)
        {
            ResolveGenericType(_streamRequests);
        }

        void ResolveGenericType((TypeWrapper, Type)[] messages)
        {
            foreach (var (request, response) in messages)
            {
                genericTypeCache[0] = request.Type;
                genericTypeCache[1] = response;
                var result = CreateGenericType(typeWrapper, genericTypeCache);
                if (result is not null)
                {
                    typeVariants.Add(result);
                }
            }
        }
    }

    private void AddHandlerExceptionHandler(List<Type> typeVariants, TypeWrapper typeWrapper, MediatRServiceConfiguration configuration)
    {
        var genericTypeCache = new Type[2];

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestExceptionHandler<,>)) > 0)
        {
            ResolveGenericType(_requests);
        }
        else if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestExceptionAction<,>)) > 0)
        {
            ResolveGenericType(_requests);
        }

        void ResolveGenericType(TypeWrapper[] requests)
        {
            foreach (var request in requests)
            {
                foreach (var exceptionType in configuration.ExceptionTypes)
                {
                    genericTypeCache[0] = request.Type;
                    genericTypeCache[1] = exceptionType;
                    var result = CreateGenericType(typeWrapper, genericTypeCache);
                    if (result is not null)
                    {
                        typeVariants.Add(result);
                    }
                }
            }
        }
    }

    private void AddRequestExceptionHandler(List<Type> typeVariants, TypeWrapper typeWrapper, MediatRServiceConfiguration configuration)
    {
        var genericTypeCache = new Type[3];

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestResponseExceptionHandler<,,>)) > 0)
        {
            ResolveGenericType(_requestResponses);
        }
        else if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IRequestResponseExceptionAction<,,>)) > 0)
        {
            ResolveGenericType(_requestResponses);
        }

        void ResolveGenericType((TypeWrapper, Type)[] requestsResponse)
        {
            foreach (var (request, response) in requestsResponse)
            {
                foreach (var exceptionType in configuration.ExceptionTypes)
                {
                    genericTypeCache[0] = request.Type;
                    genericTypeCache[1] = response;
                    genericTypeCache[2] = exceptionType;
                    var result = CreateGenericType(typeWrapper, genericTypeCache);
                    if (result is not null)
                    {
                        typeVariants.Add(result);
                    }
                }
            }
        }
    }

    private void AddRequestPipelineBehaviors(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var genericTypeCache = new Type[1];

        // Check if the type has any Request behaviors and if the type if already added to the behaviors and contains the IPipelineBehavior interface.
        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IPipelineBehavior<>)) <= 0 &&
            _configuration.BehaviorsToRegister.All(i => i.ImplementingType != typeWrapper.Type ||
                                                        !i.ServiceType.Contains(typeof(IPipelineBehavior<>))))
            return;

        foreach (var request in _requests)
        {
            genericTypeCache[0] = request.Type;
            var result = CreateGenericType(typeWrapper, genericTypeCache);
            if (result is not null)
            {
                typeVariants.Add(result);
            }
        }
    }

    private void AddRequestResponsePipelineBehaviors(List<Type> typeVariants, TypeWrapper typeWrapper)
    {
        var genericTypeCache = new Type[2];

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IPipelineBehavior<,>)) > 0 &&
            _configuration.BehaviorsToRegister.All(t => t.ImplementingType != typeWrapper.Type ||
                                                        t.ServiceType.Contains(typeof(IPipelineBehavior<,>))))
        {
            ResolveGenericType(_requestResponses);
        }

        if (Array.BinarySearch(typeWrapper.OpenGenericInterfaces, typeof(IStreamPipelineBehavior<,>)) > 0 &&
            _configuration.BehaviorsToRegister.All(t => t.ImplementingType != typeWrapper.Type ||
                                                        t.ServiceType.Contains(typeof(IStreamPipelineBehavior<,>))))
        {
            ResolveGenericType(_streamRequests);
        }
        
        void ResolveGenericType((TypeWrapper, Type)[] requestResponse)
        {
            foreach (var (request, response) in requestResponse)
            {
                genericTypeCache[0] = request.Type;
                genericTypeCache[1] = response;
                var result = CreateGenericType(typeWrapper, genericTypeCache);
                if (result is not null)
                {
                    typeVariants.Add(result);
                }
            }
        }
    }

    private static Type? CreateGenericType(TypeWrapper type, Type[] parameter)
    {
        try
        {
            return type.Type.MakeGenericType(parameter);
        }
        catch (ArgumentException)
        {
            // We should only catch the exception for not matching the generic constrains.
            return null;
        }
    }
}