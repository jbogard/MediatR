using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR.Pipeline;

namespace MediatR.Internal;

internal class ObjectDetails : IComparer<ObjectDetails>
{
    public string Name { get; }

    public string? AssemblyName { get; }

    public string? Location { get; }

    public object Value { get; }

    public Type Type { get; }

    public bool IsOverridden { get; set; }

    public ObjectDetails(object value)
    {
        Value = value;
        Type = Value.GetType();
        var exceptionHandlerType = value.GetType();

        Name = exceptionHandlerType.Name;
        AssemblyName = exceptionHandlerType.Assembly.GetName().Name;
        Location = exceptionHandlerType.Namespace?.Replace($"{AssemblyName}.", string.Empty);
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

        return CompareByOrder(x, y) ?? CompareByAssembly(x, y) ?? CompareByNamespace(x, y) ?? CompareByLocation(x, y);
    }

    /// <summary>
    /// Compare two objects according to an optional Order property that has been declared on many of the
    /// IRequestXXXHandler interfaces.
    /// </summary>
    /// <param name="x">First object to compare</param>
    /// <param name="y">Second object to compare</param>
    /// <returns>-1 if the left object should be ordered sooner, or 1 if the right object
    /// should be ordered sooner.  If either object does not support the Order
    /// property, or their orders match, then null is returned to allow pass-through
    /// handling.</returns>
    private int? CompareByOrder(ObjectDetails x, ObjectDetails y)
    {
        var leftOrder = x.Value.GetOrderIfExists();
        var rightOrder = y.Value.GetOrderIfExists();

        if (leftOrder is null && rightOrder is null) return null;

        if (leftOrder is null) return 1;    // always sort null after values
        if (rightOrder is null) return -1;  // same

        return leftOrder.Value.CompareTo(rightOrder.Value);
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
        if (Location is null || x.Location is null || y.Location is null)
        {
            return 0;
        }

        if (x.Location.StartsWith(Location, StringComparison.Ordinal) && !y.Location.StartsWith(Location, StringComparison.Ordinal))
        {
            return -1;
        }

        if (!x.Location.StartsWith(Location, StringComparison.Ordinal) && y.Location.StartsWith(Location, StringComparison.Ordinal))
        {
            return 1;
        }
        if (x.Location.StartsWith(Location, StringComparison.Ordinal) && y.Location.StartsWith(Location, StringComparison.Ordinal))
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
    /// An object has a higher priority if its location is part of the current location and the other is not;
    /// If both objects are part of the current location, the closest has higher priority;
    /// If none of the objects are part of the current location, they can be considered equal.
    /// </returns>
    private int CompareByLocation(ObjectDetails x, ObjectDetails y)
    {
        if (Location is null || x.Location is null || y.Location is null)
        {
            return 0;
        }

        if (Location.StartsWith(x.Location, StringComparison.Ordinal) && !Location.StartsWith(y.Location, StringComparison.Ordinal))
        {
            return -1;
        }

        if (!Location.StartsWith(x.Location, StringComparison.Ordinal) && Location.StartsWith(y.Location, StringComparison.Ordinal))
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