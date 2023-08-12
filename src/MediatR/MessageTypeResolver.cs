using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MediatR;

internal sealed class MessageTypeResolver
{
    private static readonly ConcurrentDictionary<Type, (Type Request, Type Response)> _messageTypeMapping = new();

    public static (Type Request, Type Response) GetMessageType(Type messageType, Type openGenericInterface)
    {
        Debug.Assert(openGenericInterface.IsGenericType, "open generic interface must be open generic.");

        if (_messageTypeMapping.TryGetValue(messageType, out var messageInterfaceType))
        {
            return messageInterfaceType;
        }

        messageInterfaceType = default;

        foreach (var implementingInterface in messageType.GetInterfaces())
        {
            if (implementingInterface.GetGenericTypeDefinition() == openGenericInterface)
            {
                messageInterfaceType = (messageType, implementingInterface.GetGenericArguments()[0]);
                _ = _messageTypeMapping.TryAdd(messageType, messageInterfaceType);
            }
        }

        if (messageInterfaceType == default)
        {
            throw new InvalidOperationException($"Message '{messageType}' is not a '{openGenericInterface}'.");
        }

        return messageInterfaceType;
    }

    public static bool TryGetMessageType(Type messageType, Type openGenericInterface, out (Type Request, Type Response) messageInterfaceType)
    {
        Debug.Assert(openGenericInterface.IsGenericType, "open generic interface must be open generic.");

        if (_messageTypeMapping.TryGetValue(messageType, out messageInterfaceType))
        {
            return true;
        }

        messageInterfaceType = default;

        foreach (var implementingInterface in messageType.GetInterfaces())
        {
            if (implementingInterface.IsGenericType && implementingInterface.GetGenericTypeDefinition() == openGenericInterface)
            {
                messageInterfaceType = (messageType, implementingInterface.GetGenericArguments()[0]);
                _ = _messageTypeMapping.TryAdd(messageType, messageInterfaceType);
            }
        }

        return messageInterfaceType != default;
    }
}