namespace MediatR.Internal
{
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
            var handlerObjectsDetails = handlers.Select(s => new ObjectDetails(s)).ToList();

            var uniqueHandlers = RemoveOverridden(handlerObjectsDetails).ToArray();
            Array.Sort(uniqueHandlers, requestObjectDetails);

            return uniqueHandlers.Select(s => s.Value).ToList();
        }

        private static IEnumerable<ObjectDetails> RemoveOverridden(IList<ObjectDetails> handlersData)
        {
            for (int i = 0; i < handlersData.Count - 1; i++)
            {
                for (int j = i + 1; j < handlersData.Count; j++)
                {
                    if (handlersData[i].IsOverridden || handlersData[j].IsOverridden)
                    {
                        continue;
                    }

                    if (handlersData[i].Type.IsAssignableFrom(handlersData[j].Type))
                    {
                        handlersData[i].IsOverridden = true;
                    }
                    else if (handlersData[j].Type.IsAssignableFrom(handlersData[i].Type))
                    {
                        handlersData[j].IsOverridden = true;
                    }
                }
            }

            return handlersData.Where(w => !w.IsOverridden);
        }
    }

    internal class ObjectDetails : IComparer<ObjectDetails>
    {
        public string Name { get; }

        public string AssemblyName { get; }

        public string Location { get; }

        public object Value { get; }

        public Type Type { get; }

        public bool IsOverridden { get; set; }

        public ObjectDetails(object value)
        {
            Value = value;
            Type = Value.GetType();
            var exceptionHandlerType = value.GetType();

            Name = exceptionHandlerType.Name;
            AssemblyName = exceptionHandlerType.Assembly.GetName().Name!;
            Location = exceptionHandlerType.Namespace!.Replace($"{AssemblyName}.", string.Empty);
        }

        public int Compare(ObjectDetails? x, ObjectDetails? y)
        {
            if (x == null)
            {
                return 1;
            }

            if (y == null)
            {
                return -1;
            }

            var compareByAssemblyResult = CompareByAssembly(x, y);
            if (compareByAssemblyResult != null)
            {
                return compareByAssemblyResult.Value;
            }

            var compareByNamespaceResult = CompareByNamespace(x, y);
            if (compareByNamespaceResult != null)
            {
                return compareByNamespaceResult.Value;
            }

            return CompareByLocation(x, y);
        }

        /// <summary>
        /// Compare two objects according to current assembly
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it belongs to the current assembly and the other is not;
        /// If none of the objects belong to the current assembly, they can be considered equal;
        /// If both objects belong to the current assembly, they can't be compared only by this criterion.
        /// </returns>
        private int? CompareByAssembly(ObjectDetails x, ObjectDetails y)
        {
            if (x.AssemblyName == AssemblyName && y.AssemblyName != AssemblyName)
            {
                return -1;
            }

            if (x.AssemblyName != AssemblyName && y.AssemblyName == AssemblyName)
            {
                return 1;
            }
            if (x.AssemblyName != AssemblyName && y.AssemblyName != AssemblyName)
            {
                return 0;
            }

            return null;
        }

        /// <summary>
        /// Compare two objects according to current namespace
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it belongs to the current/child namespace and the other is not;
        /// If both objects belong to the current/child namespace, they can be considered equal;
        /// If none of the objects belong to the current/child namespace, they can't be compared by this criterion.
        /// </returns>
        private int? CompareByNamespace(ObjectDetails x, ObjectDetails y)
        {
            if (x.Location.StartsWith(Location) && !y.Location.StartsWith(Location))
            {
                return -1;
            }

            if (!x.Location.StartsWith(Location) && y.Location.StartsWith(Location))
            {
                return 1;
            }
            if (x.Location.StartsWith(Location) && y.Location.StartsWith(Location))
            {
                return 0;
            }

            return null;
        }

        /// <summary>
        /// Compare two objects according to location in the assembly
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it location is part of the current location and the other is not;
        /// If both objects are part of the current location, the closest has higher priority;
        /// If none of the objects are part of the current location, they can be considered equal.
        /// </returns>
        private int CompareByLocation(ObjectDetails x, ObjectDetails y)
        {
            if (Location.StartsWith(x.Location) && !Location.StartsWith(y.Location))
            {
                return -1;
            }

            if (!Location.StartsWith(x.Location) && Location.StartsWith(y.Location))
            {
                return 1;
            }
            if (x.Location.Length > y.Location.Length)
            {
                return -1;
            }
            if (x.Location.Length < y.Location.Length)
            {
                return 1;
            }
            return 0;
        }
    }
}
