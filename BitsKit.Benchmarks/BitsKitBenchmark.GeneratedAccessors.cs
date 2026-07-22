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
    private readonly GeneratedAccessorAlignedMemoryModel[] _generatedAccessorAlignedMemoryModels = CreateGeneratedAccessorAlignedMemoryModels();
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
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Set", "MSB")]
    public uint GeneratedAccessorSetUInt32MSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].MostSignificantValue = (uint)i;

        return _generatedAccessorSetModels[0].MostSignificantBackingField;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Integral", "Set", "UInt64", "MSB")]
    public ulong GeneratedAccessorSetUInt64MSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].MostSignificantWideValue = (ulong)i;

        return _generatedAccessorSetModels[0].MostSignificantWideBackingField;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Boolean", "Set", "MSB")]
    public uint GeneratedAccessorSetBooleanMSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].MostSignificantFlag = (i & 1) != 0;

        return _generatedAccessorSetModels[0].MostSignificantBooleanBackingField;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Enum", "Set", "MSB")]
    public uint GeneratedAccessorSetEnumMSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorSetModels[i & AccessorModelMask].MostSignificantKind = (GeneratedAccessorKind)(i & 7);

        return _generatedAccessorSetModels[0].MostSignificantEnumBackingField;
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
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Aligned", "Get", "UInt32", "LSB")]
    public uint GeneratedAccessorGetAlignedMemoryUInt32LSB()
    {
        uint sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorAlignedMemoryModels[i & AccessorModelMask].UInt32Value;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Aligned", "Set", "UInt32", "LSB")]
    public uint GeneratedAccessorSetAlignedMemoryUInt32LSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorAlignedMemoryModels[i & AccessorModelMask].UInt32Value = (uint)i;

        return BitConverter.ToUInt32(_generatedAccessorAlignedMemoryModels[0].UInt32BackingField.Span);
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Aligned", "Get", "UInt64", "LSB")]
    public ulong GeneratedAccessorGetAlignedMemoryUInt64LSB()
    {
        ulong sum = 0;

        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorAlignedMemoryModels[i & AccessorModelMask].UInt64Value;

        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "Memory", "Aligned", "Set", "UInt64", "LSB")]
    public ulong GeneratedAccessorSetAlignedMemoryUInt64LSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorAlignedMemoryModels[i & AccessorModelMask].UInt64Value = (ulong)i;

        return BitConverter.ToUInt64(_generatedAccessorAlignedMemoryModels[0].UInt64BackingField.Span);
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
            models[i].MostSignificantWideBackingField = ((ulong)value << 32) | ~value;
            models[i].MostSignificantBooleanBackingField = value;
            models[i].MostSignificantEnumBackingField = value;
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

    private static GeneratedAccessorAlignedMemoryModel[] CreateGeneratedAccessorAlignedMemoryModels()
    {
        var models = new GeneratedAccessorAlignedMemoryModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            ulong value = unchecked((ulong)i * 0x9E3779B97F4A7C15UL);
            models[i].UInt32BackingField = BitConverter.GetBytes((uint)value);
            models[i].UInt64BackingField = BitConverter.GetBytes(value);
        }

        return models;
    }
}
