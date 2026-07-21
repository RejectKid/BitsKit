using BitsKit.BitFields;

namespace BitsKit.Benchmarks;

[BitObject(BitOrder.LeastSignificant)]
public partial struct GeneratedAccessorLsbModel
{
    [BitField(5)]
    [BitField("Value", 11)]
    public uint BackingField;
}
