using System.Runtime.InteropServices;

namespace BitsKit.Primitives;

/// <summary>
/// Reads or writes repeated packed and record-strided bit fields with one bounds validation.
/// </summary>
/// <remarks>
/// The destination or values span determines the number of fields processed. A contiguous overload
/// advances by <c>bitCount</c>; a strided overload advances by <c>bitStride</c>. Strides smaller than
/// the field width are rejected so writes cannot overlap.
/// </remarks>
public static class BitBatchPrimitives
{
    #region LSB integral batches

    /// <summary>Reads contiguous Int8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt8LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<sbyte> destination) =>
        ReadInt8LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt8LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<sbyte> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt8LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<sbyte>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous Int8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt8LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<sbyte> values) =>
        WriteInt8LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt8LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<sbyte> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt8LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous UInt8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt8LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<byte> destination) =>
        ReadUInt8LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt8LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<byte> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt8LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<byte>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous UInt8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt8LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<byte> values) =>
        WriteUInt8LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt8LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<byte> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt8LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous Int16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt16LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<short> destination) =>
        ReadInt16LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt16LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<short> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 16);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt16LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<short>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous Int16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt16LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<short> values) =>
        WriteInt16LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt16LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<short> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 16);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt16LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous UInt16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt16LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<ushort> destination) =>
        ReadUInt16LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt16LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<ushort> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 16);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt16LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<ushort>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous UInt16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt16LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<ushort> values) =>
        WriteUInt16LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt16LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<ushort> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 16);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt16LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous Int32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt32LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<int> destination) =>
        ReadInt32LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt32LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<int> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 32);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt32LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<int>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous Int32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt32LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<int> values) =>
        WriteInt32LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt32LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<int> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 32);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt32LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous UInt32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt32LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<uint> destination) =>
        ReadUInt32LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt32LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<uint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 32);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt32LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<uint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous UInt32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt32LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<uint> values) =>
        WriteUInt32LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt32LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<uint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 32);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                WriteUInt32LSBUnchecked(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous Int64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt64LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<long> destination) =>
        ReadInt64LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt64LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<long> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 64);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt64LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<long>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous Int64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt64LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<long> values) =>
        WriteInt64LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt64LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<long> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 64);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt64LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous UInt64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt64LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<ulong> destination) =>
        ReadUInt64LSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt64LSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<ulong> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 64);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt64LSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<ulong>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous UInt64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt64LSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<ulong> values) =>
        WriteUInt64LSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt64LSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<ulong> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 64);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt64LSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous IntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadIntPtrLSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<nint> destination) =>
        ReadIntPtrLSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided IntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadIntPtrLSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<nint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, IntPtr.Size * 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadIntPtrLSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<nint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous IntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteIntPtrLSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<nint> values) =>
        WriteIntPtrLSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided IntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteIntPtrLSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<nint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, IntPtr.Size * 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteIntPtrLSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Reads contiguous UIntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUIntPtrLSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<nuint> destination) =>
        ReadUIntPtrLSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UIntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUIntPtrLSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<nuint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, IntPtr.Size * 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUIntPtrLSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<nuint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.LeastSignificant);
        }
    }

    /// <summary>Writes contiguous UIntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteUIntPtrLSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<nuint> values) =>
        WriteUIntPtrLSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UIntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteUIntPtrLSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<nuint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, IntPtr.Size * 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUIntPtrLSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.LeastSignificant);
        }
    }

    #endregion

    #region MSB integral batches

    /// <summary>Reads contiguous Int8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt8MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<sbyte> destination) =>
        ReadInt8MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt8MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<sbyte> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt8MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<sbyte>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous Int8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt8MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<sbyte> values) =>
        WriteInt8MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt8MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<sbyte> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt8MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous UInt8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt8MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<byte> destination) =>
        ReadUInt8MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt8 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt8MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<byte> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt8MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<byte>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous UInt8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt8MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<byte> values) =>
        WriteUInt8MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt8 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt8MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<byte> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt8MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous Int16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt16MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<short> destination) =>
        ReadInt16MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt16MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<short> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 16);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt16MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<short>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous Int16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt16MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<short> values) =>
        WriteInt16MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt16MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<short> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 16);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt16MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous UInt16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt16MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<ushort> destination) =>
        ReadUInt16MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt16 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt16MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<ushort> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 16);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt16MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<ushort>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous UInt16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt16MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<ushort> values) =>
        WriteUInt16MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt16 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt16MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<ushort> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 16);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 4;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt16MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous Int32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt32MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<int> destination) =>
        ReadInt32MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt32MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<int> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 32);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt32MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<int>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous Int32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt32MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<int> values) =>
        WriteInt32MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt32MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<int> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 32);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt32MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous UInt32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt32MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<uint> destination) =>
        ReadUInt32MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt32 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt32MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<uint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 32);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt32MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<uint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous UInt32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt32MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<uint> values) =>
        WriteUInt32MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt32 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt32MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<uint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 32);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                WriteUInt32MSBUnchecked(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous Int64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt64MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<long> destination) =>
        ReadInt64MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided Int64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadInt64MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<long> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 64);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadInt64MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<long>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous Int64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt64MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<long> values) =>
        WriteInt64MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided Int64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteInt64MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<long> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 64);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteInt64MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous UInt64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt64MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<ulong> destination) =>
        ReadUInt64MSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UInt64 bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUInt64MSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<ulong> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, 64);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUInt64MSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<ulong>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous UInt64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt64MSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<ulong> values) =>
        WriteUInt64MSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UInt64 bit fields from <paramref name="values"/>.</summary>
    public static void WriteUInt64MSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<ulong> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, 64);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = 16;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUInt64MSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous IntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadIntPtrMSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<nint> destination) =>
        ReadIntPtrMSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided IntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadIntPtrMSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<nint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, IntPtr.Size * 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadIntPtrMSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<nint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous IntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteIntPtrMSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<nint> values) =>
        WriteIntPtrMSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided IntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteIntPtrMSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<nint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, IntPtr.Size * 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteIntPtrMSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Reads contiguous UIntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUIntPtrMSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, Span<nuint> destination) =>
        ReadUIntPtrMSB(source, bitOffset, bitCount, bitCount, destination);

    /// <summary>Reads strided UIntPtr bit fields into <paramref name="destination"/>.</summary>
    public static void ReadUIntPtrMSB(ReadOnlySpan<byte> source, int bitOffset, int bitCount, int bitStride, Span<nuint> destination)
    {
        ValidateBatch(source.Length, bitOffset, bitCount, bitStride, destination.Length, IntPtr.Size * 8);
        if (destination.IsEmpty) return;
        if (bitCount == 0) { destination.Clear(); return; }
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = byteOffset <= source.Length - accessWidth
                ? ReadUIntPtrMSBUnchecked(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte, bitCount)
                : ReadTail<nuint>(source.Slice(byteOffset), bitInByte, bitCount, BitOrder.MostSignificant);
        }
    }

    /// <summary>Writes contiguous UIntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteUIntPtrMSB(Span<byte> destination, int bitOffset, int bitCount, ReadOnlySpan<nuint> values) =>
        WriteUIntPtrMSB(destination, bitOffset, bitCount, bitCount, values);

    /// <summary>Writes strided UIntPtr bit fields from <paramref name="values"/>.</summary>
    public static void WriteUIntPtrMSB(Span<byte> destination, int bitOffset, int bitCount, int bitStride, ReadOnlySpan<nuint> values)
    {
        ValidateBatch(destination.Length, bitOffset, bitCount, bitStride, values.Length, IntPtr.Size * 8);
        if (values.IsEmpty || bitCount == 0) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        int accessWidth = IntPtr.Size == 8 ? 16 : 8;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            if (byteOffset <= destination.Length - accessWidth)
                UnsafeBitPrimitives.WriteUIntPtrMSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i], bitCount);
            else
                WriteTail(destination.Slice(byteOffset), bitInByte, values[i], bitCount, BitOrder.MostSignificant);
        }
    }

    #endregion

    #region Boolean batches

    /// <summary>Reads contiguous Boolean bits into <paramref name="destination"/>.</summary>
    public static void ReadBitLSB(ReadOnlySpan<byte> source, int bitOffset, Span<bool> destination) =>
        ReadBitLSB(source, bitOffset, 1, destination);

    /// <summary>Reads strided Boolean bits into <paramref name="destination"/>.</summary>
    public static void ReadBitLSB(ReadOnlySpan<byte> source, int bitOffset, int bitStride, Span<bool> destination)
    {
        ValidateBatch(source.Length, bitOffset, 1, bitStride, destination.Length, 1);
        if (destination.IsEmpty) return;
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = UnsafeBitPrimitives.ReadBitLSB(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte);
        }
    }

    /// <summary>Writes contiguous Boolean bits from <paramref name="values"/>.</summary>
    public static void WriteBitLSB(Span<byte> destination, int bitOffset, ReadOnlySpan<bool> values) =>
        WriteBitLSB(destination, bitOffset, 1, values);

    /// <summary>Writes strided Boolean bits from <paramref name="values"/>.</summary>
    public static void WriteBitLSB(Span<byte> destination, int bitOffset, int bitStride, ReadOnlySpan<bool> values)
    {
        ValidateBatch(destination.Length, bitOffset, 1, bitStride, values.Length, 1);
        if (values.IsEmpty) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            UnsafeBitPrimitives.WriteBitLSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i]);
        }
    }

    /// <summary>Reads contiguous Boolean bits into <paramref name="destination"/>.</summary>
    public static void ReadBitMSB(ReadOnlySpan<byte> source, int bitOffset, Span<bool> destination) =>
        ReadBitMSB(source, bitOffset, 1, destination);

    /// <summary>Reads strided Boolean bits into <paramref name="destination"/>.</summary>
    public static void ReadBitMSB(ReadOnlySpan<byte> source, int bitOffset, int bitStride, Span<bool> destination)
    {
        ValidateBatch(source.Length, bitOffset, 1, bitStride, destination.Length, 1);
        if (destination.IsEmpty) return;
        ref byte sourceReference = ref MemoryMarshal.GetReference(source);
        int currentOffset = bitOffset;
        for (int i = 0; i < destination.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            destination[i] = UnsafeBitPrimitives.ReadBitMSB(ref Unsafe.Add(ref sourceReference, byteOffset), bitInByte);
        }
    }

    /// <summary>Writes contiguous Boolean bits from <paramref name="values"/>.</summary>
    public static void WriteBitMSB(Span<byte> destination, int bitOffset, ReadOnlySpan<bool> values) =>
        WriteBitMSB(destination, bitOffset, 1, values);

    /// <summary>Writes strided Boolean bits from <paramref name="values"/>.</summary>
    public static void WriteBitMSB(Span<byte> destination, int bitOffset, int bitStride, ReadOnlySpan<bool> values)
    {
        ValidateBatch(destination.Length, bitOffset, 1, bitStride, values.Length, 1);
        if (values.IsEmpty) return;
        ref byte destinationReference = ref MemoryMarshal.GetReference(destination);
        int currentOffset = bitOffset;
        for (int i = 0; i < values.Length; i++, currentOffset += bitStride)
        {
            int byteOffset = (int)(currentOffset >> 3);
            int bitInByte = (int)currentOffset & 7;
            UnsafeBitPrimitives.WriteBitMSB(ref Unsafe.Add(ref destinationReference, byteOffset), bitInByte, values[i]);
        }
    }

    #endregion

    private static void ValidateBatch(
        int byteLength,
        int bitOffset,
        int bitCount,
        int bitStride,
        int valueCount,
        int maxBits)
    {
        if ((uint)bitCount > (uint)maxBits)
            throw new ArgumentOutOfRangeException(nameof(bitCount));
        if (bitStride < bitCount)
            throw new ArgumentOutOfRangeException(nameof(bitStride));
        if (bitOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(bitOffset));

        long requiredBits = valueCount == 0
            ? bitOffset
            : (long)bitOffset + ((long)valueCount - 1) * bitStride + bitCount;

        if (requiredBits > (long)byteLength * 8 || requiredBits > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(bitOffset), "The batch exceeds the available buffer.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadUInt8LSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref source);
        return unchecked((byte)BitPrimitives.ReadValue32(value, bitOffset, bitCount, BitOrder.LeastSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte ReadInt8LSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((sbyte)SignExtend(ReadUInt8LSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReadUInt16LSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref source);
        return unchecked((ushort)BitPrimitives.ReadValue32(value, bitOffset, bitCount, BitOrder.LeastSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short ReadInt16LSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((short)SignExtend(ReadUInt16LSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadInt32LSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((int)SignExtend(ReadUInt32LSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadUInt64LSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        UInt128 value = Unsafe.ReadUnaligned<UInt128>(ref source);
        return bitCount + bitOffset > 64
            ? unchecked((ulong)BitPrimitives.ReadValue128(value, bitOffset, bitCount, BitOrder.LeastSignificant))
            : BitPrimitives.ReadValue64(unchecked((ulong)value), bitOffset, bitCount, BitOrder.LeastSignificant);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ReadInt64LSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        SignExtend(ReadUInt64LSBUnchecked(ref source, bitOffset, bitCount), bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ReadIntPtrLSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8
            ? unchecked((nint)ReadInt64LSBUnchecked(ref source, bitOffset, bitCount))
            : ReadInt32LSBUnchecked(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint ReadUIntPtrLSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8
            ? unchecked((nuint)ReadUInt64LSBUnchecked(ref source, bitOffset, bitCount))
            : ReadUInt32LSBUnchecked(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadUInt8MSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref source);
        return unchecked((byte)BitPrimitives.ReadValue32(value, bitOffset, bitCount, BitOrder.MostSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte ReadInt8MSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((sbyte)SignExtend(ReadUInt8MSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReadUInt16MSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref source);
        return unchecked((ushort)BitPrimitives.ReadValue32(value, bitOffset, bitCount, BitOrder.MostSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short ReadInt16MSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((short)SignExtend(ReadUInt16MSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadInt32MSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        unchecked((int)SignExtend(ReadUInt32MSBUnchecked(ref source, bitOffset, bitCount), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadUInt64MSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        UInt128 value = Unsafe.ReadUnaligned<UInt128>(ref source);
        return bitCount + bitOffset > 64
            ? unchecked((ulong)BitPrimitives.ReadValue128(value, bitOffset, bitCount, BitOrder.MostSignificant))
            : BitPrimitives.ReadValue64(unchecked((ulong)value), bitOffset, bitCount, BitOrder.MostSignificant);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ReadInt64MSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        SignExtend(ReadUInt64MSBUnchecked(ref source, bitOffset, bitCount), bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ReadIntPtrMSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8
            ? unchecked((nint)ReadInt64MSBUnchecked(ref source, bitOffset, bitCount))
            : ReadInt32MSBUnchecked(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint ReadUIntPtrMSBUnchecked(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8
            ? unchecked((nuint)ReadUInt64MSBUnchecked(ref source, bitOffset, bitCount))
            : ReadUInt32MSBUnchecked(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long SignExtend(ulong value, int bitCount) =>
        bitCount == 0 ? 0 : unchecked((long)(value << (64 - bitCount))) >> (64 - bitCount);



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32LSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        ulong value = Unsafe.ReadUnaligned<ulong>(ref source);
        return bitCount + bitOffset <= 32
            ? BitPrimitives.ReadValue32(unchecked((uint)value), bitOffset, bitCount, BitOrder.LeastSignificant)
            : unchecked((uint)BitPrimitives.ReadValue64(value, bitOffset, bitCount, BitOrder.LeastSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32MSBUnchecked(ref byte source, int bitOffset, int bitCount)
    {
        ulong value = Unsafe.ReadUnaligned<ulong>(ref source);
        return bitCount + bitOffset <= 32
            ? BitPrimitives.ReadValue32(unchecked((uint)value), bitOffset, bitCount, BitOrder.MostSignificant)
            : unchecked((uint)BitPrimitives.ReadValue64(value, bitOffset, bitCount, BitOrder.MostSignificant));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteUInt32LSBUnchecked(ref byte destination, int bitOffset, uint value, int bitCount)
    {
        if (bitCount + bitOffset > 32)
            BitPrimitives.WriteValue64(ref Unsafe.As<byte, ulong>(ref destination), bitOffset, value, bitCount, BitOrder.LeastSignificant);
        else
            BitPrimitives.WriteValue32(ref Unsafe.As<byte, uint>(ref destination), bitOffset, value, bitCount, BitOrder.LeastSignificant);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteUInt32MSBUnchecked(ref byte destination, int bitOffset, uint value, int bitCount)
    {
        if (bitCount + bitOffset > 32)
            BitPrimitives.WriteValue64(ref Unsafe.As<byte, ulong>(ref destination), bitOffset, value, bitCount, BitOrder.MostSignificant);
        else
            BitPrimitives.WriteValue32(ref Unsafe.As<byte, uint>(ref destination), bitOffset, value, bitCount, BitOrder.MostSignificant);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ReadTail<T>(
        ReadOnlySpan<byte> source,
        int bitOffset,
        int bitCount,
        BitOrder bitOrder)
        where T : unmanaged
    {
        Span<byte> padded = stackalloc byte[16];
        source.CopyTo(padded);
        return ReadUnchecked<T>(ref MemoryMarshal.GetReference(padded), bitOffset, bitCount, bitOrder);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WriteTail<T>(
        Span<byte> destination,
        int bitOffset,
        T value,
        int bitCount,
        BitOrder bitOrder)
        where T : unmanaged
    {
        Span<byte> padded = stackalloc byte[16];
        destination.CopyTo(padded);
        WriteUnchecked(ref MemoryMarshal.GetReference(padded), bitOffset, value, bitCount, bitOrder);
        padded.Slice(0, destination.Length).CopyTo(destination);
    }

    private static T ReadUnchecked<T>(ref byte source, int bitOffset, int bitCount, BitOrder bitOrder)
        where T : unmanaged
    {
        if (typeof(T) == typeof(sbyte))
        {
            sbyte value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadInt8LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadInt8MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<sbyte, T>(ref value);
        }

        if (typeof(T) == typeof(byte))
        {
            byte value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadUInt8LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadUInt8MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<byte, T>(ref value);
        }

        if (typeof(T) == typeof(short))
        {
            short value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadInt16LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadInt16MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<short, T>(ref value);
        }

        if (typeof(T) == typeof(ushort))
        {
            ushort value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadUInt16LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadUInt16MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<ushort, T>(ref value);
        }

        if (typeof(T) == typeof(int))
        {
            int value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadInt32LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadInt32MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<int, T>(ref value);
        }

        if (typeof(T) == typeof(uint))
        {
            uint value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadUInt32LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadUInt32MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<uint, T>(ref value);
        }

        if (typeof(T) == typeof(long))
        {
            long value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadInt64LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadInt64MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<long, T>(ref value);
        }

        if (typeof(T) == typeof(ulong))
        {
            ulong value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadUInt64LSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadUInt64MSB(ref source, bitOffset, bitCount);
            return Unsafe.As<ulong, T>(ref value);
        }

        if (typeof(T) == typeof(nint))
        {
            nint value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadIntPtrLSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadIntPtrMSB(ref source, bitOffset, bitCount);
            return Unsafe.As<nint, T>(ref value);
        }

        if (typeof(T) == typeof(nuint))
        {
            nuint value = bitOrder == BitOrder.LeastSignificant
                ? UnsafeBitPrimitives.ReadUIntPtrLSB(ref source, bitOffset, bitCount)
                : UnsafeBitPrimitives.ReadUIntPtrMSB(ref source, bitOffset, bitCount);
            return Unsafe.As<nuint, T>(ref value);
        }

        throw new NotSupportedException();
    }

    private static void WriteUnchecked<T>(ref byte destination, int bitOffset, T value, int bitCount, BitOrder bitOrder)
        where T : unmanaged
    {
        if (typeof(T) == typeof(sbyte))
        {
            sbyte typedValue = Unsafe.As<T, sbyte>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteInt8LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteInt8MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(byte))
        {
            byte typedValue = Unsafe.As<T, byte>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteUInt8LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteUInt8MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(short))
        {
            short typedValue = Unsafe.As<T, short>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteInt16LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteInt16MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(ushort))
        {
            ushort typedValue = Unsafe.As<T, ushort>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteUInt16LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteUInt16MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(int))
        {
            int typedValue = Unsafe.As<T, int>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteInt32LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteInt32MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(uint))
        {
            uint typedValue = Unsafe.As<T, uint>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteUInt32LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteUInt32MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(long))
        {
            long typedValue = Unsafe.As<T, long>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteInt64LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteInt64MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(ulong))
        {
            ulong typedValue = Unsafe.As<T, ulong>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteUInt64LSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteUInt64MSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(nint))
        {
            nint typedValue = Unsafe.As<T, nint>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteIntPtrLSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteIntPtrMSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        if (typeof(T) == typeof(nuint))
        {
            nuint typedValue = Unsafe.As<T, nuint>(ref value);
            if (bitOrder == BitOrder.LeastSignificant)
                UnsafeBitPrimitives.WriteUIntPtrLSB(ref destination, bitOffset, typedValue, bitCount);
            else
                UnsafeBitPrimitives.WriteUIntPtrMSB(ref destination, bitOffset, typedValue, bitCount);
            return;
        }

        throw new NotSupportedException();
    }
}
