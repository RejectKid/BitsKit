using System;
using System.IO;
using BitsKit.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class IO_ReaderTests
{
    private readonly byte[] Data =
    [
        0xCD, 0x0A, 162, 245, 92, 71, 202, 103, 218, 72
    ];

    private readonly int[] BitCounts =
    [
        7, 10, 43, 0, 13, 6
    ];

    [TestMethod]
    public void LSBSequentialMatchTest()
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            ulong expected = Helpers.ReadBitsLSB(Data, bitOffset, bitCount);

            ulong readerValue = bitReader.ReadUInt64LSB(bitCount);
            ulong memoryValue = memoryBitReader.ReadUInt64LSB(bitCount);
            ulong streamValue = bitStreamReader.ReadUInt64LSB(bitCount);

            Assert.AreEqual(expected, readerValue, "BitReader");
            Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
            Assert.AreEqual(expected, streamValue, "BitStreamReader");

            bitOffset += bitCount;

            Assert.AreEqual(bitOffset, bitReader.Position, "BitReader.Position");
            Assert.AreEqual(bitOffset, memoryBitReader.Position, "MemoryBitReader.Position");
            Assert.AreEqual(bitOffset, bitStreamReader.Position, "BitStreamReader.Position");
        }
    }

    [TestMethod]
    public void MSBSequentialMatchTest()
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            ulong expected = Helpers.ReadBitsMSB(Data, bitOffset, bitCount);

            ulong readerValue = bitReader.ReadUInt64MSB(bitCount);
            ulong memoryValue = memoryBitReader.ReadUInt64MSB(bitCount);
            ulong streamValue = bitStreamReader.ReadUInt64MSB(bitCount);

            Assert.AreEqual(expected, readerValue, "BitReader");
            Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
            Assert.AreEqual(expected, streamValue, "BitStreamReader");

            bitOffset += bitCount;

            Assert.AreEqual(bitOffset, bitReader.Position, "BitReader.Position");
            Assert.AreEqual(bitOffset, memoryBitReader.Position, "MemoryBitReader.Position");
            Assert.AreEqual(bitOffset, bitStreamReader.Position, "BitStreamReader.Position");
        }
    }

    [TestMethod]
    public void LSBNonSequentialMatchTest()
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        int bitOffset = 80;
        foreach (int bitCount in BitCounts)
        {
            bitOffset -= bitCount;

            ulong expected = Helpers.ReadBitsLSB(Data, bitOffset, bitCount);

            bitReader.Position = bitOffset;
            memoryBitReader.Position = bitOffset;
            bitStreamReader.Position = bitOffset;

            ulong readerValue = bitReader.ReadUInt64LSB(bitCount);
            ulong memoryValue = memoryBitReader.ReadUInt64LSB(bitCount);
            ulong streamValue = bitStreamReader.ReadUInt64LSB(bitCount);

            Assert.AreEqual(expected, readerValue, "BitReader");
            Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
            Assert.AreEqual(expected, streamValue, "BitStreamReader");
        }
    }

    [TestMethod]
    public void MSBNonSequentialMatchTest()
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        int bitOffset = 80;
        foreach (int bitCount in BitCounts)
        {
            bitOffset -= bitCount;

            ulong expected = Helpers.ReadBitsMSB(Data, bitOffset, bitCount);

            bitReader.Position = bitOffset;
            memoryBitReader.Position = bitOffset;
            bitStreamReader.Position = bitOffset;

            ulong readerValue = bitReader.ReadUInt64MSB(bitCount);
            ulong memoryValue = memoryBitReader.ReadUInt64MSB(bitCount);
            ulong streamValue = bitStreamReader.ReadUInt64MSB(bitCount);

            Assert.AreEqual(expected, readerValue, "BitReader");
            Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
            Assert.AreEqual(expected, streamValue, "BitStreamReader");
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void LSBBitMatchTest(int bitOffset)
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        bool expected = Helpers.ReadBitsLSB(Data, bitOffset, 1) == 1;

        bitReader.Position = bitOffset;
        memoryBitReader.Position = bitOffset;
        bitStreamReader.Position = bitOffset;

        bool readerValue = bitReader.ReadBitLSB();
        bool memoryValue = memoryBitReader.ReadBitLSB();
        bool streamValue = bitStreamReader.ReadBitLSB();

        Assert.AreEqual(expected, readerValue, "BitReader");
        Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
        Assert.AreEqual(expected, streamValue, "BitStreamReader");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void MSBBitMatchTest(int bitOffset)
    {
        using MemoryStream ms = new(Data);

        BitReader bitReader = new(Data);
        MemoryBitReader memoryBitReader = new(Data);
        BitStreamReader bitStreamReader = new(ms);

        bool expected = Helpers.ReadBitsMSB(Data, bitOffset, 1) == 1;

        bitReader.Position = bitOffset;
        memoryBitReader.Position = bitOffset;
        bitStreamReader.Position = bitOffset;

        bool readerValue = bitReader.ReadBitMSB();
        bool memoryValue = memoryBitReader.ReadBitMSB();
        bool streamValue = bitStreamReader.ReadBitMSB();

        Assert.AreEqual(expected, readerValue, "BitReader");
        Assert.AreEqual(expected, memoryValue, "MemoryBitReader");
        Assert.AreEqual(expected, streamValue, "BitStreamReader");
    }

    [TestMethod]
    public void ShortReadsAreCombined()
    {
        using ShortReadMemoryStream stream = new(Data);
        using BitStreamReader reader = new(stream);

        ulong expected = Helpers.ReadBitsLSB(Data, 0, 64);

        Assert.AreEqual(expected, reader.ReadUInt64LSB(64));
        Assert.AreEqual(64, reader.Position);
    }

    [TestMethod]
    public void SequentialReadsUseReadAheadBuffer()
    {
        using CountingReadMemoryStream stream = new(new byte[8192]);
        using BitStreamReader reader = new(stream);

        for (int i = 0; i < 1024; i++)
            reader.ReadUInt8LSB(8);

        Assert.AreEqual(1, stream.ReadCount);
        Assert.AreEqual(8192, reader.Position);
    }

    [TestMethod]
    public void SeekingWithinBufferedDataDoesNotReadAgain()
    {
        using CountingReadMemoryStream stream = new(Data);
        using BitStreamReader reader = new(stream);

        reader.ReadUInt64LSB(64);
        int readCount = stream.ReadCount;
        reader.Position = 8;

        Assert.AreEqual(Data[1], reader.ReadUInt8LSB(8));
        Assert.AreEqual(readCount, stream.ReadCount);
    }

    [TestMethod]
    public void ReadsAcrossInternalBufferBoundary()
    {
        byte[] data = new byte[4104];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)i;

        const int bitOffset = (4093 * 8) + 3;
        ulong expected = Helpers.ReadBitsMSB(data, bitOffset, 64);
        using MemoryStream stream = new(data);
        using BitStreamReader reader = new(stream);
        reader.Position = bitOffset;

        Assert.AreEqual(expected, reader.ReadUInt64MSB(64));
        Assert.AreEqual(bitOffset + 64, reader.Position);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SequentialBitReadsMatchAcrossBufferRefills(bool mostSignificant)
    {
        byte[] data = new byte[8201];
        new Random(0xB175).NextBytes(data);
        using MemoryStream stream = new(data);
        using BitStreamReader reader = new(stream);

        for (int bitOffset = 0; bitOffset < data.Length * 8; bitOffset++)
        {
            bool expected = mostSignificant
                ? Helpers.ReadBitsMSB(data, bitOffset, 1) != 0
                : Helpers.ReadBitsLSB(data, bitOffset, 1) != 0;
            bool actual = mostSignificant ? reader.ReadBitMSB() : reader.ReadBitLSB();

            Assert.AreEqual(expected, actual, $"Bit offset {bitOffset}");
        }
    }

    [TestMethod]
    public void RandomizedReadsMatchAcrossBufferRefillsAndSeeks()
    {
        byte[] data = new byte[8193];
        Random random = new(0xB175);
        random.NextBytes(data);
        using MemoryStream stream = new(data);
        using BitStreamReader reader = new(stream);

        for (int i = 0; i < 1000; i++)
        {
            int bitCount = random.Next(65);
            int bitOffset = random.Next((data.Length * 8) - bitCount + 1);
            reader.Position = bitOffset;

            ulong actual = (i & 1) == 0
                ? reader.ReadUInt64LSB(bitCount)
                : reader.ReadUInt64MSB(bitCount);
            ulong expected = (i & 1) == 0
                ? Helpers.ReadBitsLSB(data, bitOffset, bitCount)
                : Helpers.ReadBitsMSB(data, bitOffset, bitCount);

            Assert.AreEqual(expected, actual, $"Offset: {bitOffset}, Count: {bitCount}");
            Assert.AreEqual(bitOffset + bitCount, reader.Position);
        }
    }

    [TestMethod]
    public void LeaveOpenRestoresUnderlyingStreamPosition()
    {
        using MemoryStream stream = new(Data);
        using (BitStreamReader reader = new(stream, true))
            reader.ReadBitLSB();

        Assert.AreEqual(1, stream.Position);
        Assert.IsTrue(stream.CanRead);
    }

    [TestMethod]
    public void StartsAtUnderlyingStreamPosition()
    {
        using MemoryStream stream = new(Data);
        stream.Position = 3;
        using BitStreamReader reader = new(stream);

        Assert.AreEqual(24, reader.Position);
        Assert.AreEqual(Data[3], reader.ReadUInt8LSB(8));
        Assert.AreEqual(32, reader.Position);
    }

    [TestMethod]
    public void SequentialReadingSupportsNonSeekableStreams()
    {
        using NonSeekableReadStream stream = new(Data);
        using BitStreamReader reader = new(stream);

        Assert.AreEqual(Data[0], reader.ReadUInt8LSB(8));
        Assert.AreEqual(Data[1], reader.ReadUInt8LSB(8));
        Assert.ThrowsExactly<NotSupportedException>(() => _ = reader.Position);
    }

    [TestMethod]
    public void ReadingBitAtEndThrowsEndOfStreamException()
    {
        using MemoryStream stream = new([0xA5]);
        using BitStreamReader reader = new(stream);

        reader.ReadUInt8LSB(8);

        Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadBitLSB());
    }

    [TestMethod]
    public void ReadingValuePastEndThrowsEndOfStreamException()
    {
        using MemoryStream stream = new([0xA5]);
        using BitStreamReader reader = new(stream);

        Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadUInt16LSB(16));
    }

    [TestMethod]
    public void ReadingUnalignedValuePastEndThrowsEndOfStreamException()
    {
        using MemoryStream stream = new([0xA5]);
        using BitStreamReader reader = new(stream);

        reader.ReadUInt8LSB(7);

        Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadUInt8LSB(2));
    }

    [TestMethod]
    public void SupportsPositionsBeyondInt32ByteRange()
    {
        long bytePosition = (long)int.MaxValue + 42;
        using SparseStream stream = new(bytePosition + 1);
        using BitStreamReader reader = new(stream);

        reader.Position = bytePosition << 3;

        Assert.AreEqual(bytePosition, stream.Position);
        Assert.IsFalse(reader.ReadBitLSB());
        Assert.AreEqual((bytePosition << 3) + 1, reader.Position);
    }

    [TestMethod]
    public void MembersThrowObjectDisposedExceptionAfterDispose()
    {
        using MemoryStream stream = new([0xA5]);
        BitStreamReader reader = new(stream, true);
        reader.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = reader.Position);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = reader.Length);
        Assert.ThrowsExactly<ObjectDisposedException>(() => reader.ReadBitLSB());
        Assert.ThrowsExactly<ObjectDisposedException>(() => reader.Seek(0, SeekOrigin.Begin));
    }
}
