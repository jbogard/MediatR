namespace MediatR.Internal;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class HandlersOrderer
{
    public static IList<object> Prioritize<TRequest>(IList<object> handlers, TRequest request)
        where TRequest : notnull
    {
        if (handlers.Count < 2)
        {
            return handlers;
        }

        var requestObjectDetails = new ObjectDetails(request);
        var handlerObjectsDetails = handlers.Select(static s => new ObjectDetails(s)).ToList();

        var uniqueHandlers = RemoveOverridden(handlerObjectsDetails).ToArray();
        Array.Sort(uniqueHandlers, requestObjectDetails);

        return uniqueHandlers.Select(static s => s.Value).ToList();
    }

    private static IEnumerable<ObjectDetails> RemoveOverridden(IList<ObjectDetails> handlersData)
    {
        var typeOverrides = new Dictionary<Type, bool>();

        foreach (var handler in handlersData)
        {
            if (handler.IsOverridden)
            {
                continue;
            }

            if (typeOverrides.TryGetValue(handler.Type, out var isOverridden))
            {
                handler.IsOverridden = isOverridden;
            }
            else
            {
                typeOverrides[handler.Type] = false;
            }
        }

        return handlersData.Where(static w => !w.IsOverridden);

    }
}