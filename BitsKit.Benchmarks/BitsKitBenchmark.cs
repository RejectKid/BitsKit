namespace BitsKit.Benchmarks;

[BenchmarkDotNet.Attributes.CategoriesColumn]
[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public partial class BitsKitBenchmark
{
    private const int BufferSize = 10000;
    private const int UInt1Operations = (BufferSize / 1) * 8;
    private const int UInt4Operations = (BufferSize / 4) * 8;
    private const int UInt8Operations = (BufferSize / 8) * 8;
    private const int UInt16Operations = (BufferSize / 16) * 8;
    private const int UInt32Operations = (BufferSize / 32) * 8;
    private const int UInt64Operations = (BufferSize / 64) * 8;

    private readonly byte[] ReadBuffer = new byte[BufferSize];
    private readonly byte[] WriteBuffer = new byte[BufferSize];

    public BitsKitBenchmark()
    {
        FillBuffer(ReadBuffer);
        FillBuffer(WriteBuffer);
    }

    private static void FillBuffer(Span<byte> buffer)
    {
        uint h = 2166136261u;

        for (int i = 0; i < buffer.Length; i++)
        {
            h ^= (h << 5) + (h >> 2) + (uint)i;
            buffer[i] = (byte)h;
        }
    }
}
