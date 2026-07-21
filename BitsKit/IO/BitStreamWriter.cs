using System.Buffers;
using BitsKit.Primitives;

namespace BitsKit.IO;

/// <summary>
/// A writer for packing bits into a stream
/// <remarks>
/// <para>
/// Sequential writes support forward-only streams. Seeking requires a seekable
/// stream, and writing in-place also requires it to be readable.
/// </para> 
/// </remarks>
/// </summary>
public sealed class BitStreamWriter : IBitWriter, IBitStream
{
    /// <inheritdoc cref="IBitStream.Position"/>
    public long Position
    {
        get
        {
            ThrowIfDisposed();
            long bytePosition = _isSeekable ? _stream.Position : _forwardOnlyBytePosition;
            return ((bytePosition + _writeBufferLength) << 3) + _bitsPos;
        }
        set => SetPosition(value);
    }

    /// <inheritdoc cref="IBitStream.Length"/>
    public long Length
    {
        get
        {
            ThrowIfDisposed();

            if (!_isSeekable)
                return (_forwardOnlyBytePosition + _writeBufferLength) << 3;

            return Math.Max(_stream.Length, _stream.Position + _writeBufferLength) << 3;
        }
    }

    private const int BufferSize = 4096;

    private Stream _stream;
    private byte[] _writeBuffer;
    private int _writeBufferLength;
    private long _forwardOnlyBytePosition;
    private byte _buffer;
    private int _bitsPos;

    private readonly bool _leaveOpen;
    private readonly bool _isSeekable;
    private bool _disposed;

    /// <summary>
    /// Initialises a new instance of the <see cref="BitStreamWriter"/> class using the specific stream
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="NotSupportedException"></exception>
    public BitStreamWriter(Stream source) : this(source, false)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="BitStreamWriter"/> class using the specific stream
    /// and optionally leaves the stream open
    /// </summary>
    /// <param name="source"></param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="source"/> does not support writing.</exception>
    public BitStreamWriter(Stream source, bool leaveOpen)
    {
        if (source is null)
            IBitStream.ThrowSourceNullException();

        if (!source.CanWrite)
            throw new NotSupportedException("Stream does not support writing.");

        _stream = source;
        _leaveOpen = leaveOpen;
        _isSeekable = source.CanSeek;

        ResetBuffer();
        _writeBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
    }

    /// <inheritdoc cref="IBitStream.Seek"/>
    public long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();
        EnsureSeekable();

        return Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
    }

    /// <summary>
    /// Clears all buffers for this stream and causes any buffered data 
    /// to be written to the underlying stream. Sequential writes may not be
    /// visible through the underlying stream until this method is called.
    /// </summary>
    public void Flush()
    {
        ThrowIfDisposed();
        FlushWriteBuffer();

        // write any buffered bits
        if (_bitsPos != 0)
        {
            _stream.WriteByte(_buffer);

            if (!_isSeekable)
                _forwardOnlyBytePosition++;

            _bitsPos = 0;
            ResetBuffer();
        }

        _stream.Flush();
    }

    /// <summary>
    /// Closes this stream and releases all associated resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                Flush();
            }
            finally
            {
                try
                {
                    if (!_leaveOpen)
                        _stream.Dispose();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(_writeBuffer, clearArray: true);
                    _stream = null!;
                    _writeBuffer = null!;
                    _disposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }
    }

    #region Methods

    /// <inheritdoc cref="IBitWriter.WriteBitLSB"/>
    public void WriteBitLSB(bool value)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(1);
            BitPrimitives.WriteBitLSB(appendBuffer, _bitsPos, value);
            CommitAppend(1);
            return;
        }

        Span<byte> buffer = stackalloc byte[1];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteBitLSB(buffer, _bitsPos, value);
        InternalWrite(buffer, 1);
    }

    /// <inheritdoc cref="IBitWriter.WriteBitMSB"/>
    public void WriteBitMSB(bool value)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(1);
            BitPrimitives.WriteBitMSB(appendBuffer, _bitsPos, value);
            CommitAppend(1);
            return;
        }

        Span<byte> buffer = stackalloc byte[1];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteBitMSB(buffer, _bitsPos, value);
        InternalWrite(buffer, 1);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt8LSB"/>
    public void WriteInt8LSB(sbyte value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt8LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[2];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt8LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt8MSB"/>
    public void WriteInt8MSB(sbyte value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt8MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[2];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt8MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt16LSB"/>
    public void WriteInt16LSB(short value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt16LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[3];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt16LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt16MSB"/>
    public void WriteInt16MSB(short value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt16MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[3];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt16MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt32LSB"/>
    public void WriteInt32LSB(int value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt32LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[5];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt32LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt32MSB"/>
    public void WriteInt32MSB(int value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt32MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[5];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt32MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt64LSB"/>
    public void WriteInt64LSB(long value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt64LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[9];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt64LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteInt64MSB"/>
    public void WriteInt64MSB(long value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteInt64MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[9];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteInt64MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt8LSB"/>
    public void WriteUInt8LSB(byte value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt8LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[2];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt8LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt8MSB"/>
    public void WriteUInt8MSB(byte value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt8MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[2];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt8MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt16LSB"/>
    public void WriteUInt16LSB(ushort value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt16LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[3];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt16LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt16MSB"/>
    public void WriteUInt16MSB(ushort value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt16MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[3];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt16MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt32LSB"/>
    public void WriteUInt32LSB(uint value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt32LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[5];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt32LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt32MSB"/>
    public void WriteUInt32MSB(uint value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt32MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[5];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt32MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt64LSB"/>
    public void WriteUInt64LSB(ulong value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt64LSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[9];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt64LSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    /// <inheritdoc cref="IBitWriter.WriteUInt64MSB"/>
    public void WriteUInt64MSB(ulong value, int bitCount)
    {
        if (IsSequentialAppend)
        {
            Span<byte> appendBuffer = PrepareAppendBuffer(bitCount);
            BitPrimitives.WriteUInt64MSB(appendBuffer, _bitsPos, value, bitCount);
            CommitAppend(bitCount);
            return;
        }

        Span<byte> buffer = stackalloc byte[9];
        PopulateWriteBuffer(buffer);

        BitPrimitives.WriteUInt64MSB(buffer, _bitsPos, value, bitCount);
        InternalWrite(buffer, bitCount);
    }

    #endregion

    private void ResetBuffer()
    {
        _buffer = 0;

        // if this is mid-stream, buffer the next byte
        if (_isSeekable && _writeBufferLength == 0 && _stream.Position < _stream.Length)
        {
            EnsureReadableForInPlaceWrite();

            int value = _stream.ReadByte();
            if (value < 0)
                throw new EndOfStreamException();

            _buffer = (byte)value;
            _stream.Position -= 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PopulateWriteBuffer(Span<byte> buffer)
    {
        ThrowIfDisposed();
        buffer.Clear();

        // if this is a mid-stream write then populate the buffer
        // with the existing data to allow writing in-place
        if (_isSeekable && _writeBufferLength == 0 && _stream.Position < _stream.Length)
        {
            EnsureReadableForInPlaceWrite();

            long position = _stream.Position;
            int bytesToRead = (int)Math.Min(buffer.Length, _stream.Length - position);

            try
            {
                ReadExactly(buffer[..bytesToRead]);
            }
            finally
            {
                _stream.Position = position;
            }
        }

        // preserve any unwritten bits
        if (_bitsPos != 0)
            buffer[0] = _buffer;
    }

    private void InternalWrite(Span<byte> buffer, int bitCount)
    {
        // number of whole bytes to write
        int writeLen = (bitCount + _bitsPos) >> 3;

        // write all the whole bytes
        if (writeLen != 0)
            _stream.Write(buffer[..writeLen]);

        _bitsPos = (_bitsPos + bitCount) & 7;

        // preserve any unwritten bits
        if (_bitsPos != 0)
            _buffer = buffer[writeLen];
    }

    private bool IsSequentialAppend
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return !_isSeekable || _writeBufferLength != 0 || _stream.Position >= _stream.Length;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<byte> PrepareAppendBuffer(int bitCount)
    {
        long totalBits = _bitsPos + (long)bitCount;
        int bytesRequired = (int)Math.Clamp((totalBits + 7) >> 3, 1, 9);

        if (BufferSize - _writeBufferLength < bytesRequired)
            FlushWriteBuffer();

        Span<byte> buffer = _writeBuffer.AsSpan(_writeBufferLength);
        buffer[..bytesRequired].Clear();

        if (_bitsPos != 0)
            buffer[0] = _buffer;

        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CommitAppend(int bitCount)
    {
        int totalBits = _bitsPos + bitCount;
        _writeBufferLength += totalBits >> 3;
        _bitsPos = totalBits & 7;
        _buffer = _bitsPos == 0 ? (byte)0 : _writeBuffer[_writeBufferLength];

        if (_writeBufferLength == BufferSize)
            FlushWriteBuffer();
    }

    private void SetPosition(long position)
    {
        ThrowIfDisposed();
        EnsureSeekable();

        if (position < 0)
            IBitStream.ThrowNegativePositionException();

        // calculate offsets
        long bytePos = position >> 3;
        int bitsPos = (int)(position & 7);
        long currentBytePos = _stream.Position + _writeBufferLength;

        // check if this is a Seek operation
        if (bytePos != currentBytePos)
        {
            // write any buffered bits
            Flush();
            // update the stream position
            _stream.Position = bytePos;
            // and repopulate the buffer
            ResetBuffer();
        }

        _bitsPos = bitsPos;
    }

    private void FlushWriteBuffer()
    {
        if (_writeBufferLength == 0)
            return;

        _stream.Write(_writeBuffer.AsSpan(0, _writeBufferLength));

        if (!_isSeekable)
            _forwardOnlyBytePosition += _writeBufferLength;

        _writeBufferLength = 0;
    }

    private void ReadExactly(Span<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            int bytesRead = _stream.Read(buffer);
            if (bytesRead == 0)
                throw new EndOfStreamException();

            buffer = buffer[bytesRead..];
        }
    }

    private void EnsureReadableForInPlaceWrite()
    {
        if (!_stream.CanRead)
            throw new NotSupportedException("Writing within existing stream data requires a readable stream.");
    }

    private void EnsureSeekable()
    {
        if (!_isSeekable)
            throw new NotSupportedException("Stream does not support seeking.");
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BitStreamWriter));
    }
}
