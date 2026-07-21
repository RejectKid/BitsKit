using BenchmarkDotNet.Attributes;

namespace BitsKit.Benchmarks;

public partial class BitsKitBenchmark
{
    private const int AccessorOperations = 4096;
    private const int AccessorModelCount = 1024;
    private const int AccessorModelMask = AccessorModelCount - 1;

    private readonly GeneratedAccessorLsbModel[] _generatedAccessorGetModels = CreateGeneratedAccessorModels();
    private readonly GeneratedAccessorLsbModel[] _generatedAccessorSetModels = CreateGeneratedAccessorModels();
    private readonly GeneratedAccessorMemoryModel[] _generatedAccessorMemoryModels = CreateGeneratedAccessorMemoryModels();
    private readonly GeneratedAccessorInlineArrayModel[] _generatedAccessorInlineArrayModels = CreateGeneratedAccessorInlineArrayModels();

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Get", "LSB")]
    public uint GeneratedAccessorGetUInt32LSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorGetModels[i & AccessorModelMask].Value;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Set", "LSB")]
    public uint GeneratedAccessorSetUInt32LSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].Value = (uint)i;

        return _generatedAccessorSetModels[0].BackingField;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Get", "Signed", "LSB")]
    public int GeneratedAccessorGetInt32LSB()
    {
        int sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorGetModels[i & AccessorModelMask].SignedValue;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Boolean", "Get", "LSB")]
    public int GeneratedAccessorGetBooleanLSB()
    {
        int count = 0;

        for (int i = 0; i < AccessorOperations; i++)
            count += _generatedAccessorGetModels[i & AccessorModelMask].Flag ? 1 : 0;

        return count;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Enum", "Get", "LSB")]
    public uint GeneratedAccessorGetEnumLSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += (uint)_generatedAccessorGetModels[i & AccessorModelMask].Kind;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Get", "UInt64", "LSB")]
    public ulong GeneratedAccessorGetUInt64LSB()
    {
        ulong sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorGetModels[i & AccessorModelMask].WideValue;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Get", "MSB")]
    public uint GeneratedAccessorGetUInt32MSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorGetModels[i & AccessorModelMask].MostSignificantValue;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Get", "LSB")]
    public uint GeneratedAccessorGetMemoryLSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorMemoryModels[i & AccessorModelMask].Value;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Set", "LSB")]
    public uint GeneratedAccessorSetMemoryLSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorMemoryModels[i & AccessorModelMask].Value = (uint)i;

        return BitConverter.ToUInt32(_generatedAccessorMemoryModels[0].BackingField.Span);
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "InlineArray", "Get", "LSB")]
    public uint GeneratedAccessorGetInlineArrayLSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorInlineArrayModels[i & AccessorModelMask].Value;

        return sum;
    }

    private static GeneratedAccessorLsbModel[] CreateGeneratedAccessorModels()
    {
        var models = new GeneratedAccessorLsbModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            uint value = unchecked((uint)i * 0x9E3779B9u);
            models[i].BackingField = value;
            models[i].SignedBackingField = unchecked((int)(value ^ 0xA5A5A5A5u));
            models[i].BooleanBackingField = value;
            models[i].EnumBackingField = value;
            models[i].WideBackingField = ((ulong)value << 32) | ~value;
            models[i].MostSignificantBackingField = value;
        }

        return models;
    }

    private static GeneratedAccessorMemoryModel[] CreateGeneratedAccessorMemoryModels()
    {
        var models = new GeneratedAccessorMemoryModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            uint value = unchecked((uint)i * 0x9E3779B9u);
            models[i].BackingField = BitConverter.GetBytes(value);
        }

        return models;
    }

    private static GeneratedAccessorInlineArrayModel[] CreateGeneratedAccessorInlineArrayModels()
    {
        var models = new GeneratedAccessorInlineArrayModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            uint value = unchecked((uint)i * 0x9E3779B9u);
            models[i][0] = (byte)value;
            models[i][1] = (byte)(value >> 8);
            models[i][2] = (byte)(value >> 16);
            models[i][3] = (byte)(value >> 24);
        }

        return models;
    }
}
