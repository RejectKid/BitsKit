using BenchmarkDotNet.Running;

namespace BitsKit.Benchmarks;

public static class Program
{
    public static int Main(string[] args)
    {
        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args).ToArray();
        bool informationOnly = args.Any(arg => arg is "--help" or "--list" or "--info" or "--version");

        if (summaries.Length == 0)
            return informationOnly ? 0 : 1;

        return summaries.Any(summary =>
            summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.Success)) ? 1 : 0;
    }
}
