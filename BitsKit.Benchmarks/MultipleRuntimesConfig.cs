using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using Perfolizer.Horology;
using Perfolizer.Metrology;

namespace BitsKit.Benchmarks;
internal class MultipleRuntimesConfig : ManualConfig
{
    public MultipleRuntimesConfig(MultipleRuntimesFlags flags, params string[] filters)
    {
        if (File.Exists(@"C:\Program Files\dotnet\dotnet.exe"))
        {
            if (flags.HasFlag(MultipleRuntimesFlags.net8_0_x64))
                AddJob(Job.Default
                    .WithPlatform(Platform.X64)
                    .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings
                    .NetCoreApp80
                    .WithCustomDotNetCliPath(@"C:\Program Files\dotnet\dotnet.exe", "64 bit 8.0")))
                    .WithId("64 bit 8.0"));

            if (flags.HasFlag(MultipleRuntimesFlags.net10_0_x64))
                AddJob(Job.Default
                    .WithPlatform(Platform.X64)
                    .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings
                    .NetCoreApp10_0
                    .WithCustomDotNetCliPath(@"C:\Program Files\dotnet\dotnet.exe", "64 bit 10.0")))
                    .WithId("64 bit 10.0"));
        }

        if (File.Exists(@"C:\Program Files (x86)\dotnet\dotnet.exe"))
        {
            if (flags.HasFlag(MultipleRuntimesFlags.net8_0_x86))
                AddJob(Job.Default
                    .WithPlatform(Platform.X86)
                    .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings
                    .NetCoreApp80
                    .WithCustomDotNetCliPath(@"C:\Program Files (x86)\dotnet\dotnet.exe", "32 bit 8.0")))
                    .WithId("32 bit 8.0"));

            if (flags.HasFlag(MultipleRuntimesFlags.net10_0_x86))
                AddJob(Job.Default
                    .WithPlatform(Platform.X86)
                    .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings
                    .NetCoreApp10_0
                    .WithCustomDotNetCliPath(@"C:\Program Files (x86)\dotnet\dotnet.exe", "32 bit 10.0")))
                    .WithId("32 bit 10.0"));
        }

        AddLogger(new ConsoleLogger());
        AddExporter(DefaultExporters.Plain);
        AddColumnProvider(DefaultColumnProviders.Instance);        

        if (filters?.Length > 0)
            AddFilter(new AllCategoriesFilter(filters));

        SummaryStyle = new SummaryStyle(null, true, SizeUnit.B, TimeUnit.Nanosecond);
    }

    [Flags]
    public enum MultipleRuntimesFlags
    {
        net8_0_x64 = 1,
        net8_0_x86 = 2,
        net10_0_x64 = 4,
        net10_0_x86 = 8,

        net8_0 = net8_0_x64 | net8_0_x86,
        net10_0 = net10_0_x64 | net10_0_x86,

        all_x64 = net8_0_x64 | net10_0_x64,
        all_x86 = net8_0_x86 | net10_0_x86,
        all = all_x86 | all_x64
    }
}
