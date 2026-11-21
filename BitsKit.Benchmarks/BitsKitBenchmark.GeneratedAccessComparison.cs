using BenchmarkDotNet.Attributes;
using BitsKit.BitFields;

namespace BitsKit.Benchmarks;

public partial class BitsKitBenchmark
{
    private readonly GeneratedAccessorCheckedAccessModel[] _generatedAccessorCheckedAccessModels = CreateGeneratedAccessorCheckedAccessModels();
    private readonly GeneratedAccessorUnsafeAccessModel[] _generatedAccessorUnsafeAccessModels = CreateGeneratedAccessorUnsafeAccessModels();

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "CheckedAccess", "Memory", "Get", "20", "LSB")]
    public uint GeneratedAccessorAccessComparisonCheckedGetMemory20LSB()
    {
        uint sum = 0;
        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorCheckedAccessModels[i & AccessorModelMask].Value20;
        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "UnsafeAccess", "Memory", "Get", "20", "LSB")]
    public uint GeneratedAccessorAccessComparisonUnsafeGetMemory20LSB()
    {
        uint sum = 0;
        for (int i = 0; i < AccessorOperations; i++)
            sum += _generatedAccessorUnsafeAccessModels[i & AccessorModelMask].Value20;
        return sum;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "CheckedAccess", "Memory", "Set", "20", "LSB")]
    public uint GeneratedAccessorAccessComparisonCheckedSetMemory20LSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorCheckedAccessModels[i & AccessorModelMask].Value20 = (uint)i;
        return BitConverter.ToUInt32(_generatedAccessorCheckedAccessModels[0].Value20BackingField.Span);
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "UnsafeAccess", "Memory", "Set", "20", "LSB")]
    public uint GeneratedAccessorAccessComparisonUnsafeSetMemory20LSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorUnsafeAccessModels[i & AccessorModelMask].Value20 = (uint)i;
        return BitConverter.ToUInt32(_generatedAccessorUnsafeAccessModels[0].Value20BackingField.Span);
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "CheckedAccess", "Memory", "Boolean", "Get", "LSB")]
    public int GeneratedAccessorAccessComparisonCheckedGetMemoryBooleanLSB()
    {
        int count = 0;
        for (int i = 0; i < AccessorOperations; i++)
            count += _generatedAccessorCheckedAccessModels[i & AccessorModelMask].Flag ? 1 : 0;
        return count;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "UnsafeAccess", "Memory", "Boolean", "Get", "LSB")]
    public int GeneratedAccessorAccessComparisonUnsafeGetMemoryBooleanLSB()
    {
        int count = 0;
        for (int i = 0; i < AccessorOperations; i++)
            count += _generatedAccessorUnsafeAccessModels[i & AccessorModelMask].Flag ? 1 : 0;
        return count;
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "CheckedAccess", "Memory", "Boolean", "Set", "LSB")]
    public byte GeneratedAccessorAccessComparisonCheckedSetMemoryBooleanLSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorCheckedAccessModels[i & AccessorModelMask].Flag = (i & 1) != 0;
        return _generatedAccessorCheckedAccessModels[0].BooleanBackingField.Span[0];
    }

    [Benchmark(OperationsPerInvoke = AccessorOperations)]
    [BenchmarkCategory("GeneratedAccessor", "UnsafeAccess", "Memory", "Boolean", "Set", "LSB")]
    public byte GeneratedAccessorAccessComparisonUnsafeSetMemoryBooleanLSB()
    {
        for (int i = 0; i < AccessorOperations; i++)
            _generatedAccessorUnsafeAccessModels[i & AccessorModelMask].Flag = (i & 1) != 0;
        return _generatedAccessorUnsafeAccessModels[0].BooleanBackingField.Span[0];
    }

    private static GeneratedAccessorCheckedAccessModel[] CreateGeneratedAccessorCheckedAccessModels()
    {
        var models = new GeneratedAccessorCheckedAccessModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            byte[] value20 = new byte[16];
            byte[] boolean = new byte[16];
            uint value = unchecked((uint)i * 0x9E3779B9u);
            BitConverter.TryWriteBytes(value20, value);
            boolean[0] = (byte)value;
            models[i].Value20BackingField = value20;
            models[i].BooleanBackingField = boolean;
        }

        return models;
    }

    private static GeneratedAccessorUnsafeAccessModel[] CreateGeneratedAccessorUnsafeAccessModels()
    {
        var models = new GeneratedAccessorUnsafeAccessModel[AccessorModelCount];

        for (int i = 0; i < models.Length; i++)
        {
            byte[] value20 = new byte[16];
            byte[] boolean = new byte[16];
            uint value = unchecked((uint)i * 0x9E3779B9u);
            BitConverter.TryWriteBytes(value20, value);
            boolean[0] = (byte)value;
            models[i].Value20BackingField = value20;
            models[i].BooleanBackingField = boolean;
        }

        return models;
    }
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorCheckedAccessModel
{
    [BitField(3)]
    [BitField("Value20", 20, BitFieldType.UInt32)]
    public Memory<byte> Value20BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBackingField;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public partial struct GeneratedAccessorUnsafeAccessModel
{
    [BitField(3)]
    [BitField("Value20", 20, BitFieldType.UInt32)]
    public Memory<byte> Value20BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBackingField;
}
