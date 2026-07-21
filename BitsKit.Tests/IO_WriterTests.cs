using System;
using System.IO;
using System.Runtime.CompilerServices;
using BitsKit.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class IO_WriterTests
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
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            Helpers.WriteBitsLSB(expected, bitOffset, value, bitCount);

            bitWriter.WriteUInt64LSB(value, bitCount);
            memoryBitWriter.WriteUInt64LSB(value, bitCount);
            bitStreamWriter.WriteUInt64LSB(value, bitCount);

            bitOffset += bitCount;

            // BitStreamWriter buffers partially written bytes
            // so only comparing the number of whole bytes written
            int byteCount = bitOffset >> 3;

            CollectionAssert.AreEqual(expected[..byteCount], data1[..byteCount], "BitWriter");
            CollectionAssert.AreEqual(expected[..byteCount], data2[..byteCount], "MemoryBitWriter");
            CollectionAssert.AreEqual(expected[..byteCount], data3[..byteCount], "BitStreamWriter");

            Assert.AreEqual(bitOffset, bitWriter.Position, "BitWriter.Position");
            Assert.AreEqual(bitOffset, memoryBitWriter.Position, "MemoryBitWriter.Position");
            Assert.AreEqual(bitOffset, bitStreamWriter.Position, "BitStreamWriter.Position");
        }

        // write remaining bits to stream
        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, data1, "BitWriter");
        CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
        CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
    }

    [TestMethod]
    public void MSBSequentialMatchTest()
    {
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            Helpers.WriteBitsMSB(expected, bitOffset, value, bitCount);

            bitWriter.WriteUInt64MSB(value, bitCount);
            memoryBitWriter.WriteUInt64MSB(value, bitCount);
            bitStreamWriter.WriteUInt64MSB(value, bitCount);

            bitOffset += bitCount;

            // BitStreamWriter buffers partially written bytes
            // so only comparing the number of whole bytes written
            int byteCount = bitOffset >> 3;

            CollectionAssert.AreEqual(expected[..byteCount], data1[..byteCount], "BitWriter");
            CollectionAssert.AreEqual(expected[..byteCount], data2[..byteCount], "MemoryBitWriter");
            CollectionAssert.AreEqual(expected[..byteCount], data3[..byteCount], "BitStreamWriter");

            Assert.AreEqual(bitOffset, bitWriter.Position, "BitWriter.Position");
            Assert.AreEqual(bitOffset, memoryBitWriter.Position, "MemoryBitWriter.Position");
            Assert.AreEqual(bitOffset, bitStreamWriter.Position, "BitStreamWriter.Position");
        }

        // write remaining bits to stream
        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, data1, "BitWriter");
        CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
        CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
    }

    [TestMethod]
    public void LSBNonSequentialMatchTest()
    {
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 80;
        foreach (int bitCount in BitCounts)
        {
            bitOffset -= bitCount;

            Helpers.WriteBitsLSB(expected, bitOffset, value, bitCount);

            bitWriter.Position = bitOffset;
            memoryBitWriter.Position = bitOffset;
            bitStreamWriter.Position = bitOffset;

            bitWriter.WriteUInt64LSB(value, bitCount);
            memoryBitWriter.WriteUInt64LSB(value, bitCount);
            bitStreamWriter.WriteUInt64LSB(value, bitCount);

            bitStreamWriter.Flush();

            CollectionAssert.AreEqual(expected, data1, "BitWriter");
            CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
            CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
        }
    }

    [TestMethod]
    public void MSBNonSequentialMatchTest()
    {
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 80;
        foreach (int bitCount in BitCounts)
        {
            bitOffset -= bitCount;

            Helpers.WriteBitsMSB(expected, bitOffset, value, bitCount);

            bitWriter.Position = bitOffset;
            memoryBitWriter.Position = bitOffset;
            bitStreamWriter.Position = bitOffset;

            bitWriter.WriteUInt64MSB(value, bitCount);
            memoryBitWriter.WriteUInt64MSB(value, bitCount);
            bitStreamWriter.WriteUInt64MSB(value, bitCount);

            bitStreamWriter.Flush();

            CollectionAssert.AreEqual(expected, data1, "BitWriter");
            CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
            CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void LSBBitMatchTest(int bitOffset)
    {
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        Helpers.WriteBitsLSB(expected, bitOffset, 1, 1);

        bitWriter.Position = bitOffset;
        memoryBitWriter.Position = bitOffset;
        bitStreamWriter.Position = bitOffset;

        bitWriter.WriteBitLSB(true);
        memoryBitWriter.WriteBitLSB(true);
        bitStreamWriter.WriteBitLSB(true);

        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, data1, "BitWriter");
        CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
        CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void MSBBitMatchTest(int bitOffset)
    {
        byte[] expected = Data[..];
        byte[] data1 = Data[..];
        byte[] data2 = Data[..];
        byte[] data3 = Data[..];
        using MemoryStream ms = new(data3);

        BitWriter bitWriter = new(data1);
        MemoryBitWriter memoryBitWriter = new(data2);
        BitStreamWriter bitStreamWriter = new(ms);

        Helpers.WriteBitsMSB(expected, bitOffset, 1, 1);

        bitWriter.Position = bitOffset;
        memoryBitWriter.Position = bitOffset;
        bitStreamWriter.Position = bitOffset;

        bitWriter.WriteBitMSB(true);
        memoryBitWriter.WriteBitMSB(true);
        bitStreamWriter.WriteBitMSB(true);

        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, data1, "BitWriter");
        CollectionAssert.AreEqual(expected, data2, "MemoryBitWriter");
        CollectionAssert.AreEqual(expected, data3, "BitStreamWriter");
    }

    [TestMethod]
    public void WriteOnlyStreamTestLSB()
    {
        byte[] expected = new byte[10];

        using WriteOnlyMemoryStream ms = new();
        using BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            Helpers.WriteBitsLSB(expected, bitOffset, value, bitCount);

            bitStreamWriter.WriteUInt64LSB(value, bitCount);

            bitOffset += bitCount;

            Assert.AreEqual(bitOffset, bitStreamWriter.Position, "BitStreamWriter.Position");
            Assert.AreEqual(bitOffset & ~7, bitStreamWriter.Length, "BitStreamWriter.Length");
        }

        // write remaining bits to stream
        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, ms.GetBuffer((int)ms.Length), "BitStreamWriter");

        // check we can't seek or buffer
        Assert.ThrowsExactly<NotSupportedException>(() => bitStreamWriter.Position = 0);
    }

    [TestMethod]
    public void WriteOnlyStreamTestMSB()
    {
        byte[] expected = new byte[10];

        using WriteOnlyMemoryStream ms = new();
        using BitStreamWriter bitStreamWriter = new(ms);

        ulong value = Unsafe.ReadUnaligned<ulong>(ref Data[2]);

        int bitOffset = 0;
        foreach (int bitCount in BitCounts)
        {
            Helpers.WriteBitsMSB(expected, bitOffset, value, bitCount);

            bitStreamWriter.WriteUInt64MSB(value, bitCount);

            bitOffset += bitCount;

            Assert.AreEqual(bitOffset, bitStreamWriter.Position, "BitStreamWriter.Position");
            Assert.AreEqual(bitOffset & ~7, bitStreamWriter.Length, "BitStreamWriter.Length");
        }

        // write remaining bits to stream
        bitStreamWriter.Flush();

        CollectionAssert.AreEqual(expected, ms.GetBuffer((int)ms.Length), "BitStreamWriter");

        // check we can't seek or buffer
        Assert.ThrowsExactly<NotSupportedException>(() => bitStreamWriter.Position = 0);
    }

    [TestMethod]
    public void ShortReadsAreCombinedForInPlaceWrites()
    {
        byte[] expected = Data[..];
        byte[] actual = Data[..];
        Helpers.WriteBitsLSB(expected, 3, 0x0123456789ABCDEF, 64);

        using ShortReadMemoryStream stream = new(actual);
        using BitStreamWriter writer = new(stream);
        writer.Position = 3;

        writer.WriteUInt64LSB(0x0123456789ABCDEF, 64);
        writer.Flush();

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void WritingPastEndInitializesNewBytes()
    {
        byte[] expected = new byte[9];
        expected[0] = 0xA5;
        Helpers.WriteBitsLSB(expected, 3, 0x0123456789ABCDEF, 64);

        using MemoryStream stream = new();
        stream.WriteByte(0xA5);
        stream.Position = 0;
        using BitStreamWriter writer = new(stream, true);
        writer.Position = 3;

        writer.WriteUInt64LSB(0x0123456789ABCDEF, 64);
        writer.Flush();

        CollectionAssert.AreEqual(expected, stream.ToArray());
    }

    [TestMethod]
    public void SequentialWritesUseOutputBuffer()
    {
        using CountingWriteMemoryStream stream = new();
        using BitStreamWriter writer = new(stream, true);

        for (int i = 0; i < 1024; i++)
            writer.WriteUInt8LSB((byte)i, 8);

        Assert.AreEqual(0, stream.WriteCount);
        Assert.AreEqual(0, stream.Length);
        Assert.AreEqual(8192, writer.Position);
        Assert.AreEqual(8192, writer.Length);

        writer.Flush();

        Assert.AreEqual(1, stream.WriteCount);
        Assert.AreEqual(1024, stream.Length);
    }

    [TestMethod]
    public void WritesAcrossMultipleOutputBuffers()
    {
        byte[] expected = new byte[8201];
        for (int i = 0; i < expected.Length; i++)
            expected[i] = (byte)i;

        using CountingWriteMemoryStream stream = new();
        using BitStreamWriter writer = new(stream, true);

        foreach (byte value in expected)
            writer.WriteUInt8LSB(value, 8);

        writer.Flush();

        Assert.AreEqual(3, stream.WriteCount);
        CollectionAssert.AreEqual(expected, stream.ToArray());
    }

    [TestMethod]
    public void SeekingFlushesPendingOutputBeforeInPlaceWrite()
    {
        byte[] expected = new byte[5000];
        using MemoryStream stream = new();
        using BitStreamWriter writer = new(stream, true);

        for (int i = 0; i < expected.Length; i++)
            writer.WriteUInt8LSB(0, 8);

        const int bitOffset = (100 * 8) + 3;
        Helpers.WriteBitsLSB(expected, bitOffset, 1, 1);
        writer.Position = bitOffset;
        writer.WriteBitLSB(true);
        writer.Flush();

        CollectionAssert.AreEqual(expected, stream.ToArray());
    }

    [TestMethod]
    public void RandomizedUnalignedWritesMatchAcrossOutputBuffers()
    {
        byte[] expected = new byte[8201];
        Random random = new(0xB175);
        using MemoryStream stream = new();
        using BitStreamWriter writer = new(stream, true);
        int bitOffset = 0;

        while (bitOffset < expected.Length * 8)
        {
            int bitCount = Math.Min(random.Next(1, 65), (expected.Length * 8) - bitOffset);
            ulong value = ((ulong)(uint)random.Next() << 32) | (uint)random.Next();

            if ((bitOffset & 1) == 0)
            {
                Helpers.WriteBitsLSB(expected, bitOffset, value, bitCount);
                writer.WriteUInt64LSB(value, bitCount);
            }
            else
            {
                Helpers.WriteBitsMSB(expected, bitOffset, value, bitCount);
                writer.WriteUInt64MSB(value, bitCount);
            }

            bitOffset += bitCount;
        }

        writer.Flush();

        Assert.AreEqual(bitOffset, writer.Position);
        CollectionAssert.AreEqual(expected, stream.ToArray());
    }

    [TestMethod]
    public void DisposeFlushesPendingOutputAndLeavesStreamOpen()
    {
        using MemoryStream stream = new();
        using (BitStreamWriter writer = new(stream, true))
            writer.WriteUInt16LSB(0xA5CD, 16);

        CollectionAssert.AreEqual(new byte[] { 0xCD, 0xA5 }, stream.ToArray());
        Assert.IsTrue(stream.CanWrite);
    }

    [TestMethod]
    public void ConstructorRejectsNonSeekableStream()
    {
        using NonSeekableWriteStream stream = new();

        Assert.ThrowsExactly<NotSupportedException>(() => new BitStreamWriter(stream));
    }

    [TestMethod]
    public void SupportsPositionsBeyondInt32ByteRange()
    {
        long bytePosition = (long)int.MaxValue + 42;
        using SparseStream stream = new(bytePosition + 1);
        using BitStreamWriter writer = new(stream, true);

        writer.Position = bytePosition << 3;

        Assert.AreEqual(bytePosition, stream.Position);
        Assert.AreEqual(bytePosition << 3, writer.Position);
    }

    [TestMethod]
    public void MembersThrowObjectDisposedExceptionAfterDispose()
    {
        using MemoryStream stream = new();
        BitStreamWriter writer = new(stream, true);
        writer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = writer.Position);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = writer.Length);
        Assert.ThrowsExactly<ObjectDisposedException>(() => writer.WriteBitLSB(true));
        Assert.ThrowsExactly<ObjectDisposedException>(() => writer.Flush());
        Assert.ThrowsExactly<ObjectDisposedException>(() => writer.Seek(0, SeekOrigin.Begin));
    }
}
