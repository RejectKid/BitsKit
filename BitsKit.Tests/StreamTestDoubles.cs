using System;
using System.IO;

namespace BitsKit.Tests;

internal sealed class ShortReadMemoryStream(byte[] buffer) : MemoryStream(buffer)
{
    public override int Read(byte[] buffer, int offset, int count) =>
        base.Read(buffer, offset, Math.Min(count, 1));

    public override int Read(Span<byte> buffer) =>
        base.Read(buffer[..Math.Min(buffer.Length, 1)]);
}

internal sealed class CountingReadMemoryStream(byte[] buffer) : MemoryStream(buffer)
{
    public int ReadCount { get; private set; }

    public override int Read(Span<byte> buffer)
    {
        ReadCount++;
        return base.Read(buffer);
    }
}

internal sealed class NonSeekableWriteStream : MemoryStream
{
    public override bool CanSeek => false;
}

internal sealed class NonSeekableReadStream(byte[] buffer) : MemoryStream(buffer)
{
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
}

internal sealed class SparseStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override long Length { get; }
    public override long Position { get; set; }

    public SparseStream(long length)
    {
        Length = length;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = GetReadLength(count);
        Array.Clear(buffer, offset, bytesRead);
        Position += bytesRead;
        return bytesRead;
    }

    public override int Read(Span<byte> buffer)
    {
        int bytesRead = GetReadLength(buffer.Length);
        buffer[..bytesRead].Clear();
        Position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        return Position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => Position += count;

    public override void Write(ReadOnlySpan<byte> buffer) => Position += buffer.Length;

    private int GetReadLength(int requestedLength) =>
        (int)Math.Min(requestedLength, Math.Max(0, Length - Position));
}
