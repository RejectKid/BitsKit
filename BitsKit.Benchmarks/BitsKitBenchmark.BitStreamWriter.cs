using BenchmarkDotNet.Attributes;
using BitsKit.IO;

namespace BitsKit.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

public partial class BitsKitBenchmark
{
    [Benchmark(OperationsPerInvoke = UInt1Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt1")]
    public int BitStreamWriterBit()
    {
        var maxNumIterations = BufferSize;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteBitMSB(false);
            writer.WriteBitMSB(true);
            writer.WriteBitMSB(false);
            writer.WriteBitMSB(true);
            writer.WriteBitMSB(false);
            writer.WriteBitMSB(true);
            writer.WriteBitMSB(false);
            writer.WriteBitMSB(true);
        }

        return 0;
    }
    [Benchmark(OperationsPerInvoke = UInt4Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt4")]
    public int BitStreamWriterUInt04()
    {
        const int bitCount = 4;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt8MSB((byte)numIterations, 3);
            writer.WriteUInt8MSB((byte)numIterations, 4);
            writer.WriteUInt8MSB((byte)numIterations, 3);
            writer.WriteUInt8MSB((byte)numIterations, 4);
            writer.WriteUInt8MSB((byte)numIterations, 3);
            writer.WriteUInt8MSB((byte)numIterations, 4);
            writer.WriteUInt8MSB((byte)numIterations, 3);
            writer.WriteUInt8MSB((byte)numIterations, 4);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt8Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt8")]
    public int BitStreamWriterUInt08()
    {
        const int bitCount = 8;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt8MSB((byte)numIterations, 1);
            writer.WriteUInt8MSB((byte)numIterations, 2);
            writer.WriteUInt8MSB((byte)numIterations, 3);
            writer.WriteUInt8MSB((byte)numIterations, 4);
            writer.WriteUInt8MSB((byte)numIterations, 5);
            writer.WriteUInt8MSB((byte)numIterations, 6);
            writer.WriteUInt8MSB((byte)numIterations, 7);
            writer.WriteUInt8MSB((byte)numIterations, 8);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt16Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt16")]
    public int BitStreamWriterUInt16()
    {
        const int bitCount = 16;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt16MSB((ushort)numIterations, 15);
            writer.WriteUInt16MSB((ushort)numIterations, 16);
            writer.WriteUInt16MSB((ushort)numIterations, 15);
            writer.WriteUInt16MSB((ushort)numIterations, 16);
            writer.WriteUInt16MSB((ushort)numIterations, 15);
            writer.WriteUInt16MSB((ushort)numIterations, 16);
            writer.WriteUInt16MSB((ushort)numIterations, 15);
            writer.WriteUInt16MSB((ushort)numIterations, 16);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt32Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt32", "Unaligned")]
    public int BitStreamWriterUInt32()
    {
        const int bitCount = 32;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt32MSB((uint)numIterations, 31);
            writer.WriteUInt32MSB((uint)numIterations, 32);
            writer.WriteUInt32MSB((uint)numIterations, 31);
            writer.WriteUInt32MSB((uint)numIterations, 32);
            writer.WriteUInt32MSB((uint)numIterations, 31);
            writer.WriteUInt32MSB((uint)numIterations, 32);
            writer.WriteUInt32MSB((uint)numIterations, 31);
            writer.WriteUInt32MSB((uint)numIterations, 32);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt32Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt32", "Aligned")]
    public int BitStreamWriterUInt32Aligned()
    {
        const int bitCount = 32;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
        }

        return 0;
    }

    [Benchmark(OperationsPerInvoke = UInt32Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt32", "Aligned", "ForwardOnly")]
    public long BitStreamWriterUInt32ForwardOnly()
    {
        const int bitCount = 32;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new ForwardOnlyWriteStream();
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
            writer.WriteUInt32MSB((uint)numIterations, bitCount);
        }

        writer.Flush();
        return stream.BytesWritten;
    }

    [Benchmark(OperationsPerInvoke = UInt64Operations)]
    [BenchmarkCategory("BitStreamWriter", "UInt64")]
    public int BitStreamWriterUInt64()
    {
        const int bitCount = 64;
        int maxNumIterations = BufferSize / bitCount;
        using var stream = new MemoryStream(BufferSize);
        using var writer = new BitStreamWriter(stream);

        for (var numIterations = 0; numIterations < maxNumIterations; numIterations++)
        {
            writer.WriteUInt64MSB((ulong)numIterations, 63);
            writer.WriteUInt64MSB((ulong)numIterations, 64);
            writer.WriteUInt64MSB((ulong)numIterations, 63);
            writer.WriteUInt64MSB((ulong)numIterations, 64);
            writer.WriteUInt64MSB((ulong)numIterations, 63);
            writer.WriteUInt64MSB((ulong)numIterations, 64);
            writer.WriteUInt64MSB((ulong)numIterations, 63);
            writer.WriteUInt64MSB((ulong)numIterations, 64);
        }

        return 0;
    }

    private sealed class ForwardOnlyWriteStream : Stream
    {
        public long BytesWritten { get; private set; }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => BytesWritten += count;
        public override void Write(ReadOnlySpan<byte> buffer) => BytesWritten += buffer.Length;
        public override void WriteByte(byte value) => BytesWritten++;
    }
}
