using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatR.ExceptionHandling;

internal static class MessageTypeResolver
{
    private static readonly Type ObjectType = typeof(object);
    private static readonly Type RequestType = typeof(IRequest);
    private static readonly Type RequestResponseType = typeof(IRequest<>);
    private static readonly Type StreamRequestType = typeof(IStreamRequest<>);
    private static readonly Type NotificationType = typeof(INotification);

    [ThreadStatic]
    private static List<Type>? arrayBuilder;

    public static Type[] MessageTypeHierarchyFactory(Type? rootMessageType)
    {
        arrayBuilder ??= new List<Type>();

        while (rootMessageType is not null && rootMessageType.GetInterfaces().Any(IsMessageType) && rootMessageType != ObjectType)
        {
            arrayBuilder.Add(rootMessageType);
            rootMessageType = rootMessageType.BaseType;
        }

        var result = arrayBuilder.ToArray();
        arrayBuilder.Clear();
        return result;
    }

    private static bool IsMessageType(Type type)
    {
        if (type == RequestType || type == NotificationType)
        {
            return true;
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == RequestResponseType || genericTypeDefinition == StreamRequestType)
            {
                return true;
            }
        }

        return false;
    }
}