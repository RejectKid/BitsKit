namespace BitsKit.BitFields;

/// <summary>
/// Controls how generated bit-field accessors validate byte-addressable backing storage.
/// </summary>
public enum BitObjectAccessMode
{
    /// <summary>
    /// Generated accessors retain bounds checks and throw when backing storage is too small.
    /// </summary>
    Checked,

    /// <summary>
    /// Generated accessors may skip bounds checks for byte-addressable backing storage when the
    /// unchecked path is faster than the checked specialization.
    /// </summary>
    /// <remarks>
    /// The caller must guarantee that every backing buffer is non-empty and large enough for
    /// the generated access width. Violating that contract can read or modify memory outside
    /// the declared buffer and may cause data corruption, information disclosure, or process failure.
    /// </remarks>
    Unsafe
}
