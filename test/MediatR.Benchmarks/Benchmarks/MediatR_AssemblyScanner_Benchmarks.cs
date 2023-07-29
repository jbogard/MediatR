using BenchmarkDotNet.Attributes;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Benchmarks.Mediator;

[MemoryDiagnoser]
public class MediatR_AssemblyScanner_Benchmarks
{
    private readonly ServiceCollection serviceCollection = new();

    [Benchmark]
    public void ScanAllTypes() =>
        serviceCollection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<MediatR_AssemblyScanner_Benchmarks>();
        });
}