using BenchmarkDotNet.Running;

namespace MediatR.Benchmarks;

internal sealed class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}