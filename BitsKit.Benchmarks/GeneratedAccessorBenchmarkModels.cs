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

#endif
