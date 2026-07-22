using BenchmarkDotNet.Attributes;
using BitsKit.Primitives;

namespace BitsKit.Benchmarks;

public partial class BitsKitBenchmark
{
    private const int BatchOperations = 4096;
    private const int BatchBitOffset = 3;
    private const int BatchBitCount = 12;
    private const int BatchBitStride = 32;

    private readonly byte[] _batchReadBuffer = CreateBatchBuffer();
    private readonly byte[] _batchScalarWriteBuffer = CreateBatchBuffer();
    private readonly byte[] _batchWriteBuffer = CreateBatchBuffer();
    private readonly uint[] _batchScalarResults = new uint[BatchOperations];
    private readonly uint[] _batchResults = new uint[BatchOperations];
    private readonly uint[] _batchValues = CreateBatchValues();

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "ScalarLoop", "Read", "Contiguous", "LSB")]
    public uint BatchReadUInt32LSBScalarLoop()
    {
        for (int i = 0; i < _batchScalarResults.Length; i++)
        {
            _batchScalarResults[i] = BitPrimitives.ReadUInt32LSB(
                _batchReadBuffer,
                BatchBitOffset + i * BatchBitCount,
                BatchBitCount);
        }

        return _batchScalarResults[BatchOperations - 1];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "Batch", "Read", "Contiguous", "LSB")]
    public uint BatchReadUInt32LSB()
    {
        BitBatchPrimitives.ReadUInt32LSB(
            _batchReadBuffer,
            BatchBitOffset,
            BatchBitCount,
            _batchResults);
        return _batchResults[BatchOperations - 1];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "GeneratedAccessor", "Read", "Contiguous", "LSB")]
    public uint BatchGeneratedReadUInt32LSB()
    {
        GeneratedBatchAccessorModel.ReadValueBatch(_batchReadBuffer, _batchResults);
        return _batchResults[BatchOperations - 1];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "ScalarLoop", "Read", "Strided", "LSB")]
    public uint BatchStridedReadUInt32LSBScalarLoop()
    {
        for (int i = 0; i < _batchScalarResults.Length; i++)
        {
            _batchScalarResults[i] = BitPrimitives.ReadUInt32LSB(
                _batchReadBuffer,
                BatchBitOffset + i * BatchBitStride,
                BatchBitCount);
        }

        return _batchScalarResults[BatchOperations - 1];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "Batch", "Read", "Strided", "LSB")]
    public uint BatchStridedReadUInt32LSB()
    {
        BitBatchPrimitives.ReadUInt32LSB(
            _batchReadBuffer,
            BatchBitOffset,
            BatchBitCount,
            BatchBitStride,
            _batchResults);
        return _batchResults[BatchOperations - 1];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "ScalarLoop", "Write", "Contiguous", "LSB")]
    public byte BatchWriteUInt32LSBScalarLoop()
    {
        for (int i = 0; i < _batchValues.Length; i++)
        {
            BitPrimitives.WriteUInt32LSB(
                _batchScalarWriteBuffer,
                BatchBitOffset + i * BatchBitCount,
                _batchValues[i],
                BatchBitCount);
        }

        return _batchScalarWriteBuffer[0];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "Batch", "Write", "Contiguous", "LSB")]
    public byte BatchWriteUInt32LSB()
    {
        BitBatchPrimitives.WriteUInt32LSB(
            _batchWriteBuffer,
            BatchBitOffset,
            BatchBitCount,
            _batchValues);
        return _batchWriteBuffer[0];
    }

    [Benchmark(OperationsPerInvoke = BatchOperations)]
    [BenchmarkCategory("BatchPrimitives", "GeneratedAccessor", "Write", "Contiguous", "LSB")]
    public byte BatchGeneratedWriteUInt32LSB()
    {
        GeneratedBatchAccessorModel.WriteValueBatch(_batchWriteBuffer, _batchValues);
        return _batchWriteBuffer[0];
    }

    private static byte[] CreateBatchBuffer()
    {
        var buffer = new byte[((BatchOperations - 1) * BatchBitStride + BatchBitOffset + BatchBitCount + 7) / 8];
        FillBuffer(buffer);
        return buffer;
    }

    private static uint[] CreateBatchValues()
    {
        var values = new uint[BatchOperations];
        for (int i = 0; i < values.Length; i++)
            values[i] = unchecked((uint)i * 0x9E3779B9u);
        return values;
    }
}
