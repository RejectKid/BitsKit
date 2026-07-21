using BenchmarkDotNet.Running;

namespace BitsKit.Benchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        var summaries = BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args)
            .ToArray();

        return summaries.Any(summary => summary.HasCriticalValidationErrors) ? 1 : 0;
    }
}
