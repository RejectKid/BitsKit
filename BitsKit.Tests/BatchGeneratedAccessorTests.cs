using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class BatchGeneratedAccessorTests
{
    [TestMethod]
    public void GeneratedStridedReadersUseDeclaredFieldLayouts()
    {
        const int RecordStride = 24;
        const int RecordCount = 32;
        var random = new Random(0x6E4A7);
        var source = new byte[RecordCount * 3];
        random.NextBytes(source);

        var values = new ushort[RecordCount];
        var flags = new bool[RecordCount];
        var kinds = new TestEnum[RecordCount];
        var bigEndianValues = new uint[RecordCount];

        BatchAccessorStruct.ReadValueBatch(source, RecordStride, values);
        BatchAccessorStruct.ReadFlagBatch(source, RecordStride, flags);
        BatchAccessorStruct.ReadKindBatch(source, RecordStride, kinds);
        BatchAccessorStruct.ReadBigEndianValueBatch(source, RecordStride, bigEndianValues);

        for (int i = 0; i < RecordCount; i++)
        {
            int recordOffset = i * RecordStride;
            Assert.AreEqual((ushort)Helpers.ReadBitsLSB(source, recordOffset + 3, 12), values[i]);
            Assert.AreEqual(Helpers.ReadBitsLSB(source, recordOffset + 15, 1) != 0, flags[i]);
            int rawKind = (int)Helpers.ReadBitsLSB(source, recordOffset + 16, 2);
            Assert.AreEqual((TestEnum)((rawKind << 30) >> 30), kinds[i]);
            Assert.AreEqual((uint)Helpers.ReadBitsMSB(source, recordOffset + 5, 11), bigEndianValues[i]);
        }
    }

    [TestMethod]
    public void GeneratedStridedWritersUseDeclaredFieldLayouts()
    {
        const int RecordStride = 24;
        const int RecordCount = 32;
        var random = new Random(0xBA7C8);
        var original = new byte[RecordCount * 3];
        random.NextBytes(original);

        var values = new ushort[RecordCount];
        var flags = new bool[RecordCount];
        var kinds = new TestEnum[RecordCount];
        for (int i = 0; i < RecordCount; i++)
        {
            values[i] = unchecked((ushort)random.Next());
            flags[i] = (i & 1) != 0;
            kinds[i] = (TestEnum)(i & 3);
        }

        var expected = (byte[])original.Clone();
        for (int i = 0; i < RecordCount; i++)
        {
            int recordOffset = i * RecordStride;
            Helpers.WriteBitsLSB(expected, recordOffset + 3, values[i], 12);
            Helpers.WriteBitsLSB(expected, recordOffset + 15, flags[i] ? 1UL : 0UL, 1);
            Helpers.WriteBitsLSB(expected, recordOffset + 16, (ulong)kinds[i], 2);
        }

        var actual = (byte[])original.Clone();
        BatchAccessorStruct.WriteValueBatch(actual, RecordStride, values);
        BatchAccessorStruct.WriteFlagBatch(actual, RecordStride, flags);
        BatchAccessorStruct.WriteKindBatch(actual, RecordStride, kinds);

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GeneratedPackedOverloadsAdvanceByFieldWidth()
    {
        const int ValueCount = 16;
        var values = new ushort[ValueCount];
        for (int i = 0; i < values.Length; i++)
            values[i] = (ushort)(i * 71);

        int bitLength = 3 + values.Length * 12;
        var storage = new byte[(bitLength + 7) / 8];

        BatchAccessorStruct.WriteValueBatch(storage, values);

        var actual = new ushort[ValueCount];
        BatchAccessorStruct.ReadValueBatch(storage, actual);
        CollectionAssert.AreEqual(values, actual);
    }
}
