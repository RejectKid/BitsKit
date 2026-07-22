using System;
using System.Collections.Immutable;
using System.Linq;
using BitsKit.BitFields;
using BitsKit.Generator;
using BitsKit.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class GeneratorTests
{
    [TestMethod]
    public void SetterTests()
    {
        const int Expected = 0b1111;

        SequentialBitFieldStruct sequentialObj = new()
        {
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, sequentialObj.BackingField00, "SequentialBitFieldStruct.BackingField00 != Expected");
        Assert.AreEqual(0, sequentialObj.BackingField01, "SequentialBitFieldStruct.BackingField01 != 0");

        ExplicitBitFieldStruct explicitObj = new()
        {
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, explicitObj.BackingField00, "ExplicitBitFieldStruct.BackingField00 != Expected");
        Assert.AreEqual((ulong)Expected, explicitObj.BackingField01, "ExplicitBitFieldStruct.BackingField00 != ExplicitBitFieldStruct.BackingField01");

        BitFieldRecordStruct recordObj = new()
        {
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, recordObj.BackingField00, "BitFieldRecordStruct.BackingField00 != Expected");
        Assert.AreEqual(0, recordObj.BackingField01, "BitFieldRecordStruct.BackingField01 != 0");

        BitFieldMemoryStruct memoryObj = new()
        {
            BackingField00 = new byte[4],
            BackingField01 = new byte[4],
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, memoryObj.IntValue00, "BitFieldMemoryStruct.IntValue00 != Expected");
        Assert.AreEqual(0, memoryObj.IntValue01, "BitFieldMemoryStruct.IntValue01 != 0");

        BitFieldRefStruct refStructObj = new()
        {
            BackingField00 = new byte[4],
            BackingField01 = new byte[4],
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, refStructObj.IntValue00, "BitFieldRefStruct.IntValue00 != Expected");
        Assert.AreEqual(0, refStructObj.IntValue01, "BitFieldRefStruct.IntValue01 != 0");


        BitFieldFixedStruct fixedObj = new()
        {
            Generated01 = 0b11,
            Generated02 = 0b11,
        };

        Assert.AreEqual(Expected, fixedObj.IntValue00, "BitFieldFixedStruct.IntValue00 != Expected");
        Assert.AreEqual(0, fixedObj.BackingField01, "BitFieldFixedStruct.BackingField01 != 0");

        EnumBitFieldStruct enumObj = new()
        {
            Generated01 = (TestEnum)0b111,
            Generated02 = (TestEnum)0b111,
        };

        Assert.AreEqual((uint)Expected, enumObj.BackingField00, "EnumBitFieldStruct.BackingField00 != Expected");
        Assert.AreEqual(0u, enumObj.BackingField01, "EnumBitFieldStruct.BackingField01 != 0");

#if NET8_0_OR_GREATER
        InlineArrayStruct inlineArrayObj = new()
        {
            Generated01 = 0b11,
            Generated02 = 0b11,
            // padding : 27
            Generated03 = 0b11 // boundary straddling test
        };

        Assert.AreEqual(Expected | 0x80000000, (uint)inlineArrayObj[0], "InlineArrayStruct[0] != (Expected | 0x80000000)");
        Assert.AreEqual(1, inlineArrayObj[1], "InlineArrayStruct[1] != 1");
#endif
    }

    [TestMethod]
    public void GetterTests()
    {
        const int Input = 0b101110;
        const int Expected01 = -0b10; // negative as sign extended
        const int Expected02 = -0b01; // negative as sign extended

        SequentialBitFieldStruct sequentialObj = new()
        {
            BackingField00 = Input
        };

        Assert.AreEqual(Expected01, sequentialObj.Generated01, "SequentialBitFieldStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, sequentialObj.Generated02, "SequentialBitFieldStruct.Generated02 != Expected02");

        ExplicitBitFieldStruct explicitObj = new()
        {
            BackingField00 = Input
        };

        Assert.AreEqual(Expected01, explicitObj.Generated01, "ExplicitBitFieldStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, explicitObj.Generated02, "ExplicitBitFieldStruct.Generated02 != Expected02");

        BitFieldRecordStruct recordObj = new()
        {
            BackingField00 = Input
        };

        Assert.AreEqual(Expected01, recordObj.Generated01, "BitFieldRecordStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, recordObj.Generated02, "BitFieldRecordStruct.Generated02 != Expected02");

        BitFieldMemoryStruct memoryObj = new()
        {
            BackingField00 = BitConverter.GetBytes(Input)
        };

        Assert.AreEqual(Expected01, memoryObj.Generated01, "BitFieldMemoryStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, memoryObj.Generated02, "BitFieldMemoryStruct.Generated02 != Expected02");

        BitFieldRefStruct refStructObj = new()
        {
            BackingField00 = BitConverter.GetBytes(Input)
        };

        Assert.AreEqual(Expected01, refStructObj.Generated01, "BitFieldRefStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, refStructObj.Generated02, "BitFieldRefStruct.Generated02 != Expected02");


        BitFieldFixedStruct fixedObj = new()
        {
            IntValue00 = Input
        };

        Assert.AreEqual(Expected01, fixedObj.Generated01, "BitFieldFixedStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, fixedObj.Generated02, "BitFieldFixedStruct.Generated02 != Expected02");

        EnumBitFieldStruct enumObj = new()
        {
            BackingField00 = Input
        };

        Assert.AreEqual(TestEnum.B, enumObj.Generated01, "EnumBitFieldStruct.Generated01 != B");
        Assert.AreEqual(TestEnum.A | TestEnum.B, enumObj.Generated02, "EnumBitFieldStruct.Generated02 != (A | B)");

#if NET8_0_OR_GREATER
        InlineArrayStruct inlineArrayObj = new();
        inlineArrayObj[0] = Input;

        Assert.AreEqual(Expected01, inlineArrayObj.Generated01, "InlineArrayStruct.Generated01 != Expected01");
        Assert.AreEqual(Expected02, inlineArrayObj.Generated02, "InlineArrayStruct.Generated02 != Expected02");
        Assert.AreEqual(0, inlineArrayObj.Generated03, "InlineArrayStruct.Generated03 != 0");
#endif
    }

    [TestMethod]
    public void PaddingFieldTest()
    {
        const uint ExpectedA = 0b110010110011;
        const uint ExpectedB = 0b001101001100;

        // set the fields to test the setters
        PaddingFieldStruct obj = new()
        {
            Generated00 = 0b11,
            //          0b00, // 2 padding bits
            Generated01 = 0b11,
            //          0b1,  // 1 padding bit
            Generated02 = true,
            //          0b00, // 2 padding bits
            Generated03 = TestEnum.A | TestEnum.B,
        };

        Assert.AreEqual(ExpectedA, obj.BackingField00);

        // fill the backing field to test setters
        obj.BackingField00 = 0b111111111111;

        obj.Generated00 = 0b00;
        obj.Generated01 = 0b00;
        obj.Generated02 = false;
        obj.Generated03 = 0;

        Assert.AreEqual(ExpectedB, obj.BackingField00);
    }

    [TestMethod]
    public void EnumFieldTest()
    {
        const TestEnum Expected = TestEnum.A | TestEnum.B;

        // set the fields to test the setters
        EnumBitFieldStruct obj = new()
        {
            Generated01 = TestEnum.A | TestEnum.B | TestEnum.C
        };

        Assert.AreEqual(Expected, obj.Generated01);

        obj.BackingField00 = 0b11111111;

        Assert.AreEqual(Expected, obj.Generated01);
        Assert.AreEqual(Expected, obj.Generated02);
    }

    [TestMethod]
    public void OptimizedIntegralAccessorsMatchBitPrimitives()
    {
        Random random = new(0xB17F13D);

        for (int i = 0; i < 1000; i++)
        {
            var actual = new OptimizedIntegralAccessorStruct
            {
                ByteBacking = (byte)random.Next(),
                ReversedByteBacking = (byte)random.Next(),
                ReversedFullByteBacking = (byte)random.Next(),
                ShortBacking = (short)random.Next(),
                ReversedShortBacking = (short)random.Next(),
                IntBacking = random.Next(),
                ReversedIntBacking = random.Next(),
                UInt64Backing = ((ulong)(uint)random.Next() << 32) | (uint)random.Next(),
                ReversedUInt64Backing = ((ulong)(uint)random.Next() << 32) | (uint)random.Next(),
                ReversedFullUInt64Backing = ((ulong)(uint)random.Next() << 32) | (uint)random.Next(),
                BooleanBacking = (uint)random.Next(),
                SignedBooleanBacking = random.Next(),
                ReversedSignedBooleanBacking = random.Next(),
                ReversedBooleanBacking = (uint)random.Next(),
                EnumBacking = (uint)random.Next(),
                ReversedEnumBacking = (uint)random.Next()
            };

            Assert.AreEqual(BitPrimitives.ReadUInt8LSB(actual.ByteBacking, 2, 5), actual.ByteValue);
            Assert.AreEqual(BitPrimitives.ReadUInt8MSB(actual.ReversedByteBacking, 2, 5), actual.ReversedByteValue);
            Assert.AreEqual(BitPrimitives.ReadUInt8MSB(actual.ReversedFullByteBacking, 0, 8), actual.ReversedFullByteValue);
            Assert.AreEqual(BitPrimitives.ReadInt16LSB(actual.ShortBacking, 3, 9), actual.ShortValue);
            Assert.AreEqual(BitPrimitives.ReadInt16MSB(actual.ReversedShortBacking, 3, 9), actual.ReversedShortValue);
            Assert.AreEqual(BitPrimitives.ReadInt32LSB(actual.IntBacking, 5, 11), actual.IntValue);
            Assert.AreEqual(BitPrimitives.ReadInt32MSB(actual.ReversedIntBacking, 5, 11), actual.ReversedIntValue);
            Assert.AreEqual(BitPrimitives.ReadUInt64LSB(actual.UInt64Backing, 0, 64), actual.FullUInt64Value);
            Assert.AreEqual(BitPrimitives.ReadUInt64MSB(actual.ReversedUInt64Backing, 7, 43), actual.ReversedUInt64Value);
            Assert.AreEqual(BitPrimitives.ReadUInt64MSB(actual.ReversedFullUInt64Backing, 0, 64), actual.ReversedFullUInt64Value);
            Assert.AreEqual(BitPrimitives.ReadUInt32LSB(actual.BooleanBacking, 5, 1) == 1, actual.Flag);
            Assert.AreEqual((actual.SignedBooleanBacking & (1 << 5)) != 0, actual.SignedFlag);
            Assert.AreEqual(BitPrimitives.ReadInt32MSB(actual.ReversedSignedBooleanBacking, 5, 1) != 0, actual.ReversedSignedFlag);
            Assert.AreEqual(BitPrimitives.ReadUInt32MSB(actual.ReversedBooleanBacking, 5, 1) != 0, actual.ReversedFlag);
            Assert.AreEqual((TestEnum)BitPrimitives.ReadUInt32LSB(actual.EnumBacking, 5, 2), actual.EnumValue);
            Assert.AreEqual((TestEnum)BitPrimitives.ReadUInt32MSB(actual.ReversedEnumBacking, 5, 2), actual.ReversedEnumValue);

            byte byteValue = (byte)random.Next();
            byte expectedByte = actual.ByteBacking;
            BitPrimitives.WriteUInt8LSB(ref expectedByte, 2, byteValue, 5);
            actual.ByteValue = byteValue;
            Assert.AreEqual(expectedByte, actual.ByteBacking);

            byte expectedReversedByte = actual.ReversedByteBacking;
            BitPrimitives.WriteUInt8MSB(ref expectedReversedByte, 2, byteValue, 5);
            actual.ReversedByteValue = byteValue;
            Assert.AreEqual(expectedReversedByte, actual.ReversedByteBacking);

            byte fullByteValue = (byte)random.Next();
            byte expectedReversedFullByte = actual.ReversedFullByteBacking;
            BitPrimitives.WriteUInt8MSB(ref expectedReversedFullByte, 0, fullByteValue, 8);
            actual.ReversedFullByteValue = fullByteValue;
            Assert.AreEqual(expectedReversedFullByte, actual.ReversedFullByteBacking);

            short shortValue = (short)random.Next();
            short expectedShort = actual.ShortBacking;
            BitPrimitives.WriteInt16LSB(ref expectedShort, 3, shortValue, 9);
            actual.ShortValue = shortValue;
            Assert.AreEqual(expectedShort, actual.ShortBacking);

            short expectedReversedShort = actual.ReversedShortBacking;
            BitPrimitives.WriteInt16MSB(ref expectedReversedShort, 3, shortValue, 9);
            actual.ReversedShortValue = shortValue;
            Assert.AreEqual(expectedReversedShort, actual.ReversedShortBacking);

            int intValue = random.Next();
            int expectedInt = actual.IntBacking;
            BitPrimitives.WriteInt32LSB(ref expectedInt, 5, intValue, 11);
            actual.IntValue = intValue;
            Assert.AreEqual(expectedInt, actual.IntBacking);

            int reversedIntValue = random.Next();
            int expectedReversedInt = actual.ReversedIntBacking;
            BitPrimitives.WriteInt32MSB(ref expectedReversedInt, 5, reversedIntValue, 11);
            actual.ReversedIntValue = reversedIntValue;
            Assert.AreEqual(expectedReversedInt, actual.ReversedIntBacking);

            ulong fullUInt64Value = ((ulong)(uint)random.Next() << 32) | (uint)random.Next();
            actual.FullUInt64Value = fullUInt64Value;
            Assert.AreEqual(fullUInt64Value, actual.UInt64Backing);

            ulong reversedUInt64Value = ((ulong)(uint)random.Next() << 32) | (uint)random.Next();
            ulong expectedReversedUInt64 = actual.ReversedUInt64Backing;
            BitPrimitives.WriteUInt64MSB(ref expectedReversedUInt64, 7, reversedUInt64Value, 43);
            actual.ReversedUInt64Value = reversedUInt64Value;
            Assert.AreEqual(expectedReversedUInt64, actual.ReversedUInt64Backing);

            ulong reversedFullUInt64Value = ((ulong)(uint)random.Next() << 32) | (uint)random.Next();
            ulong expectedReversedFullUInt64 = actual.ReversedFullUInt64Backing;
            BitPrimitives.WriteUInt64MSB(ref expectedReversedFullUInt64, 0, reversedFullUInt64Value, 64);
            actual.ReversedFullUInt64Value = reversedFullUInt64Value;
            Assert.AreEqual(expectedReversedFullUInt64, actual.ReversedFullUInt64Backing);

            bool flag = random.Next(2) != 0;
            uint expectedBoolean = actual.BooleanBacking;
            BitPrimitives.WriteUInt32LSB(ref expectedBoolean, 5, flag ? 1u : 0u, 1);
            actual.Flag = flag;
            Assert.AreEqual(expectedBoolean, actual.BooleanBacking);

            int expectedSignedBoolean = actual.SignedBooleanBacking;
            BitPrimitives.WriteInt32LSB(ref expectedSignedBoolean, 5, flag ? 1 : 0, 1);
            actual.SignedFlag = flag;
            Assert.AreEqual(expectedSignedBoolean, actual.SignedBooleanBacking);

            int expectedReversedSignedBoolean = actual.ReversedSignedBooleanBacking;
            BitPrimitives.WriteInt32MSB(ref expectedReversedSignedBoolean, 5, flag ? 1 : 0, 1);
            actual.ReversedSignedFlag = flag;
            Assert.AreEqual(expectedReversedSignedBoolean, actual.ReversedSignedBooleanBacking);

            uint expectedReversedBoolean = actual.ReversedBooleanBacking;
            BitPrimitives.WriteUInt32MSB(ref expectedReversedBoolean, 5, flag ? 1u : 0u, 1);
            actual.ReversedFlag = flag;
            Assert.AreEqual(expectedReversedBoolean, actual.ReversedBooleanBacking);

            TestEnum enumValue = (TestEnum)random.Next(4);
            uint expectedEnum = actual.EnumBacking;
            BitPrimitives.WriteUInt32LSB(ref expectedEnum, 5, (uint)enumValue, 2);
            actual.EnumValue = enumValue;
            Assert.AreEqual(expectedEnum, actual.EnumBacking);

            uint expectedReversedEnum = actual.ReversedEnumBacking;
            BitPrimitives.WriteUInt32MSB(ref expectedReversedEnum, 5, (uint)enumValue, 2);
            actual.ReversedEnumValue = enumValue;
            Assert.AreEqual(expectedReversedEnum, actual.ReversedEnumBacking);
        }
    }

    [TestMethod]
    public void OptimizedStorageGettersMatchBitPrimitives()
    {
        Random random = new(0x51A6E);

        for (int i = 0; i < 1000; i++)
        {
            byte[] bytes = new byte[4];
            random.NextBytes(bytes);
            var memory = new OptimizedMemoryAccessorStruct { Backing = bytes };

            Assert.AreEqual(BitPrimitives.ReadUInt32LSB(bytes, 5, 11), memory.Value);

            uint nextValue = (uint)random.Next(1 << 11);
            byte[] expectedBytes = (byte[])bytes.Clone();
            BitPrimitives.WriteUInt32LSB(expectedBytes, 5, nextValue, 11);
            memory.Value = nextValue;
            CollectionAssert.AreEqual(expectedBytes, bytes);

#if NET8_0_OR_GREATER
            var inline = new OptimizedInlineArrayAccessorStruct();
            bytes.CopyTo((Span<byte>)inline);
            Assert.AreEqual(BitPrimitives.ReadUInt32LSB(bytes, 5, 11), inline.Value);
#endif
        }
    }

    [TestMethod]
    public void ReadOnlyMemberTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public ref partial struct BitFieldReadOnly
        {
            [BitField(""Generated00"", 2)]
            public readonly int BackingField00;

            [BitField(""Generated10"", 2, BitFieldType.Int32)]
            public ReadOnlySpan<byte> BackingField01;

            [BitField(""Generated20"", 2, BitFieldType.Int32)]
            public ReadOnlyMemory<byte> BackingField02;
        }
        ";

        string expected = @"
        partial struct BitFieldReadOnly
        {
            public  Int32 Generated00 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30));
            }

            public  Int32 Generated10 
            {
                get => BitPrimitives.ReadInt32LSB(BackingField01, 0, 2);
            }

            public  Int32 Generated20 
            {
                get => BitPrimitives.ReadInt32LSB(BackingField02.Span, 0, 2);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

    [TestMethod]
    public void TypeModifierTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public readonly ref partial struct BitFieldReadOnly
        {
            [BitField(""Generated00"", 2)]
            public readonly int BackingField00;

            [BitField(""Generated10"", 2, BitFieldType.Int32)]
            public readonly ReadOnlySpan<byte> BackingField01;

            [BitField(""Generated20"", 2, BitFieldType.Int32)]
            public readonly ReadOnlyMemory<byte> BackingField02;
        }
        ";

        string expected = @"
        partial struct BitFieldReadOnly
        {
            public  Int32 Generated00 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30));
            }

            public  Int32 Generated10 
            {
                get => BitPrimitives.ReadInt32LSB(BackingField01, 0, 2);
            }

            public  Int32 Generated20 
            {
                get => BitPrimitives.ReadInt32LSB(BackingField02.Span, 0, 2);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

    [TestMethod]
    public void MemberAttributeTestNet60()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public unsafe partial class BitFieldGeneratorTest
        {
            [BitField(""Generated01"", 2, Modifiers = BitFieldModifiers.Public)]
            [BitField(2)]
            [BitField(""Generated02"", 2, Modifiers = BitFieldModifiers.Private)]
            [BitField(""Generated03"", 2, Modifiers = BitFieldModifiers.Internal)]
            [BitField(""Generated04"", 2, Modifiers = BitFieldModifiers.ReadOnly)]
            [BitField(""Generated05"", 2, Modifiers = BitFieldModifiers.InitOnly)]
            [BitField(""Generated06"", 2, ReverseBitOrder = true)]
            [BitField(""Generated07"", 2, Modifiers = BitFieldModifiers.Protected)]
            [BitField(""Generated08"", 2, Modifiers = BitFieldModifiers.ProtectedInternal)]
            [BitField(""Generated09"", 2, Modifiers = BitFieldModifiers.PrivateProtected)]
            public int BackingField00;

            [BitField(""Generated10"", 2, BitFieldType.SByte)]
            [BitField(2)]
            [BitField(""Generated11"", 2, BitFieldType.Int16)]
            [BitField(""Generated12"", 2, BitFieldType.Int32)]
            [BitField(""Generated13"", 2, BitFieldType.Int64)]
            [BitField(""Generated14"", 2, BitFieldType.Byte)]
            [BitField(""Generated15"", 2, BitFieldType.UInt16)]
            [BitField(""Generated16"", 2, BitFieldType.UInt32)]
            [BitField(""Generated17"", 2, BitFieldType.UInt64)]
            [BitField(""Generated18"", 2, BitFieldType.IntPtr)]
            [BitField(""Generated19"", 2, BitFieldType.UIntPtr)]
            public byte[] BackingField01;
        }
        ";

        string expected = @"
        partial class BitFieldGeneratorTest
        {
            public  Int32 Generated01 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x3U) | ((unchecked((UInt32)(value)) << 0) & 0x3U)));
            }

            private  Int32 Generated02 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 26)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x30U) | ((unchecked((UInt32)(value)) << 4) & 0x30U)));
            }

            internal  Int32 Generated03 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 24)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC0U) | ((unchecked((UInt32)(value)) << 6) & 0xC0U)));
            }

            public  Int32 Generated04 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 22)) >> 30));
            }

            public  Int32 Generated05 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 20)) >> 30));
                init => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC00U) | ((unchecked((UInt32)(value)) << 10) & 0xC00U)));
            }

            public  Int32 Generated06 
            {
                get => unchecked((Int32)((unchecked((Int32)((BinaryPrimitives.ReverseEndianness(unchecked((UInt32)BackingField00)) << 12) >> 30)) << 30) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC00U) | BinaryPrimitives.ReverseEndianness((unchecked((UInt32)(value)) & 0x3U) << 18)));
            }

            protected  Int32 Generated07 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 16)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC000U) | ((unchecked((UInt32)(value)) << 14) & 0xC000U)));
            }

            protected internal  Int32 Generated08 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 14)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x30000U) | ((unchecked((UInt32)(value)) << 16) & 0x30000U)));
            }

            private protected  Int32 Generated09 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 12)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC0000U) | ((unchecked((UInt32)(value)) << 18) & 0xC0000U)));
            }

            public  SByte Generated10 
            {
                get => BitPrimitives.ReadInt8LSB(BackingField01, 0, 2);
                set => BitPrimitives.WriteInt8LSB(BackingField01, 0, value, 2);
            }

            public  Int16 Generated11 
            {
                get => BitPrimitives.ReadInt16LSB(BackingField01, 4, 2);
                set => BitPrimitives.WriteInt16LSB(BackingField01, 4, value, 2);
            }

            public  Int32 Generated12 
            {
                get => BitPrimitives.ReadInt32LSB(BackingField01, 6, 2);
                set => BitPrimitives.WriteInt32LSB(BackingField01, 6, value, 2);
            }

            public  Int64 Generated13 
            {
                get => BitPrimitives.ReadInt64LSB(BackingField01, 8, 2);
                set => BitPrimitives.WriteInt64LSB(BackingField01, 8, value, 2);
            }

            public  Byte Generated14 
            {
                get => BitPrimitives.ReadUInt8LSB(BackingField01, 10, 2);
                set => BitPrimitives.WriteUInt8LSB(BackingField01, 10, value, 2);
            }

            public  UInt16 Generated15 
            {
                get => BitPrimitives.ReadUInt16LSB(BackingField01, 12, 2);
                set => BitPrimitives.WriteUInt16LSB(BackingField01, 12, value, 2);
            }

            public  UInt32 Generated16 
            {
                get => BitPrimitives.ReadUInt32LSB(BackingField01, 14, 2);
                set => BitPrimitives.WriteUInt32LSB(BackingField01, 14, value, 2);
            }

            public  UInt64 Generated17 
            {
                get => BitPrimitives.ReadUInt64LSB(BackingField01, 16, 2);
                set => BitPrimitives.WriteUInt64LSB(BackingField01, 16, value, 2);
            }

            public  IntPtr Generated18 
            {
                get => BitPrimitives.ReadIntPtrLSB(BackingField01, 18, 2);
                set => BitPrimitives.WriteIntPtrLSB(BackingField01, 18, value, 2);
            }

            public  UIntPtr Generated19 
            {
                get => BitPrimitives.ReadUIntPtrLSB(BackingField01, 20, 2);
                set => BitPrimitives.WriteUIntPtrLSB(BackingField01, 20, value, 2);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

    [TestMethod]
    public void MemberAttributeTestNet70()
    {
#if NET7_0_OR_GREATER
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public unsafe partial class BitFieldGeneratorTest
        {
            [BitField(""Generated01"", 2, Modifiers = BitFieldModifiers.Public)]
            [BitField(2)]
            [BitField(""Generated02"", 2, Modifiers = BitFieldModifiers.Private)]
            [BitField(""Generated03"", 2, Modifiers = BitFieldModifiers.Internal)]
            [BitField(""Generated04"", 2, Modifiers = BitFieldModifiers.ReadOnly)]
            [BitField(""Generated05"", 2, Modifiers = BitFieldModifiers.InitOnly)]
            [BitField(""Generated06"", 2, ReverseBitOrder = true)]
            [BitField(""Generated07"", 2, Modifiers = BitFieldModifiers.Required)]
            [BitField(""Generated08"", 2, Modifiers = BitFieldModifiers.Protected)]
            [BitField(""Generated09"", 2, Modifiers = BitFieldModifiers.ProtectedInternal)]
            [BitField(""Generated0A"", 2, Modifiers = BitFieldModifiers.PrivateProtected)]
            public int BackingField00;
        }
        ";

        string expected = @"
        partial class BitFieldGeneratorTest
        {
            public  Int32 Generated01 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x3U) | ((unchecked((UInt32)(value)) << 0) & 0x3U)));
            }

            private  Int32 Generated02 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 26)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x30U) | ((unchecked((UInt32)(value)) << 4) & 0x30U)));
            }

            internal  Int32 Generated03 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 24)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC0U) | ((unchecked((UInt32)(value)) << 6) & 0xC0U)));
            }

            public  Int32 Generated04 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 22)) >> 30));
            }

            public  Int32 Generated05 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 20)) >> 30));
                init => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC00U) | ((unchecked((UInt32)(value)) << 10) & 0xC00U)));
            }

            public  Int32 Generated06 
            {
                get => unchecked((Int32)((unchecked((Int32)((BinaryPrimitives.ReverseEndianness(unchecked((UInt32)BackingField00)) << 12) >> 30)) << 30) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC00U) | BinaryPrimitives.ReverseEndianness((unchecked((UInt32)(value)) & 0x3U) << 18)));
            }

            public required Int32 Generated07 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 16)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC000U) | ((unchecked((UInt32)(value)) << 14) & 0xC000U)));
            }

            protected Int32 Generated08 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 14)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x30000U) | ((unchecked((UInt32)(value)) << 16) & 0x30000U)));
            }

            protected internal Int32 Generated09 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 12)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xC0000U) | ((unchecked((UInt32)(value)) << 18) & 0xC0000U)));
            }

            private protected Int32 Generated0A 
            {
                get => unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 10)) >> 30));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x300000U) | ((unchecked((UInt32)(value)) << 20) & 0x300000U)));
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
#endif
    }

    [TestMethod]
    public void BooleanMemberTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public unsafe ref partial struct BooleanGeneratorTest
        {
            [BooleanField(""Generated01"")]
            public int BackingField00;

            [BooleanField(""Generated10"")]
            public Span<byte> BackingField01;

            [BooleanField(""Generated20"")]
            public ReadOnlySpan<byte> BackingField02;

            [BooleanField(""Generated30"")]
            public fixed byte BackingField03[4];
        }
        ";

        string expected = @"
        partial struct BooleanGeneratorTest
        {
            public  System.Boolean Generated01 
            {
                readonly get => (unchecked((UInt32)BackingField00) & 0x1U) != 0;
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x1U) | ((unchecked((UInt32)(value ? 1 : 0)) << 0) & 0x1U)));
            }

            public  System.Boolean Generated10 
            {
                readonly get => BitPrimitives.ReadBitLSB(BackingField01, 0);
                set => BitPrimitives.WriteBitLSB(BackingField01, 0, value);
            }

            public  System.Boolean Generated20 
            {
                get => BitPrimitives.ReadBitLSB(BackingField02, 0);
            }

            public unsafe  System.Boolean Generated30 
            {
                get => BitPrimitives.ReadBitLSB(MemoryMarshal.CreateReadOnlySpan(ref BackingField03[0], 4), 0);
                set => BitPrimitives.WriteBitLSB(MemoryMarshal.CreateSpan(ref BackingField03[0], 4), 0, value);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

    [TestMethod]
    public void EnumMemberTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public unsafe ref partial struct EnumGeneratorTest
        {
            [EnumField(""Generated00"", 2, typeof(BitsKit.Tests.TestEnum))]
            [EnumField(""Generated01"", 2, typeof(BitsKit.Tests.TestEnum))]
            public int BackingField00;

            [EnumField(""Generated10"", 2, typeof(BitsKit.Tests.TestEnum))]
            [EnumField(""Generated11"", 2, typeof(BitsKit.Tests.TestEnum))]
            public Span<byte> BackingField01;

            [EnumField(""Generated20"", 2, typeof(BitsKit.Tests.TestEnum))]
            [EnumField(""Generated21"", 2, typeof(BitsKit.Tests.TestEnum))]
            public ReadOnlySpan<byte> BackingField02;

            [EnumField(""Generated30"", 2, typeof(BitsKit.Tests.TestEnum))]
            [EnumField(""Generated31"", 2, typeof(BitsKit.Tests.TestEnum))]
            public fixed byte BackingField03[4];
        }
        ";

        string expected = @"
        partial struct EnumGeneratorTest
        {
            public  BitsKit.Tests.TestEnum Generated00 
            {
                readonly get => (BitsKit.Tests.TestEnum)(unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30)));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x3U) | ((unchecked((UInt32)(value)) << 0) & 0x3U)));
            }

            public  BitsKit.Tests.TestEnum Generated01 
            {
                readonly get => (BitsKit.Tests.TestEnum)(unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 28)) >> 30)));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0xCU) | ((unchecked((UInt32)(value)) << 2) & 0xCU)));
            }

            public  BitsKit.Tests.TestEnum Generated10 
            {
                readonly get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(BackingField01, 0, 2);
                set => BitPrimitives.WriteInt32LSB(BackingField01, 0, (Int32)value, 2);
            }

            public  BitsKit.Tests.TestEnum Generated11 
            {
                readonly get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(BackingField01, 2, 2);
                set => BitPrimitives.WriteInt32LSB(BackingField01, 2, (Int32)value, 2);
            }

            public  BitsKit.Tests.TestEnum Generated20 
            {
                get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(BackingField02, 0, 2);
            }

            public  BitsKit.Tests.TestEnum Generated21 
            {
                get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(BackingField02, 2, 2);
            }

            public unsafe  BitsKit.Tests.TestEnum Generated30 
            {
                get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(MemoryMarshal.CreateReadOnlySpan(ref BackingField03[0], 4), 0, 2);
                set => BitPrimitives.WriteInt32LSB(MemoryMarshal.CreateSpan(ref BackingField03[0], 4), 0, (Int32)value, 2);
            }

            public unsafe  BitsKit.Tests.TestEnum Generated31 
            {
                get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(MemoryMarshal.CreateReadOnlySpan(ref BackingField03[0], 4), 2, 2);
                set => BitPrimitives.WriteInt32LSB(MemoryMarshal.CreateSpan(ref BackingField03[0], 4), 2, (Int32)value, 2);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

    [TestMethod]
    public void IntegerConversionTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        public ref partial struct BitFieldIntegerConversion
        {
            [BitField(""Generated00"", 2, BitFieldType.Byte)]
            public int BackingField00;

            [BitField(""Generated10"", 2, BitFieldType.Int32)]
            public Span<byte> BackingField01;
        }
        ";

        string expected = @"
        partial struct BitFieldIntegerConversion
        {
            public  Byte Generated00 
            {
                readonly get => (Byte)(unchecked((Int32)(unchecked((Int32)(unchecked((UInt32)BackingField00) << 30)) >> 30)));
                set => BackingField00 = unchecked((Int32)((unchecked((UInt32)BackingField00) & ~0x3U) | ((unchecked((UInt32)(value)) << 0) & 0x3U)));
            }

            public  Int32 Generated10 
            {
                readonly get => BitPrimitives.ReadInt32LSB(BackingField01, 0, 2);
                set => BitPrimitives.WriteInt32LSB(BackingField01, 0, value, 2);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

#if NET8_0_OR_GREATER

    [TestMethod]
    public void InlineArrayTest()
    {
        string source = @"
        [BitObject(BitOrder.LeastSignificant)]
        [InlineArray(length: 10)]
        public partial struct BitFieldInlineArray
        {
            [BitField(""Generated00"", 2)]
            [BitField(""Generated01"", 2, BitFieldType.Byte)]
            [EnumField(""Generated02"", 2, typeof(BitsKit.Tests.TestEnum))]
            [BooleanField(""Generated03"")]
            public int BackingField00;
        }
        ";

        string expected = @"
        partial struct BitFieldInlineArray
        {
            public  Int32 Generated00 
            {
                 get => BitPrimitives.ReadInt32LSB(MemoryMarshal.AsBytes<int>(this), 0, 2);
                 set => BitPrimitives.WriteInt32LSB(MemoryMarshal.AsBytes((Span<int>)this), 0, value, 2);
            }

            public  Byte Generated01 
            {
                 get => BitPrimitives.ReadUInt8LSB(MemoryMarshal.AsBytes<int>(this), 2, 2);
                 set => BitPrimitives.WriteUInt8LSB(MemoryMarshal.AsBytes((Span<int>)this), 2, value, 2);
            }

            public  BitsKit.Tests.TestEnum Generated02 
            {
                 get => (BitsKit.Tests.TestEnum)BitPrimitives.ReadInt32LSB(MemoryMarshal.AsBytes<int>(this), 4, 2);
                 set => BitPrimitives.WriteInt32LSB(MemoryMarshal.AsBytes((Span<int>)this), 4, (Int32)value, 2);
            }

            public  System.Boolean Generated03 
            {
                 get => BitPrimitives.ReadBitLSB(MemoryMarshal.AsBytes<int>(this), 6);
                 set => BitPrimitives.WriteBitLSB(MemoryMarshal.AsBytes((Span<int>)this), 6, value);
            }
        }
        ";

        string? sourceOutput = GenerateSourceAndTest(source);

        Assert.IsTrue(Helpers.StrEqualExWhiteSpace(sourceOutput, expected));
    }

#endif

    private static string? GenerateSourceAndTest(string source)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
                                  .Where(assembly => !assembly.IsDynamic)
                                  .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                                  .Cast<MetadataReference>();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "BitsKit.Tests.InMemory",
            syntaxTrees: [CSharpSyntaxTree.ParseText(Helpers.GeneratorTestHeader + source)],
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                checkOverflow: true
            )
        );

        var insignificantEditComp = compilation.Clone()
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy"));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new BitObjectGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        var run1Result = RunGenerator(ref driver, compilation);
        var run2Result = RunGenerator(ref driver, insignificantEditComp);

        foreach (var outputStep in run2Result.Results[0].TrackedOutputSteps)
        {
            AssertGeneratorDidntRun(outputStep.Value);
        }
        AssertGeneratorDidntRun(run2Result.Results[0].TrackedSteps["Main"]);

        Assert.AreEqual(1, run1Result.GeneratedTrees.Length);
        Assert.IsTrue(run1Result.Diagnostics.IsEmpty);

        GeneratorRunResult generatorResult = run1Result.Results[0];
        Assert.AreEqual(typeof(BitObjectGenerator), generatorResult.Generator.GetGeneratorType());
        Assert.IsTrue(generatorResult.Diagnostics.IsEmpty);
        Assert.AreEqual(1, generatorResult.GeneratedSources.Length);
        Assert.IsTrue(generatorResult.Exception is null);

        string sourceOutput = generatorResult.GeneratedSources[0].SourceText.ToString();
        return TruncateUsings(sourceOutput);
    }

    private static GeneratorDriverRunResult RunGenerator(
        ref GeneratorDriver driver,
        Compilation compilation
    )
    {
        driver = driver
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics
            );

        // verify the compilation with the added source has no diagnostics
        Assert.IsFalse(
            outputCompilation
                .GetDiagnostics()
                .Any(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
        );

        // there were no diagnostics created by the generators
        Assert.IsTrue(diagnostics.IsEmpty);

        return driver.GetRunResult();
    }

    private static void AssertGeneratorDidntRun(ImmutableArray<IncrementalGeneratorRunStep> steps)
    {
        var outputs = steps.SelectMany(o => o.Outputs);
        foreach (var output in outputs)
        {
            Assert.IsTrue(output.Reason == IncrementalStepRunReason.Unchanged ||
                          output.Reason == IncrementalStepRunReason.Cached);
        }
    }

    private static string? TruncateUsings(string? source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        int index = source.LastIndexOf("using");
        int eol = source.IndexOf('\n', index);

        return source[eol..].TrimStart();
    }
}

/// <summary>Loads the BitsKit.Generator assembly into the current AppDomain</summary>
[BitObject(BitOrder.LeastSignificant)]
internal readonly partial struct BitsKitGeneratorStub { }
