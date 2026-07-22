namespace BitsKit.Primitives;

/// <summary>
/// Provides unchecked bit access over a raw byte reference.
/// </summary>
/// <remarks>
/// These methods perform no bounds or argument validation. The caller must guarantee that every
/// source or destination reference points to enough accessible memory for the requested operation.
/// Invalid arguments can corrupt memory, disclose data, or terminate the process. Prefer
/// <see cref="BitPrimitives"/> unless measurement proves these methods necessary.
/// </remarks>
public static class UnsafeBitPrimitives
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBitLSB(ref byte source, int bitOffset) =>
        ((Unsafe.Add(ref source, bitOffset >> 3) >> (bitOffset & 7)) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBitMSB(ref byte source, int bitOffset) =>
        ((Unsafe.Add(ref source, bitOffset >> 3) >> (7 - (bitOffset & 7))) & 1) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReadInt8LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((sbyte)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 8, BitOrder.LeastSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReadInt8MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((sbyte)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 8, BitOrder.MostSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadUInt8LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((byte)ReadUnsigned(ref source, bitOffset, bitCount, 8, BitOrder.LeastSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadUInt8MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((byte)ReadUnsigned(ref source, bitOffset, bitCount, 8, BitOrder.MostSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((short)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 16, BitOrder.LeastSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((short)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 16, BitOrder.MostSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((ushort)ReadUnsigned(ref source, bitOffset, bitCount, 16, BitOrder.LeastSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((ushort)ReadUnsigned(ref source, bitOffset, bitCount, 16, BitOrder.MostSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((int)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 32, BitOrder.LeastSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((int)SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 32, BitOrder.MostSignificant), bitCount));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32LSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((uint)ReadUnsigned(ref source, bitOffset, bitCount, 32, BitOrder.LeastSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32MSB(ref byte source, int bitOffset, int bitCount) =>
        unchecked((uint)ReadUnsigned(ref source, bitOffset, bitCount, 32, BitOrder.MostSignificant));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64LSB(ref byte source, int bitOffset, int bitCount) =>
        SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 64, BitOrder.LeastSignificant), bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64MSB(ref byte source, int bitOffset, int bitCount) =>
        SignExtend(ReadUnsigned(ref source, bitOffset, bitCount, 64, BitOrder.MostSignificant), bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64LSB(ref byte source, int bitOffset, int bitCount) =>
        ReadUnsigned(ref source, bitOffset, bitCount, 64, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64MSB(ref byte source, int bitOffset, int bitCount) =>
        ReadUnsigned(ref source, bitOffset, bitCount, 64, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint ReadIntPtrLSB(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8 ? (nint)ReadInt64LSB(ref source, bitOffset, bitCount) : ReadInt32LSB(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint ReadIntPtrMSB(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8 ? (nint)ReadInt64MSB(ref source, bitOffset, bitCount) : ReadInt32MSB(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint ReadUIntPtrLSB(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8 ? (nuint)ReadUInt64LSB(ref source, bitOffset, bitCount) : ReadUInt32LSB(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint ReadUIntPtrMSB(ref byte source, int bitOffset, int bitCount) =>
        IntPtr.Size == 8 ? (nuint)ReadUInt64MSB(ref source, bitOffset, bitCount) : ReadUInt32MSB(ref source, bitOffset, bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBitLSB(ref byte destination, int bitOffset, bool value) =>
        WriteBit(ref destination, bitOffset, value, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBitMSB(ref byte destination, int bitOffset, bool value) =>
        WriteBit(ref destination, bitOffset, value, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt8LSB(ref byte destination, int bitOffset, sbyte value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((byte)value), bitCount, 8, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt8MSB(ref byte destination, int bitOffset, sbyte value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((byte)value), bitCount, 8, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8LSB(ref byte destination, int bitOffset, byte value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 8, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8MSB(ref byte destination, int bitOffset, byte value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 8, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt16LSB(ref byte destination, int bitOffset, short value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((ushort)value), bitCount, 16, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt16MSB(ref byte destination, int bitOffset, short value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((ushort)value), bitCount, 16, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16LSB(ref byte destination, int bitOffset, ushort value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 16, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16MSB(ref byte destination, int bitOffset, ushort value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 16, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32LSB(ref byte destination, int bitOffset, int value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((uint)value), bitCount, 32, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32MSB(ref byte destination, int bitOffset, int value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((uint)value), bitCount, 32, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt32LSB(ref byte destination, int bitOffset, uint value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 32, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt32MSB(ref byte destination, int bitOffset, uint value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 32, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64LSB(ref byte destination, int bitOffset, long value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((ulong)value), bitCount, 64, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64MSB(ref byte destination, int bitOffset, long value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, unchecked((ulong)value), bitCount, 64, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt64LSB(ref byte destination, int bitOffset, ulong value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 64, BitOrder.LeastSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt64MSB(ref byte destination, int bitOffset, ulong value, int bitCount) =>
        WriteUnsigned(ref destination, bitOffset, value, bitCount, 64, BitOrder.MostSignificant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteIntPtrLSB(ref byte destination, int bitOffset, nint value, int bitCount)
    {
        if (IntPtr.Size == 8)
            WriteInt64LSB(ref destination, bitOffset, value, bitCount);
        else
            WriteInt32LSB(ref destination, bitOffset, (int)value, bitCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteIntPtrMSB(ref byte destination, int bitOffset, nint value, int bitCount)
    {
        if (IntPtr.Size == 8)
            WriteInt64MSB(ref destination, bitOffset, value, bitCount);
        else
            WriteInt32MSB(ref destination, bitOffset, (int)value, bitCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUIntPtrLSB(ref byte destination, int bitOffset, nuint value, int bitCount)
    {
        if (IntPtr.Size == 8)
            WriteUInt64LSB(ref destination, bitOffset, value, bitCount);
        else
            WriteUInt32LSB(ref destination, bitOffset, (uint)value, bitCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUIntPtrMSB(ref byte destination, int bitOffset, nuint value, int bitCount)
    {
        if (IntPtr.Size == 8)
            WriteUInt64MSB(ref destination, bitOffset, value, bitCount);
        else
            WriteUInt32MSB(ref destination, bitOffset, (uint)value, bitCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadUnsigned(ref byte source, int bitOffset, int bitCount, int maxBits, BitOrder bitOrder)
    {
        if (bitCount == 0)
            return 0;

        ref byte first = ref Unsafe.Add(ref source, bitOffset >> 3);
        int bitInByte = bitOffset & 7;

        if (maxBits <= 16)
        {
            uint value = Unsafe.ReadUnaligned<uint>(ref first);
            return BitPrimitives.ReadValue32(value, bitInByte, bitCount, bitOrder);
        }

        if (maxBits == 32)
        {
            ulong value = Unsafe.ReadUnaligned<ulong>(ref first);
            return bitCount + bitInByte <= 32
                ? BitPrimitives.ReadValue32(unchecked((uint)value), bitInByte, bitCount, bitOrder)
                : BitPrimitives.ReadValue64(value, bitInByte, bitCount, bitOrder);
        }

        UInt128 wideValue = Unsafe.ReadUnaligned<UInt128>(ref first);
        return bitCount + bitInByte > 64
            ? unchecked((ulong)BitPrimitives.ReadValue128(wideValue, bitInByte, bitCount, bitOrder))
            : BitPrimitives.ReadValue64(unchecked((ulong)wideValue), bitInByte, bitCount, bitOrder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long SignExtend(ulong value, int bitCount) =>
        bitCount == 0 ? 0 : unchecked((long)(value << (64 - bitCount))) >> (64 - bitCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBit(ref byte destination, int bitOffset, bool value, BitOrder bitOrder)
    {
        ref byte target = ref Unsafe.Add(ref destination, bitOffset >> 3);
        int bitInByte = bitOffset & 7;
        int mask = 1 << (bitOrder == BitOrder.MostSignificant ? 7 - bitInByte : bitInByte);
        target = value ? unchecked((byte)(target | mask)) : unchecked((byte)(target & ~mask));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteUnsigned(
        ref byte destination,
        int bitOffset,
        ulong value,
        int bitCount,
        int maxBits,
        BitOrder bitOrder)
    {
        ref byte first = ref Unsafe.Add(ref destination, bitOffset >> 3);
        int bitInByte = bitOffset & 7;

        if (maxBits == 8)
        {
            if (bitCount + bitInByte > 8)
                BitPrimitives.WriteValue16(ref Unsafe.As<byte, ushort>(ref first), bitInByte, unchecked((int)value), bitCount, bitOrder);
            else
                BitPrimitives.WriteValue8(ref first, bitOrder == BitOrder.MostSignificant ? 8 - bitCount - bitInByte : bitInByte, unchecked((int)value), bitCount);
            return;
        }

        if (maxBits == 16)
        {
            if (bitCount + bitInByte > 16)
                BitPrimitives.WriteValue32(ref Unsafe.As<byte, uint>(ref first), bitInByte, unchecked((uint)value), bitCount, bitOrder);
            else
                BitPrimitives.WriteValue16(ref Unsafe.As<byte, ushort>(ref first), bitInByte, unchecked((int)value), bitCount, bitOrder);
            return;
        }

        if (maxBits == 32)
        {
            if (bitCount + bitInByte > 32)
                BitPrimitives.WriteValue64(ref Unsafe.As<byte, ulong>(ref first), bitInByte, value, bitCount, bitOrder);
            else
                BitPrimitives.WriteValue32(ref Unsafe.As<byte, uint>(ref first), bitInByte, unchecked((uint)value), bitCount, bitOrder);
            return;
        }

        ref ulong target = ref Unsafe.As<byte, ulong>(ref first);
        if (bitCount + bitInByte > 64)
            BitPrimitives.WriteValue128(ref target, bitInByte, value, bitCount, bitOrder);
        else
            BitPrimitives.WriteValue64(ref target, bitInByte, value, bitCount, bitOrder);
    }
}
