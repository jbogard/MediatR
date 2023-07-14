using System;
using System.Collections.Generic;

namespace MediatR.DependencyInjection;

internal partial struct AssemblyScanner<TRegistrar>
{
    private sealed class TypeComparerImplementation : Comparer<Type>
    {
        public override int Compare(Type x, Type y) => x.GUID.CompareTo(y.GUID);
    }
}