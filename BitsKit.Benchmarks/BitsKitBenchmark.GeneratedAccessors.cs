using BenchmarkDotNet.Attributes;

namespace BitsKit.Benchmarks;

public partial class BitsKitBenchmark
{
    private const int AccessorOperations = 4096;
    private const int AccessorModelCount = 1024;
    private const int AccessorModelMask = AccessorModelCount - 1;

    private readonly GeneratedAccessorLsbModel[] _generatedAccessorGetModels = CreateGeneratedAccessorModels();
    private readonly GeneratedAccessorLsbModel[] _generatedAccessorSetModels = CreateGeneratedAccessorModels();

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Get", "LSB")]
    public uint GeneratedAccessorGet()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorGetModels[i & AccessorModelMask].Value;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Set", "LSB")]
    public uint GeneratedAccessorSet()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].Value = (uint)i;

        return _generatedAccessorSetModels[0].BackingField;
    }

    private static GeneratedAccessorLsbModel[] CreateGeneratedAccessorModels()
    {
        var models = new GeneratedAccessorLsbModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
            models[i].BackingField = unchecked((uint)i * 0x9E3779B9u);

        return models;
    }
}
