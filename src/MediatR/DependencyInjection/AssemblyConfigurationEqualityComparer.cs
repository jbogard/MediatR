using System.Collections.Generic;

namespace MediatR.DependencyInjection;

internal sealed class AssemblyConfigurationEqualityComparer : IEqualityComparer<AssemblyConfiguration>
{
    public static AssemblyConfigurationEqualityComparer Instance { get; } = new();

    private AssemblyConfigurationEqualityComparer()
    {
    }
    
    public bool Equals(AssemblyConfiguration x, AssemblyConfiguration y) => 
        x.Assembly.Equals(y.Assembly);

    public int GetHashCode(AssemblyConfiguration obj) => obj.Assembly.GetHashCode();
}