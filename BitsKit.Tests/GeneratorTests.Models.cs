using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BitsKit.BitFields;

namespace BitsKit.Tests;

[BitObject(BitOrder.LeastSignificant)]
[StructLayout(LayoutKind.Sequential)]
public partial struct SequentialBitFieldStruct
{
    [BitField("Generated01", 2)]
    [BitField("Generated02", 2)]
    public int BackingField00;

    [BitField("Generated03", 2)]
    [BitField("Generated04", 2)]
    public int BackingField01;

    [BitField("Padding01", 2)]
    [BitField(2)]
    [BitField("Padding02", 2)]
    public int BackingFieldPadding;

    [BitField("Generated05", 2)]
    [BitField(2)]
    [BitField("Generated06", 2)]
    public nint BackingFieldIntPtr;

    [BitField("Generated07", 2)]
    [BitField(2)]
    [BitField("Generated08", 2)]
    public nuint BackingFieldUIntPtr;
}

[BitObject(BitOrder.LeastSignificant)]
[StructLayout(LayoutKind.Explicit)]
public partial struct ExplicitBitFieldStruct
{
    [BitField("Generated01", 2)]
    [BitField("Generated02", 2)]
    [FieldOffset(0)]
    public int BackingField00;

    [BitField("Generated03", 2)]
    [BitField("Generated04", 2)]
    [FieldOffset(0)]
    public ulong BackingField01;
}

[BitObject(BitOrder.LeastSignificant)]
public partial record struct BitFieldRecordStruct
{
    [BitField("Generated01", 2)]
    [BitField("Generated02", 2)]
    public int BackingField00;

    [BitField("Generated03", 2)]
    [BitField("Generated04", 2)]
    public int BackingField01;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct BitFieldMemoryStruct
{
    [BitField("Generated01", 2, BitFieldType.Int32)]
    [BitField("Generated02", 2, BitFieldType.Int16)]
    public Memory<byte> BackingField00;

    [BitField("Generated03", 2, BitFieldType.Int32)]
    [BitField("Generated04", 2, BitFieldType.Int16)]
    public ReadOnlyMemory<byte> BackingField01;

    public readonly int IntValue00 => MemoryMarshal.Read<int>(BackingField00.Span);
    public readonly int IntValue01 => MemoryMarshal.Read<int>(BackingField01.Span);
}

[BitObject(BitOrder.LeastSignificant)]
public ref partial struct BitFieldRefStruct
{
    [BitField("Generated01", 2, BitFieldType.Int32)]
    [BitField("Generated02", 2, BitFieldType.Int16)]
    public Span<byte> BackingField00;

    [BitField("Generated03", 2, BitFieldType.Int32)]
    [BitField("Generated04", 2, BitFieldType.Int16)]
    public ReadOnlySpan<byte> BackingField01;

    public readonly int IntValue00 => MemoryMarshal.Read<int>(BackingField00);
    public readonly int IntValue01 => MemoryMarshal.Read<int>(BackingField01);
}

[BitObject(BitOrder.LeastSignificant)]
public unsafe partial struct BitFieldFixedStruct
{
    [BitField("Generated01", 2, BitFieldType.Int32)]
    [BitField("Generated02", 2, BitFieldType.Int16)]
    public fixed byte BackingField00[4];

    [BitField("Generated03", 2, BitFieldType.Int32)]
    [BitField("Generated04", 2, BitFieldType.Int16)]
    public int BackingField01;

    public int IntValue00
    {
        get => Unsafe.ReadUnaligned<int>(ref BackingField00[0]);
        set => Unsafe.WriteUnaligned(ref BackingField00[0], value);
    }
}

[BitObject(BitOrder.LeastSignificant)]
[StructLayout(LayoutKind.Sequential)]
public partial struct EnumBitFieldStruct
{
    [EnumField("Generated01", 2, typeof(TestEnum))]
    [EnumField("Generated02", 2, typeof(TestEnum))]
    public uint BackingField00;

    [EnumField("Generated03", 2, typeof(TestEnum))]
    [EnumField("Generated04", 2, typeof(TestEnum))]
    public uint BackingField01;

    [EnumField("Padding01", 2, typeof(TestEnum))]
    [BitField(2)]
    [EnumField("Padding02", 2, typeof(TestEnum))]
    public uint BackingFieldPadding;

    [EnumField("Generated07", 2, typeof(TestEnum))]
    [BitField(2)]
    [EnumField("Generated08", 2, typeof(TestEnum))]
    public nuint BackingFieldUIntPtr;
}

[BitObject(BitOrder.LeastSignificant)]
[StructLayout(LayoutKind.Sequential)]
public partial struct PaddingFieldStruct
{
    [BitField("Generated00", 2)]
    [BitField(2)]
    [BitField("Generated01", 2)]
    [BooleanField]
    [BooleanField("Generated02")]
    [EnumField(2)]
    [EnumField("Generated03", 2, typeof(TestEnum))]
    public uint BackingField00;
}

[BitObject(BitOrder.LeastSignificant)]
[StructLayout(LayoutKind.Explicit)]
public readonly partial struct ReadOnlyStruct
{
    [BitField("Generated01", 2)]
    [BitField("Generated02", 2)]
    [FieldOffset(0)]
    public readonly int BackingField00;

    [BitField("Generated03", 2)]
    [BitField("Generated04", 2)]
    [FieldOffset(0)]
    public readonly ulong BackingField01;
}

#if NET8_0_OR_GREATER

[BitObject(BitOrder.LeastSignificant)]
[InlineArray(2)]
public partial struct InlineArrayStruct
{
    [BitField("Generated01", 2)]
    [BitField("Generated02", 2)]
    [BitField(27)]
    [BitField("Generated03", 2)] // boundary straddled
    public int BackingField00;
}

#endif

[Flags]
public enum TestEnum
{
    A = 1,
    B = 2,
    C = 4,
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct OptimizedIntegralAccessorStruct
{
    [BitField(2)]
    [BitField("ByteValue", 5)]
    public byte ByteBacking;

    [BitField(2)]
    [BitField("ReversedByteValue", 5, ReverseBitOrder = true)]
    public byte ReversedByteBacking;

    [BitField("ReversedFullByteValue", 8, ReverseBitOrder = true)]
    public byte ReversedFullByteBacking;

    [BitField(3)]
    [BitField("ShortValue", 9)]
    public short ShortBacking;

    [BitField(3)]
    [BitField("ReversedShortValue", 9, ReverseBitOrder = true)]
    public short ReversedShortBacking;

    [BitField(5)]
    [BitField("IntValue", 11)]
    public int IntBacking;

    [BitField(5)]
    [BitField("ReversedIntValue", 11, ReverseBitOrder = true)]
    public int ReversedIntBacking;

    [BitField("FullUInt64Value", 64)]
    public ulong UInt64Backing;

    [BitField(7)]
    [BitField("ReversedUInt64Value", 43, ReverseBitOrder = true)]
    public ulong ReversedUInt64Backing;

    [BitField("ReversedFullUInt64Value", 64, ReverseBitOrder = true)]
    public ulong ReversedFullUInt64Backing;

    [BitField(5)]
    [BooleanField("Flag")]
    public uint BooleanBacking;

    [BitField(5)]
    [BooleanField("SignedFlag")]
    public int SignedBooleanBacking;

    [BitField(5)]
    [BooleanField("ReversedSignedFlag", ReverseBitOrder = true)]
    public int ReversedSignedBooleanBacking;

    [BitField(5)]
    [BooleanField("ReversedFlag", ReverseBitOrder = true)]
    public uint ReversedBooleanBacking;

    [BitField(5)]
    [EnumField("EnumValue", 2, typeof(TestEnum))]
    public uint EnumBacking;

    [BitField(5)]
    [EnumField("ReversedEnumValue", 2, typeof(TestEnum), ReverseBitOrder = true)]
    public uint ReversedEnumBacking;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct OptimizedMemoryAccessorStruct
{
    [BitField(5)]
    [BitField("Value", 11, BitFieldType.UInt32)]
    public Memory<byte> Backing;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct AlignedMemoryAccessorStruct
{
    [BitField("ByteValue", 8, BitFieldType.Byte)]
    public Memory<byte> ByteBacking;

    [BitField("SignedByteValue", 8, BitFieldType.SByte)]
    public Memory<byte> SignedByteBacking;

    [BitField("UInt16Value", 16, BitFieldType.UInt16)]
    public Memory<byte> UInt16Backing;

    [BitField("Int32Value", 32, BitFieldType.Int32)]
    public Memory<byte> Int32Backing;

    [BitField("UInt64Value", 64, BitFieldType.UInt64)]
    public Memory<byte> UInt64Backing;

    [BitField("UInt16BigEndianValue", 16, BitFieldType.UInt16, ReverseBitOrder = true)]
    public Memory<byte> UInt16BigEndianBacking;

    [BitField("Int32BigEndianValue", 32, BitFieldType.Int32, ReverseBitOrder = true)]
    public Memory<byte> Int32BigEndianBacking;

    [BitField("UInt64BigEndianValue", 64, BitFieldType.UInt64, ReverseBitOrder = true)]
    public Memory<byte> UInt64BigEndianBacking;

    [BitField(8)]
    [BitField("OffsetUInt32Value", 32, BitFieldType.UInt32)]
    public Memory<byte> OffsetUInt32Backing;

    [BitField("ReadOnlyUInt32Value", 32, BitFieldType.UInt32)]
    public ReadOnlyMemory<byte> ReadOnlyUInt32Backing;

    [EnumField("EnumValue", 32, typeof(TestEnum))]
    public Memory<byte> EnumBacking;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct SpecializedMemoryAccessorStruct
{
    [BitField(5)]
    [BitField("Value11", 11, BitFieldType.UInt32)]
    public Memory<byte> Backing11;

    [BitField(3)]
    [BitField("SignedValue12", 12, BitFieldType.Int32)]
    public Memory<byte> SignedBacking12;

    [BitField(5)]
    [BitField("BigEndianValue24", 24, BitFieldType.UInt32, ReverseBitOrder = true)]
    public Memory<byte> BigEndianBacking24;

    [BitField(7)]
    [BitField("BigEndianValue48", 48, BitFieldType.UInt64, ReverseBitOrder = true)]
    public Memory<byte> BigEndianBacking48;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public partial struct UnsafeMemoryAccessorStruct
{
    [BitField(3)]
    [BitField("Value", 20, BitFieldType.UInt32)]
    public Memory<byte> Backing;

    [BitField(5)]
    [BitField("SignedValue", 13, BitFieldType.Int32, ReverseBitOrder = true)]
    public Memory<byte> SignedBacking;

    [BitField(6)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBacking;

    [BitField("AlignedValue", 32, BitFieldType.UInt32)]
    public Memory<byte> AlignedBacking;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public partial struct UnsafeArrayAccessorStruct
{
    [BitField(3)]
    [BitField("Value", 20, BitFieldType.UInt32)]
    public byte[] Backing;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public ref partial struct UnsafeSpanAccessorStruct
{
    [BitField(3)]
    [BitField("Value", 20, BitFieldType.UInt32)]
    public Span<byte> Backing;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public unsafe partial struct UnsafeFixedAccessorStruct
{
    [BitField(3)]
    [BitField("Value", 20, BitFieldType.UInt32)]
    public fixed byte Backing[16];
}

[BitObject(BitOrder.LeastSignificant)]
public ref partial struct SpecializedSpanAccessorStruct
{
    [BitField("AlignedValue", 32, BitFieldType.UInt32)]
    public Span<byte> AlignedBacking;

    [BitField(5)]
    [BooleanField("Flag")]
    [BooleanField("BigEndianFlag", ReverseBitOrder = true)]
    public Span<byte> BooleanBacking;

    [BitField("ReadOnlyAlignedValue", 32, BitFieldType.UInt32)]
    public ReadOnlySpan<byte> ReadOnlyAlignedBacking;
}

#if NET8_0_OR_GREATER

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
[InlineArray(16)]
public partial struct UnsafeInlineArrayAccessorStruct
{
    [BitField(3)]
    [BitField("Value", 20, BitFieldType.UInt32)]
    private byte _element;
}

[BitObject(BitOrder.LeastSignificant)]
[InlineArray(4)]
public partial struct OptimizedInlineArrayAccessorStruct
{
    [BitField(5)]
    [BitField("Value", 11, BitFieldType.UInt32, Modifiers = BitFieldModifiers.ReadOnly)]
    private byte _element;
}

[BitObject(BitOrder.LeastSignificant)]
[InlineArray(4)]
public partial struct SpecializedAlignedInlineArrayAccessorStruct
{
    [BitField("Value", 32, BitFieldType.UInt32)]
    private byte _element;
}

[BitObject(BitOrder.LeastSignificant)]
[InlineArray(1)]
public partial struct SpecializedBooleanInlineArrayAccessorStruct
{
    [BitField(5)]
    [BooleanField("Flag")]
    [BooleanField("BigEndianFlag", ReverseBitOrder = true)]
    private byte _element;
}

#endif
