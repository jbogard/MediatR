﻿using System;
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

    public static Type[] GetMessageTypeHierarchy(Type? rootMessageType)
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
            return true;

        // The type here can only be a message type.
        // Therefor if is isn't any none generic message type, it has to be an generic message type.
        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == RequestResponseType ||
               genericTypeDefinition == StreamRequestType;
    }
}