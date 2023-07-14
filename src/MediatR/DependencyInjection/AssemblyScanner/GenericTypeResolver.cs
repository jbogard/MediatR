using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal static class GenericTypeResolver<TRegistrar>
{
    public static void ResolveGenericType(TypeWrapper typeWrapper, AssemblyScannerContext<TRegistrar> context)
    {
        Debug.Assert(typeWrapper.IsOpenGeneric, "Must be only invoked with generic type.");
        var genericTypeMap = typeWrapper.GenericReplaceableTypeArguments.ToDictionary(replaceableTypeArgument => replaceableTypeArgument, _ => (TypeWrapper[]?)null);
        foreach (var genericTypeKvp in genericTypeMap)
        {
            ResolveGenericTypeParameter(genericTypeMap, genericTypeKvp, context);
        }
    }

    private static void ResolveGenericTypeParameter(Dictionary<Type,TypeWrapper[]?> genericTypeMap, KeyValuePair<Type,TypeWrapper[]?> genericTypeKvp, AssemblyScannerContext<TRegistrar> context)
    {
        var messageTypes = GetPossibleGenericTypeArguments(genericTypeKvp, context);
        if (messageTypes?.Count is 0)
        {
            return;
        }
    }

    private static HashSet<TypeWrapper>? GetPossibleGenericTypeArguments(KeyValuePair<Type, TypeWrapper[]?> genericTypeKvp, AssemblyScannerContext<TRegistrar> context)
    {
        HashSet<TypeWrapper>? messageTypes = null;
        foreach (var parameterConstraint in genericTypeKvp.Key.GetGenericParameterConstraints())
        {
            if (parameterConstraint.IsInterface)
            {
                if (context.MessageTypes.Contains(parameterConstraint))
                {
                    AddTypeToHashSet(ref messageTypes, context, parameterConstraint);
                }
                else if (HasRelevantMessageInterface(parameterConstraint, context, out var interfaces))
                {
                    AddTypeToHashSet(ref messageTypes, context, interfaces);
                }
            }
            else if (parameterConstraint.IsClass)
            {
                if (HasRelevantMessageInterface(parameterConstraint, context, out var interfaces))
                {
                    AddTypeToHashSet(ref messageTypes, context, interfaces);
                }
                else if (IsExceptionType(parameterConstraint))
                {
                    AddTypeToHashSet(ref messageTypes, context, typeof(Exception));
                }
            }
        }

        return messageTypes;
    }

    private static bool HasRelevantMessageInterface(Type constrain, AssemblyScannerContext<TRegistrar> context, out Type[] interfaces)
    {
        var implInterfaces = constrain.GetInterfaces();
        foreach (var messageType in context.MessageTypes)
        {
            var index = Array.BinarySearch(implInterfaces, messageType);
            if (index > 0)
            {
                ArrayBuilder<Type>.Instance.Add(implInterfaces[index]);
            }
        }
        
        interfaces = ArrayBuilder<Type>.Instance.ToArray();
        return interfaces.Length > 0;
    }

    private static bool IsExceptionType(Type? constrain)
    {
        while (constrain is not null && constrain.BaseType != typeof(object))
        {
            constrain = constrain.BaseType;
        }

        return constrain == typeof(Exception);
    }

    private static void AddTypeToHashSet(ref HashSet<TypeWrapper>? accumulated, AssemblyScannerContext<TRegistrar> context, Type[] messageTypes)
    {
        foreach (var messageType in messageTypes)
        {
            AddTypeToHashSet(ref accumulated, context, messageType);
        }
    }

    private static void AddTypeToHashSet(ref HashSet<TypeWrapper>? accumulated, AssemblyScannerContext<TRegistrar> context, Type messageType)
    {
        if (!context.MessageMapping.TryGetValue(
                messageType.IsGenericType ?
                    messageType.GetGenericTypeDefinition() :
                    messageType,
                out var hashSet))
            return;

        if (accumulated is null)
        {
            accumulated = new HashSet<TypeWrapper>(hashSet);
        }
        else
        {
            accumulated.IntersectWith(hashSet);
        }
    }
}