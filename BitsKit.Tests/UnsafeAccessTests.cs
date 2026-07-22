using System;
using System.Linq;
using BitsKit.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class UnsafeAccessTests
{
    [TestMethod]
    public void UnsafePrimitivesMatchReferenceOperationsForValidPaddedBuffers()
    {
        var random = new Random(0x51A7E);
        int[] widths = [8, 16, 32, 64];

        foreach (int width in widths)
        {
            foreach (int bitOffset in Enumerable.Range(0, 8))
            {
                foreach (int bitCount in new[] { 1, Math.Max(1, width / 2), width })
                {
                    for (int iteration = 0; iteration < 50; iteration++)
                    {
                        var original = new byte[32];
                        random.NextBytes(original);

                        foreach (bool mostSignificant in new[] { false, true })
                        {
                            ulong expectedRead = mostSignificant
                                ? Helpers.ReadBitsMSB(original, bitOffset, bitCount)
                                : Helpers.ReadBitsLSB(original, bitOffset, bitCount);
                            var readBuffer = (byte[])original.Clone();
                            ulong actualRead = ReadUnsafe(
                                ref readBuffer[0],
                                bitOffset,
                                bitCount,
                                width,
                                mostSignificant);
                            Assert.AreEqual(expectedRead, actualRead);

                            ulong value = unchecked((ulong)random.NextInt64());
                            var expectedWrite = (byte[])original.Clone();
                            var actualWrite = (byte[])original.Clone();
                            if (mostSignificant)
                                Helpers.WriteBitsMSB(expectedWrite, bitOffset, value, bitCount);
                            else
                                Helpers.WriteBitsLSB(expectedWrite, bitOffset, value, bitCount);

                            WriteUnsafe(
                                ref actualWrite[0],
                                bitOffset,
                                value,
                                bitCount,
                                width,
                                mostSignificant);
                            CollectionAssert.AreEqual(expectedWrite, actualWrite);
                        }
                    }
                }
            }
        }
    }

    [TestMethod]
    public void UnsafeGeneratedMemoryAccessorsMatchReferenceOperations()
    {
        var random = new Random(0xB175);

        for (int iteration = 0; iteration < 1_000; iteration++)
        {
            var backing = new byte[16];
            var signedBacking = new byte[16];
            var booleanBacking = new byte[16];
            var alignedBacking = new byte[16];
            random.NextBytes(backing);
            random.NextBytes(signedBacking);
            random.NextBytes(booleanBacking);
            random.NextBytes(alignedBacking);

            var model = new UnsafeMemoryAccessorStruct
            {
                Backing = backing,
                SignedBacking = signedBacking,
                BooleanBacking = booleanBacking,
                AlignedBacking = alignedBacking
            };

            Assert.AreEqual((uint)Helpers.ReadBitsLSB(backing, 3, 20), model.Value);

            int signedValue = unchecked((int)Helpers.ReadBitsMSB(signedBacking, 5, 13));
            signedValue = (signedValue << 19) >> 19;
            Assert.AreEqual(signedValue, model.SignedValue);

            Assert.AreEqual(Helpers.ReadBitsLSB(booleanBacking, 6, 1) != 0, model.Flag);
            Assert.AreEqual((uint)Helpers.ReadBitsLSB(alignedBacking, 0, 32), model.AlignedValue);

            uint next = unchecked((uint)random.NextInt64());
            var expected = (byte[])backing.Clone();
            Helpers.WriteBitsLSB(expected, 3, next, 20);
            model.Value = next;
            CollectionAssert.AreEqual(expected, backing);

            bool nextFlag = (iteration & 1) != 0;
            var expectedBoolean = (byte[])booleanBacking.Clone();
            Helpers.WriteBitsLSB(expectedBoolean, 6, nextFlag ? 1UL : 0UL, 1);
            model.Flag = nextFlag;
            CollectionAssert.AreEqual(expectedBoolean, booleanBacking);
        }
    }

    [TestMethod]
    public void CheckedGeneratedAccessorsRemainCheckedByDefault()
    {
        var model = new OptimizedMemoryAccessorStruct { Backing = Array.Empty<byte>() };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = model.Value);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => model.Value = 1);
    }

    [TestMethod]
    public unsafe void UnsafeGenerationSupportsEveryByteAddressableBackingKind()
    {
        const uint Expected = 0xABCDE;

        var arrayModel = new UnsafeArrayAccessorStruct { Backing = new byte[16] };
        arrayModel.Value = Expected;
        Assert.AreEqual(Expected, arrayModel.Value);

        Span<byte> spanBuffer = stackalloc byte[16];
        var spanModel = new UnsafeSpanAccessorStruct { Backing = spanBuffer };
        spanModel.Value = Expected;
        Assert.AreEqual(Expected, spanModel.Value);

        var fixedModel = new UnsafeFixedAccessorStruct();
        fixedModel.Value = Expected;
        Assert.AreEqual(Expected, fixedModel.Value);

        var inlineModel = new UnsafeInlineArrayAccessorStruct();
        inlineModel.Value = Expected;
        Assert.AreEqual(Expected, inlineModel.Value);
    }

    private static ulong ReadUnsafe(
        ref byte source,
        int bitOffset,
        int bitCount,
        int width,
        bool mostSignificant)
    {
        return (width, mostSignificant) switch
        {
            (8, false) => UnsafeBitPrimitives.ReadUInt8LSB(ref source, bitOffset, bitCount),
            (8, true) => UnsafeBitPrimitives.ReadUInt8MSB(ref source, bitOffset, bitCount),
            (16, false) => UnsafeBitPrimitives.ReadUInt16LSB(ref source, bitOffset, bitCount),
            (16, true) => UnsafeBitPrimitives.ReadUInt16MSB(ref source, bitOffset, bitCount),
            (32, false) => UnsafeBitPrimitives.ReadUInt32LSB(ref source, bitOffset, bitCount),
            (32, true) => UnsafeBitPrimitives.ReadUInt32MSB(ref source, bitOffset, bitCount),
            (64, false) => UnsafeBitPrimitives.ReadUInt64LSB(ref source, bitOffset, bitCount),
            (64, true) => UnsafeBitPrimitives.ReadUInt64MSB(ref source, bitOffset, bitCount),
            _ => throw new ArgumentOutOfRangeException(nameof(width))
        };
    }

    private static void WriteUnsafe(
        ref byte destination,
        int bitOffset,
        ulong value,
        int bitCount,
        int width,
        bool mostSignificant)
    {
        switch (width, mostSignificant)
        {
            case (8, false):
                UnsafeBitPrimitives.WriteUInt8LSB(ref destination, bitOffset, (byte)value, bitCount);
                break;
            case (8, true):
                UnsafeBitPrimitives.WriteUInt8MSB(ref destination, bitOffset, (byte)value, bitCount);
                break;
            case (16, false):
                UnsafeBitPrimitives.WriteUInt16LSB(ref destination, bitOffset, (ushort)value, bitCount);
                break;
            case (16, true):
                UnsafeBitPrimitives.WriteUInt16MSB(ref destination, bitOffset, (ushort)value, bitCount);
                break;
            case (32, false):
                UnsafeBitPrimitives.WriteUInt32LSB(ref destination, bitOffset, (uint)value, bitCount);
                break;
            case (32, true):
                UnsafeBitPrimitives.WriteUInt32MSB(ref destination, bitOffset, (uint)value, bitCount);
                break;
            case (64, false):
                UnsafeBitPrimitives.WriteUInt64LSB(ref destination, bitOffset, value, bitCount);
                break;
            case (64, true):
                UnsafeBitPrimitives.WriteUInt64MSB(ref destination, bitOffset, value, bitCount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(width));
        }
    }
}
