namespace BitsKit.BitFields;

/// <summary>
/// An attribute that declares an object contains bit-fields
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
public sealed class BitObjectAttribute(BitOrder defaultBitOrder) : Attribute
{
    /// <summary>
    /// Defines the default bit order for the object
    /// </summary>
    public BitOrder DefaultOrder { get; } = defaultBitOrder;

    /// <summary>
    /// Controls whether generated accessors may use unchecked byte-addressable storage operations.
    /// </summary>
    /// <remarks>
    /// <see cref="BitObjectAccessMode.Unsafe"/> must only be used when every instance is guaranteed
    /// to provide non-empty backing storage large enough for each generated access width.
    /// </remarks>
    public BitObjectAccessMode AccessMode { get; set; } = BitObjectAccessMode.Checked;
}
