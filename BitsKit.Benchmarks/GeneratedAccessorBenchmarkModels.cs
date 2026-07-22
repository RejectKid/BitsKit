using BitsKit.BitFields;

namespace BitsKit.Benchmarks;

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorLsbModel
{
    [BitField(5)]
    [BitField("Value", 11)]
    public uint BackingField;

    [BitField(7)]
    [BitField("SignedValue", 13)]
    public int SignedBackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public uint BooleanBackingField;

    [BitField(3)]
    [EnumField("Kind", 3, typeof(GeneratedAccessorKind))]
    public uint EnumBackingField;

    [BitField(9)]
    [BitField("WideValue", 43)]
    public ulong WideBackingField;

    [BitField(5)]
    [BitField("MostSignificantValue", 11, ReverseBitOrder = true)]
    public uint MostSignificantBackingField;

    [BitField(7)]
    [BitField("MostSignificantWideValue", 43, ReverseBitOrder = true)]
    public ulong MostSignificantWideBackingField;

    [BitField(5)]
    [BooleanField("MostSignificantFlag", ReverseBitOrder = true)]
    public uint MostSignificantBooleanBackingField;

    [BitField(3)]
    [EnumField("MostSignificantKind", 3, typeof(GeneratedAccessorKind), ReverseBitOrder = true)]
    public uint MostSignificantEnumBackingField;
}

public enum GeneratedAccessorKind : uint
{
    Zero,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorMemoryModel
{
    [BitField(5)]
    [BitField("Value", 11, BitFieldType.UInt32)]
    public Memory<byte> BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBackingField;

    [BitField(3)]
    [BitField("Value12", 12, BitFieldType.UInt32)]
    public Memory<byte> BackingField12;

    [BitField(5)]
    [BitField("Value24", 24, BitFieldType.UInt32)]
    public Memory<byte> BackingField24;

    [BitField(7)]
    [BitField("Value48", 48, BitFieldType.UInt64)]
    public Memory<byte> BackingField48;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorAlignedMemoryModel
{
    [BitField("UInt32Value", 32, BitFieldType.UInt32)]
    public Memory<byte> UInt32BackingField;

    [BitField("UInt64Value", 64, BitFieldType.UInt64)]
    public Memory<byte> UInt64BackingField;
}

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorCheckedAccessModel
{
    [BitField(3)]
    [BitField("Value20", 20, BitFieldType.UInt32)]
    public Memory<byte> Value20BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBackingField;
}

[BitObject(BitOrder.LeastSignificant, AccessMode = BitObjectAccessMode.Unsafe)]
public partial struct GeneratedAccessorUnsafeAccessModel
{
    [BitField(3)]
    [BitField("Value20", 20, BitFieldType.UInt32)]
    public Memory<byte> Value20BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Memory<byte> BooleanBackingField;
}

[BitObject(BitOrder.LeastSignificant)]
public ref partial struct GeneratedAccessorAlignedSpanModel
{
    [BitField("UInt32Value", 32, BitFieldType.UInt32)]
    public Span<byte> UInt32BackingField;

    [BitField(5)]
    [BooleanField("Flag")]
    public Span<byte> BooleanBackingField;
}

#if NET8_0_OR_GREATER

[BitObject(BitOrder.LeastSignificant)]
[System.Runtime.CompilerServices.InlineArray(4)]
public partial struct GeneratedAccessorInlineArrayModel
{
    [BitField(5)]
    [BitField("Value", 11, BitFieldType.UInt32, Modifiers = BitFieldModifiers.ReadOnly)]
    private byte _element;
}

[BitObject(BitOrder.LeastSignificant)]
[System.Runtime.CompilerServices.InlineArray(4)]
public partial struct GeneratedAccessorAlignedInlineArrayModel
{
    [BitField("UInt32Value", 32, BitFieldType.UInt32)]
    private byte _element;
}

#endif
