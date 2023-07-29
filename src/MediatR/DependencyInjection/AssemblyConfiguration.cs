using System.Reflection;
using MediatR.DependencyInjection.Configuration;

namespace MediatR.DependencyInjection;

internal readonly struct AssemblyConfiguration
{
    public Assembly Assembly { get; }
    public AssemblyScannerOptions ScannerOptions { get; }

    public AssemblyConfiguration(Assembly assembly, AssemblyScannerOptions scannerOptions)
    {
        Assembly = assembly;
        ScannerOptions = scannerOptions;
    }
}