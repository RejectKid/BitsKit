using System.Buffers;
using BitsKit.Primitives;

namespace BitsKit.IO;

/// <summary>
/// A reader for retrieving packed bits from a stream
/// </summary>
public sealed class BitStreamReader : IBitReader, IBitStream
{
    /// <inheritdoc cref="IBitStream.Position"/>
    public long Position
    {
        get
        {
            ThrowIfDisposed();

            if (!_stream.CanSeek)
                throw new NotSupportedException("Stream does not support seeking.");

            return _position;
        }
        set => SetPosition(value);
    }

    /// <inheritdoc cref="IBitStream.Length"/>
    public long Length
    {
        get
        {
            ThrowIfDisposed();
            return _stream.Length << 3;
        }
    }

    private const int BufferSize = 4096;

    // BitPrimitives can use a 128-bit unaligned load. Keep cleared padding
    // beyond the logical buffer so a read near its end remains memory-safe.
    private const int MaxPrimitiveBytes = 16;

    private Stream _stream;
    private byte[] _buffer;
    private int _bufferIndex;
    private int _bufferLength;
    private int _bitsPos;
    private long _bufferStart;
    private long _position;

    private readonly bool _leaveOpen;
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="BitStreamReader"/> class using the specific stream
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="NotSupportedException"></exception>
    public BitStreamReader(Stream source) : this(source, false)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="BitStreamReader"/> class using the specific stream
    /// and optionally leaves the stream open
    /// </summary>
    /// <param name="source"></param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="source"/> does not support reading.</exception>
    public BitStreamReader(Stream source, bool leaveOpen)
    {
        if (source is null)
            IBitStream.ThrowSourceNullException();

        if (!source.CanRead)
            throw new NotSupportedException("Stream does not support reading.");

        if (source.CanSeek)
        {
            _bufferStart = source.Position;
            _position = source.Position << 3;
        }

        _stream = source;
        _buffer = ArrayPool<byte>.Shared.Rent(BufferSize + MaxPrimitiveBytes);
        _buffer.AsSpan(0, BufferSize + MaxPrimitiveBytes).Clear();
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc cref="IBitStream.Seek"/>
    public long Seek(long offset, SeekOrigin origin)
    {
        return Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
    }

    /// <inheritdoc cref="BitStreamWriter.Dispose"/>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                if (_leaveOpen)
                {
                    if (_stream.CanSeek)
                        _stream.Position = (_position >> 3) + ((_position & 7) == 0 ? 0 : 1);
                }
                else
                {
                    _stream.Dispose();
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
                _stream = null!;
                _buffer = null!;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    #region Methods

    /// <inheritdoc cref="IBitReader.ReadBitLSB"/>
    public bool ReadBitLSB()
    {
        EnsureBitAvailable();

        bool value = (_buffer[_bufferIndex] & (1 << _bitsPos)) != 0;
        AdvanceBit();
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadBitMSB"/>
    public bool ReadBitMSB()
    {
        EnsureBitAvailable();

        bool value = (_buffer[_bufferIndex] & (1 << (7 - _bitsPos))) != 0;
        AdvanceBit();
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt8LSB"/>
    public sbyte ReadInt8LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        sbyte value = BitPrimitives.ReadInt8LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt8MSB"/>
    public sbyte ReadInt8MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        sbyte value = BitPrimitives.ReadInt8MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt16LSB"/>
    public short ReadInt16LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        short value = BitPrimitives.ReadInt16LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt16MSB"/>
    public short ReadInt16MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        short value = BitPrimitives.ReadInt16MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt32LSB"/>
    public int ReadInt32LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        int value = BitPrimitives.ReadInt32LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt32MSB"/>
    public int ReadInt32MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        int value = BitPrimitives.ReadInt32MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt64LSB"/>
    public long ReadInt64LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        long value = BitPrimitives.ReadInt64LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadInt64MSB"/>
    public long ReadInt64MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        long value = BitPrimitives.ReadInt64MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt8LSB"/>
    public byte ReadUInt8LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        byte value = BitPrimitives.ReadUInt8LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt8MSB"/>
    public byte ReadUInt8MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        byte value = BitPrimitives.ReadUInt8MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt16LSB"/>
    public ushort ReadUInt16LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        ushort value = BitPrimitives.ReadUInt16LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt16MSB"/>
    public ushort ReadUInt16MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        ushort value = BitPrimitives.ReadUInt16MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt32LSB"/>
    public uint ReadUInt32LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        uint value = BitPrimitives.ReadUInt32LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt32MSB"/>
    public uint ReadUInt32MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        uint value = BitPrimitives.ReadUInt32MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt64LSB"/>
    public ulong ReadUInt64LSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        ulong value = BitPrimitives.ReadUInt64LSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    /// <inheritdoc cref="IBitReader.ReadUInt64MSB"/>
    public ulong ReadUInt64MSB(int bitCount)
    {
        PopulateBuffer(bitCount);

        ulong value = BitPrimitives.ReadUInt64MSB(CurrentBuffer, _bitsPos, bitCount);
        Advance(bitCount);
        return value;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PopulateBuffer(int bitCount)
    {
        ThrowIfDisposed();

        if (bitCount == 0)
            return;

        int bytesRequired = (_bitsPos + bitCount + 7) >> 3;
        int bytesAvailable = _bufferLength - _bufferIndex;

        if (bytesAvailable >= bytesRequired || bytesRequired > BufferSize)
            return;

        if (_bufferIndex != 0)
        {
            _buffer.AsSpan(_bufferIndex, bytesAvailable).CopyTo(_buffer);
            _bufferStart += _bufferIndex;
            _bufferIndex = 0;
            _bufferLength = bytesAvailable;
        }

        while (_bufferLength < bytesRequired)
        {
            int bytesRead = _stream.Read(_buffer.AsSpan(_bufferLength, BufferSize - _bufferLength));
            if (bytesRead == 0)
                throw new EndOfStreamException();

            _bufferLength += bytesRead;
        }

        _buffer.AsSpan(_bufferLength, MaxPrimitiveBytes).Clear();
    }

    private void SetPosition(long position)
    {
        ThrowIfDisposed();

        if (position < 0)
            IBitStream.ThrowNegativePositionException();

        if (!_stream.CanSeek)
            throw new NotSupportedException("Stream does not support seeking.");

        // calculate offsets
        long bytePos = position >> 3;
        int bitsPos = (int)(position & 7);
        long bufferEnd = _bufferStart + _bufferLength;
        bool positionIsBuffered = bytePos >= _bufferStart &&
            (bytePos < bufferEnd || (bitsPos == 0 && bytePos == bufferEnd));

        if (positionIsBuffered)
        {
            _bufferIndex = (int)(bytePos - _bufferStart);
        }
        else
        {
            _stream.Position = bytePos;
            _bufferStart = bytePos;
            _bufferIndex = 0;
            _bufferLength = 0;
        }

        _bitsPos = bitsPos;
        _position = position;

        if (bitsPos != 0 && !positionIsBuffered)
            PopulateBuffer(1);
    }

    private ReadOnlySpan<byte> CurrentBuffer =>
        _buffer.AsSpan(_bufferIndex, Math.Max(1, _bufferLength - _bufferIndex));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBitAvailable()
    {
        ThrowIfDisposed();

        if (_bufferIndex >= _bufferLength)
            PopulateBuffer(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdvanceBit()
    {
        _bitsPos++;
        _position++;

        if (_bitsPos == 8)
        {
            _bitsPos = 0;
            _bufferIndex++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int bitCount)
    {
        int position = _bitsPos + bitCount;
        _bufferIndex += position >> 3;
        _bitsPos = position & 7;
        _position += bitCount;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BitStreamReader));
    }
}
