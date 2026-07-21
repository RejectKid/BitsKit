using BenchmarkDotNet.Attributes;
using BitsKit.IO;

namespace BitsKit.Benchmarks;

public partial class BitsKitBenchmark
{

    [Benchmark(OperationsPerInvoke = UInt1Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt1")]
    public int BitStreamReaderBit()
    {
        var maxNumIterations = BufferSize;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
            _ = reader.ReadBitMSB();
        }

        return 0;
    }
    [Benchmark(OperationsPerInvoke = UInt4Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt4")]
    public int BitStreamReaderUInt04()
    {
        const int bitCount = 4;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt8Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt8")]
    public int BitStreamReaderUInt08()
    {
        const int bitCount = 8;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
            _ = reader.ReadUInt8MSB(bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt16Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt16")]
    public int BitStreamReaderUInt16()
    {
        const int bitCount = 16;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
            _ = reader.ReadUInt16MSB(bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt32Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt32")]
    public int BitStreamReaderUInt32()
    {
        const int bitCount = 32;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt32Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt32", "Unaligned")]
    public int BitStreamReaderUInt32Unaligned()
    {
        const int bitCount = 32;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);
        _ = reader.ReadBitMSB();

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
            _ = reader.ReadUInt32MSB(bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt64Operations)]
    [BenchmarkCategory("BitStreamReader", "UInt64")]
    public int BitStreamReaderUInt64()
    {
        const int bitCount = 64;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(ReadBuffer);
        using var reader = new BitStreamReader(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
            _ = reader.ReadUInt64MSB(bitCount);
        }

        return 0;
    }
}
