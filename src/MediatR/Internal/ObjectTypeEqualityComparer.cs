using System.Collections.Generic;

namespace MediatR.Internal;

public class ObjectTypeEqualityComparer : IEqualityComparer<object?>
{
    public new bool Equals(object? x, object? y) => x?.GetType() == y?.GetType();

    public int GetHashCode(object? obj) => obj?.GetType().GetHashCode() ?? 0;
}