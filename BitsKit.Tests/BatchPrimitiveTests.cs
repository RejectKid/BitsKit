using System;
using BitsKit.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class BatchPrimitiveTests
{
    [TestMethod]
    public void ContiguousUInt32BatchesMatchReferenceOperations()
    {
        const int BitOffset = 3;
        const int BitCount = 12;
        const int ValueCount = 32;
        int byteCount = (BitOffset + BitCount * ValueCount + 7) / 8;
        var random = new Random(0xBA7C4);
        var source = new byte[byteCount];
        random.NextBytes(source);

        var actual = new uint[ValueCount];
        BitBatchPrimitives.ReadUInt32LSB(source, BitOffset, BitCount, actual);

        for (int i = 0; i < actual.Length; i++)
            Assert.AreEqual((uint)Helpers.ReadBitsLSB(source, BitOffset + i * BitCount, BitCount), actual[i]);

        var values = new uint[ValueCount];
        for (int i = 0; i < values.Length; i++)
            values[i] = unchecked((uint)random.NextInt64());

        var expected = (byte[])source.Clone();
        var destination = (byte[])source.Clone();
        for (int i = 0; i < values.Length; i++)
            Helpers.WriteBitsLSB(expected, BitOffset + i * BitCount, values[i], BitCount);

        BitBatchPrimitives.WriteUInt32LSB(destination, BitOffset, BitCount, values);
        CollectionAssert.AreEqual(expected, destination);
    }

    [TestMethod]
    public void StridedUInt64MsbBatchesMatchReferenceOperations()
    {
        const int BitOffset = 5;
        const int BitCount = 43;
        const int BitStride = 64;
        const int ValueCount = 20;
        int byteCount = (BitOffset + (ValueCount - 1) * BitStride + BitCount + 7) / 8;
        var random = new Random(0x57A1D);
        var source = new byte[byteCount];
        random.NextBytes(source);

        var actual = new ulong[ValueCount];
        BitBatchPrimitives.ReadUInt64MSB(source, BitOffset, BitCount, BitStride, actual);

        for (int i = 0; i < actual.Length; i++)
            Assert.AreEqual(Helpers.ReadBitsMSB(source, BitOffset + i * BitStride, BitCount), actual[i]);

        var values = new ulong[ValueCount];
        for (int i = 0; i < values.Length; i++)
            values[i] = unchecked((ulong)random.NextInt64());

        var expected = (byte[])source.Clone();
        var destination = (byte[])source.Clone();
        for (int i = 0; i < values.Length; i++)
            Helpers.WriteBitsMSB(expected, BitOffset + i * BitStride, values[i], BitCount);

        BitBatchPrimitives.WriteUInt64MSB(destination, BitOffset, BitCount, BitStride, values);
        CollectionAssert.AreEqual(expected, destination);
    }

    [TestMethod]
    public void SignedAndBooleanBatchesPreserveTheirSemantics()
    {
        const int ValueCount = 24;
        var signedSource = new byte[48];
        var booleanSource = new byte[12];
        var random = new Random(0x519E0);
        random.NextBytes(signedSource);
        random.NextBytes(booleanSource);

        var signed = new short[ValueCount];
        BitBatchPrimitives.ReadInt16LSB(signedSource, 2, 9, 15, signed);
        for (int i = 0; i < signed.Length; i++)
        {
            int raw = (int)Helpers.ReadBitsLSB(signedSource, 2 + i * 15, 9);
            short expected = (short)((raw << 23) >> 23);
            Assert.AreEqual(expected, signed[i]);
        }

        var bits = new bool[ValueCount];
        BitBatchPrimitives.ReadBitMSB(booleanSource, 4, 3, bits);
        for (int i = 0; i < bits.Length; i++)
            Assert.AreEqual(Helpers.ReadBitsMSB(booleanSource, 4 + i * 3, 1) != 0, bits[i]);

        for (int i = 0; i < bits.Length; i++)
            bits[i] = (i & 2) != 0;

        var expectedBits = (byte[])booleanSource.Clone();
        var actualBits = (byte[])booleanSource.Clone();
        for (int i = 0; i < bits.Length; i++)
            Helpers.WriteBitsMSB(expectedBits, 4 + i * 3, bits[i] ? 1UL : 0UL, 1);

        BitBatchPrimitives.WriteBitMSB(actualBits, 4, 3, bits);
        CollectionAssert.AreEqual(expectedBits, actualBits);
    }

    [TestMethod]
    public void BatchValidationRejectsInvalidRangesAndOverlappingWrites()
    {
        byte[] buffer = new byte[4];
        uint[] values = new uint[2];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BitBatchPrimitives.ReadUInt32LSB(buffer, -1, 8, values));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BitBatchPrimitives.ReadUInt32LSB(buffer, 0, 33, values));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BitBatchPrimitives.ReadUInt32LSB(buffer, 0, 8, 7, values));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BitBatchPrimitives.ReadUInt32LSB(buffer, 17, 8, 8, values));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            BitBatchPrimitives.WriteUInt32LSB(buffer, 0, 8, 7, values));
    }

    [TestMethod]
    public void EveryIntegralBatchTypeIsAvailableForBothBitOrders()
    {
        ReadOnlySpan<byte> source = [];
        Span<byte> destination = [];

        BitBatchPrimitives.ReadInt8LSB(source, 0, 0, Span<sbyte>.Empty);
        BitBatchPrimitives.ReadUInt8LSB(source, 0, 0, Span<byte>.Empty);
        BitBatchPrimitives.ReadInt16LSB(source, 0, 0, Span<short>.Empty);
        BitBatchPrimitives.ReadUInt16LSB(source, 0, 0, Span<ushort>.Empty);
        BitBatchPrimitives.ReadInt32LSB(source, 0, 0, Span<int>.Empty);
        BitBatchPrimitives.ReadUInt32LSB(source, 0, 0, Span<uint>.Empty);
        BitBatchPrimitives.ReadInt64LSB(source, 0, 0, Span<long>.Empty);
        BitBatchPrimitives.ReadUInt64LSB(source, 0, 0, Span<ulong>.Empty);
        BitBatchPrimitives.ReadIntPtrLSB(source, 0, 0, Span<nint>.Empty);
        BitBatchPrimitives.ReadUIntPtrLSB(source, 0, 0, Span<nuint>.Empty);

        BitBatchPrimitives.ReadInt8MSB(source, 0, 0, Span<sbyte>.Empty);
        BitBatchPrimitives.ReadUInt8MSB(source, 0, 0, Span<byte>.Empty);
        BitBatchPrimitives.ReadInt16MSB(source, 0, 0, Span<short>.Empty);
        BitBatchPrimitives.ReadUInt16MSB(source, 0, 0, Span<ushort>.Empty);
        BitBatchPrimitives.ReadInt32MSB(source, 0, 0, Span<int>.Empty);
        BitBatchPrimitives.ReadUInt32MSB(source, 0, 0, Span<uint>.Empty);
        BitBatchPrimitives.ReadInt64MSB(source, 0, 0, Span<long>.Empty);
        BitBatchPrimitives.ReadUInt64MSB(source, 0, 0, Span<ulong>.Empty);
        BitBatchPrimitives.ReadIntPtrMSB(source, 0, 0, Span<nint>.Empty);
        BitBatchPrimitives.ReadUIntPtrMSB(source, 0, 0, Span<nuint>.Empty);

        BitBatchPrimitives.WriteInt8LSB(destination, 0, 0, ReadOnlySpan<sbyte>.Empty);
        BitBatchPrimitives.WriteUInt8LSB(destination, 0, 0, ReadOnlySpan<byte>.Empty);
        BitBatchPrimitives.WriteInt16LSB(destination, 0, 0, ReadOnlySpan<short>.Empty);
        BitBatchPrimitives.WriteUInt16LSB(destination, 0, 0, ReadOnlySpan<ushort>.Empty);
        BitBatchPrimitives.WriteInt32LSB(destination, 0, 0, ReadOnlySpan<int>.Empty);
        BitBatchPrimitives.WriteUInt32LSB(destination, 0, 0, ReadOnlySpan<uint>.Empty);
        BitBatchPrimitives.WriteInt64LSB(destination, 0, 0, ReadOnlySpan<long>.Empty);
        BitBatchPrimitives.WriteUInt64LSB(destination, 0, 0, ReadOnlySpan<ulong>.Empty);
        BitBatchPrimitives.WriteIntPtrLSB(destination, 0, 0, ReadOnlySpan<nint>.Empty);
        BitBatchPrimitives.WriteUIntPtrLSB(destination, 0, 0, ReadOnlySpan<nuint>.Empty);

        BitBatchPrimitives.WriteInt8MSB(destination, 0, 0, ReadOnlySpan<sbyte>.Empty);
        BitBatchPrimitives.WriteUInt8MSB(destination, 0, 0, ReadOnlySpan<byte>.Empty);
        BitBatchPrimitives.WriteInt16MSB(destination, 0, 0, ReadOnlySpan<short>.Empty);
        BitBatchPrimitives.WriteUInt16MSB(destination, 0, 0, ReadOnlySpan<ushort>.Empty);
        BitBatchPrimitives.WriteInt32MSB(destination, 0, 0, ReadOnlySpan<int>.Empty);
        BitBatchPrimitives.WriteUInt32MSB(destination, 0, 0, ReadOnlySpan<uint>.Empty);
        BitBatchPrimitives.WriteInt64MSB(destination, 0, 0, ReadOnlySpan<long>.Empty);
        BitBatchPrimitives.WriteUInt64MSB(destination, 0, 0, ReadOnlySpan<ulong>.Empty);
        BitBatchPrimitives.WriteIntPtrMSB(destination, 0, 0, ReadOnlySpan<nint>.Empty);
        BitBatchPrimitives.WriteUIntPtrMSB(destination, 0, 0, ReadOnlySpan<nuint>.Empty);
    }

    [TestMethod]
    public void EveryIntegralBatchReaderMatchesScalarPrimitives()
    {
        const int BitOffset = 7;
        const int BitStride = 73;
        const int ValueCount = 8;
        var source = new byte[80];
        new Random(0xA11B47).NextBytes(source);

        var int8Lsb = new sbyte[ValueCount];
        var uint8Msb = new byte[ValueCount];
        var int16Msb = new short[ValueCount];
        var uint16Lsb = new ushort[ValueCount];
        var int32Lsb = new int[ValueCount];
        var uint32Msb = new uint[ValueCount];
        var int64Msb = new long[ValueCount];
        var uint64Lsb = new ulong[ValueCount];
        var intptrLsb = new nint[ValueCount];
        var uintptrMsb = new nuint[ValueCount];

        BitBatchPrimitives.ReadInt8LSB(source, BitOffset, 5, BitStride, int8Lsb);
        BitBatchPrimitives.ReadUInt8MSB(source, BitOffset, 7, BitStride, uint8Msb);
        BitBatchPrimitives.ReadInt16MSB(source, BitOffset, 13, BitStride, int16Msb);
        BitBatchPrimitives.ReadUInt16LSB(source, BitOffset, 15, BitStride, uint16Lsb);
        BitBatchPrimitives.ReadInt32LSB(source, BitOffset, 29, BitStride, int32Lsb);
        BitBatchPrimitives.ReadUInt32MSB(source, BitOffset, 31, BitStride, uint32Msb);
        BitBatchPrimitives.ReadInt64MSB(source, BitOffset, 57, BitStride, int64Msb);
        BitBatchPrimitives.ReadUInt64LSB(source, BitOffset, 61, BitStride, uint64Lsb);
        BitBatchPrimitives.ReadIntPtrLSB(source, BitOffset, 29, BitStride, intptrLsb);
        BitBatchPrimitives.ReadUIntPtrMSB(source, BitOffset, 29, BitStride, uintptrMsb);

        for (int i = 0; i < ValueCount; i++)
        {
            int offset = BitOffset + i * BitStride;
            Assert.AreEqual(BitPrimitives.ReadInt8LSB(source, offset, 5), int8Lsb[i]);
            Assert.AreEqual(BitPrimitives.ReadUInt8MSB(source, offset, 7), uint8Msb[i]);
            Assert.AreEqual(BitPrimitives.ReadInt16MSB(source, offset, 13), int16Msb[i]);
            Assert.AreEqual(BitPrimitives.ReadUInt16LSB(source, offset, 15), uint16Lsb[i]);
            Assert.AreEqual(BitPrimitives.ReadInt32LSB(source, offset, 29), int32Lsb[i]);
            Assert.AreEqual(BitPrimitives.ReadUInt32MSB(source, offset, 31), uint32Msb[i]);
            Assert.AreEqual(BitPrimitives.ReadInt64MSB(source, offset, 57), int64Msb[i]);
            Assert.AreEqual(BitPrimitives.ReadUInt64LSB(source, offset, 61), uint64Lsb[i]);
            Assert.AreEqual(BitPrimitives.ReadIntPtrLSB(source, offset, 29), intptrLsb[i]);
            Assert.AreEqual(BitPrimitives.ReadUIntPtrMSB(source, offset, 29), uintptrMsb[i]);
        }
    }
}
