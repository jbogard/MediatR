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
        var overriddenSet = new HashSet<int>();

        for (var i = 0; i < handlersData.Count - 1; i++)
        {
            if (handlersData[i].IsOverridden || overriddenSet.Contains(i))
            {
                continue;
            }

            for (var j = i + 1; j < handlersData.Count; j++)
            {
                if (handlersData[j].IsOverridden || overriddenSet.Contains(j))
                {
                    continue;
                }

                if (handlersData[i].Type.IsAssignableFrom(handlersData[j].Type))
                {
                    handlersData[i].IsOverridden = true;
                    overriddenSet.Add(i);
                    break;
                }
                else if (handlersData[j].Type.IsAssignableFrom(handlersData[i].Type))
                {
                    handlersData[j].IsOverridden = true;
                    overriddenSet.Add(j);
                }
            }
        }

        return handlersData.Where(w => !w.IsOverridden);
    }
}