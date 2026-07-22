using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BitsKit.Generator.Models;

internal abstract record BitFieldModel
{
    public string Name { get; set; } = null!;
    public BitFieldType? FieldType { get; set; }
    public string? ReturnType { get; set; }
    public BackingFieldModel BackingField { get; set; } = null!;
    public BackingFieldType BackingFieldType => BackingField.Type;
    public int BitOffset { get; set; }
    public int BitCount { get; set; }
    public BitOrder BitOrder { get; set; }
    public bool ReverseBitOrder { get; }
    public BitFieldModifiers Modifiers { get; }

    private readonly bool _containingTypeIsStruct;
    protected bool UsesUnsafeAccess { get; }

    public BitFieldModel(AttributeData attributeData, TypeSymbolProcessor? typeSymbol)
    {
        if (typeSymbol != null)
        {
            // todo: for now, analyser passes null
            // these fields don't matter for it

            _containingTypeIsStruct = typeSymbol.IsStruct;
            BitOrder = typeSymbol.DefaultBitOrder;
            UsesUnsafeAccess = typeSymbol.AccessMode == BitObjectAccessMode.Unsafe;
        }

        for (int i = 0; i < attributeData.NamedArguments.Length; i++)
        {
            switch (attributeData.NamedArguments[i].Key)
            {
                case "ReverseBitOrder":
                    ReverseBitOrder = (bool)attributeData.NamedArguments[i].Value.Value!;
                    break;
                case "Modifiers":
                    Modifiers = (BitFieldModifiers)attributeData.NamedArguments[i].Value.Value!;
                    break;
            }
        }
    }

    public void GenerateCSharpSource(StringBuilder sb)
    {
        string accessor = GetAccessor();

        // property
        sb.AppendIndentedLine(2,
            GetPropertyTemplate(),
            accessor,
            Modifiers.HasFlag(BitFieldModifiers.Required) ? "required" : "",
            ReturnType ?? FieldType?.ToString(),
            Name)
          .AppendIndentedLine(2, "{");

        // getter
        {
            sb.AppendIndentedLine(3,
                GetGetterTemplate(),
                SupportsReadOnlyGetter() ? "readonly" : "",
                "get",
                FieldType?.ToIntegralName(),
                BitOrder.ToShortName(),
                BackingField.Name,
                BitOffset,
                BitCount,
                BackingField.FixedSize,
                BackingField.TypeString);
        }

        // setter
        if (!IsReadOnly())
        {
            sb.AppendIndentedLine(3,
                GetSetterTemplate(),
                "",
                Modifiers.HasFlag(BitFieldModifiers.InitOnly) ? "init" : "set",
                FieldType?.ToIntegralName(),
                BitOrder.ToShortName(),
                BackingField.Name,
                BitOffset,
                BitCount,
                BackingField.FixedSize,
                BackingField.TypeString);
        }

        sb.AppendIndentedLine(2, "}")
          .AppendLine();
    }

    public void GenerateBatchAccessors(StringBuilder sb)
    {
        string accessor = GetAccessor();
        string valueType = ReturnType ?? FieldType!.Value.ToString();
        string primitiveName = this is BooleanFieldModel ? "Bit" : FieldType!.Value.ToIntegralName();
        string bitCountArgument = this is BooleanFieldModel ? string.Empty : $", {BitCount}";
        string readDestination = this is EnumFieldModel
            ? $"MemoryMarshal.Cast<{valueType}, {FieldType!.Value}>(destination)"
            : "destination";
        string writeValues = this is EnumFieldModel
            ? $"MemoryMarshal.Cast<{valueType}, {FieldType!.Value}>(values)"
            : "values";

        sb.AppendIndentedLine(2,
            $"{accessor} static void Read{Name}Batch(ReadOnlySpan<Byte> source, Span<{valueType}> destination) =>")
          .AppendIndentedLine(3,
            $"BitBatchPrimitives.Read{primitiveName}{BitOrder.ToShortName()}(source, {BitOffset}{bitCountArgument}, {readDestination});")
          .AppendLine()
          .AppendIndentedLine(2,
            $"{accessor} static void Read{Name}Batch(ReadOnlySpan<Byte> source, Int32 bitStride, Span<{valueType}> destination) =>")
          .AppendIndentedLine(3,
            $"BitBatchPrimitives.Read{primitiveName}{BitOrder.ToShortName()}(source, {BitOffset}{bitCountArgument}, bitStride, {readDestination});")
          .AppendLine();

        if (IsReadOnly())
            return;

        sb.AppendIndentedLine(2,
            $"{accessor} static void Write{Name}Batch(Span<Byte> destination, ReadOnlySpan<{valueType}> values) =>")
          .AppendIndentedLine(3,
            $"BitBatchPrimitives.Write{primitiveName}{BitOrder.ToShortName()}(destination, {BitOffset}{bitCountArgument}, {writeValues});")
          .AppendLine()
          .AppendIndentedLine(2,
            $"{accessor} static void Write{Name}Batch(Span<Byte> destination, Int32 bitStride, ReadOnlySpan<{valueType}> values) =>")
          .AppendIndentedLine(3,
            $"BitBatchPrimitives.Write{primitiveName}{BitOrder.ToShortName()}(destination, {BitOffset}{bitCountArgument}, bitStride, {writeValues});")
          .AppendLine();
    }

    private string GetAccessor() => (Modifiers & BitFieldModifiers.AccessorMask) switch
    {
        BitFieldModifiers.Public => "public",
        BitFieldModifiers.Private => "private",
        BitFieldModifiers.Protected => "protected",
        BitFieldModifiers.Internal => "internal",
        BitFieldModifiers.ProtectedInternal => "protected internal",
        BitFieldModifiers.PrivateProtected => "private protected",
        _ => "public",
    };

    /// <summary>
    /// Generates a template for the property accessors, type and name
    /// <para>
    /// {0} = Accessor<br/>
    /// {1} = Required modifier<br/>
    /// {2} = <see cref="FieldType"/><br/>
    /// {3} = <see cref="Name"/>
    /// </para> 
    /// </summary>
    protected string GetPropertyTemplate()
    {
        return BackingFieldType switch
        {
            BackingFieldType.Integral or
            BackingFieldType.Memory or
            BackingFieldType.Span or
            BackingFieldType.InlineArray => StringConstants.PropertyTemplate,
            BackingFieldType.Pointer => StringConstants.UnsafePropertyTemplate,
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Generates a template for the property getter
    /// <para>
    /// {0} = Getter prefix<br/>
    /// {1} = Getter type<br/>
    /// {2} = BitPrimitives method<br/>
    /// {3} = <see cref="BitOrder"/><br/>
    /// {4} = IntegralName<br/>
    /// {5} = <see cref="BitOffset"/><br/> 
    /// {6} = <see cref="BitCount"/><br/>
    /// {7} = <see cref="BackingField"/>.FixedSize<br/>
    /// {8} = <see cref="BackingField"/>.Type
    /// </para> 
    /// </summary>
    protected abstract string GetGetterTemplate();

    /// <summary>
    /// Generates a template for the property setter
    /// <para>
    /// {0} = Setter prefix<br/>
    /// {1} = Setter type<br/>
    /// {2} = BitPrimitive method<br/>
    /// {3} = <see cref="BitOrder"/><br/>
    /// {4} = IntegralName<br/>
    /// {5} = <see cref="BitOffset"/><br/> 
    /// {6} = <see cref="BitCount"/><br/>
    /// {7} = <see cref="BackingField"/>.FixedSize<br/>
    /// {8} = <see cref="BackingField"/>.Type
    /// </para> 
    /// </summary>
    protected abstract string GetSetterTemplate();

    /// <summary>
    /// Gets the getter's source template based on it's BackingField
    /// </summary>
    protected string GetterSource() => BackingFieldType switch
    {
        BackingFieldType.Integral => "{4}",
        BackingFieldType.Memory => "{4}.Span",
        BackingFieldType.Span => "{4}",
        BackingFieldType.Pointer => "MemoryMarshal.CreateReadOnlySpan(ref {4}[0], {7})",
        BackingFieldType.InlineArray => "MemoryMarshal.AsBytes<{8}>(this)",
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Gets the setter's source template based on it's BackingField
    /// </summary>
    protected string SetterSource() => BackingFieldType switch
    {
        BackingFieldType.Integral => "ref {4}",
        BackingFieldType.Memory => "{4}.Span",
        BackingFieldType.Span => "{4}",
        BackingFieldType.Pointer => "MemoryMarshal.CreateSpan(ref {4}[0], {7})",
        BackingFieldType.InlineArray => "MemoryMarshal.AsBytes((Span<{8}>)this)",
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Creates an unchecked read through a raw reference for opted-in byte-addressable storage.
    /// </summary>
    protected bool TryGetUnsafeReadExpression(out string expression)
    {
        expression = string.Empty;
        if (UsesFasterCheckedStorageSpecialization())
            return false;

        if (!TryGetUnsafeStorageReference(writable: false, out string source))
            return false;

        expression = $"UnsafeBitPrimitives.Read{FieldType!.Value.ToIntegralName()}{BitOrder.ToShortName()}" +
                     $"({source}, {BitOffset}, {BitCount})";
        return true;
    }

    /// <summary>
    /// Creates an unchecked write through a raw reference for opted-in byte-addressable storage.
    /// </summary>
    protected bool TryGetUnsafeWriteExpression(string valueExpression, out string expression)
    {
        expression = string.Empty;
        if (UsesFasterCheckedStorageSpecialization())
            return false;

        if (!TryGetUnsafeStorageReference(writable: true, out string destination))
            return false;

        expression = $"UnsafeBitPrimitives.Write{FieldType!.Value.ToIntegralName()}{BitOrder.ToShortName()}" +
                     $"({destination}, {BitOffset}, unchecked(({FieldType.Value})({valueExpression})), {BitCount})";
        return true;
    }

    /// <summary>
    /// Creates an unchecked single-bit read through a raw reference.
    /// </summary>
    protected bool TryGetUnsafeBooleanReadExpression(out string expression)
    {
        expression = string.Empty;
        if (!TryGetUnsafeStorageTarget(writable: false, out string source))
            return false;

        int byteOffset = BitOffset >> 3;
        int bitInByte = BitOffset & 7;
        int mask = 1 << (BitOrder == BitOrder.MostSignificant ? 7 - bitInByte : bitInByte);
        string target = byteOffset == 0
            ? source
            : $"System.Runtime.CompilerServices.Unsafe.Add(ref {source}, {byteOffset})";
        expression = $"({target} & 0x{mask:X2}) != 0";
        return true;
    }

    /// <summary>
    /// Creates an unchecked single-bit write through a raw reference.
    /// </summary>
    protected bool TryGetUnsafeBooleanWriteTemplate(out string template)
    {
        template = string.Empty;
        if (!TryGetUnsafeStorageTarget(writable: true, out string destination))
            return false;

        int byteOffset = BitOffset >> 3;
        int bitInByte = BitOffset & 7;
        int mask = 1 << (BitOrder == BitOrder.MostSignificant ? 7 - bitInByte : bitInByte);
        string target = byteOffset == 0
            ? destination
            : $"System.Runtime.CompilerServices.Unsafe.Add(ref {destination}, {byteOffset})";
        template =
            "{0} {1}\n" +
            "{{\n" +
            $"    ref Byte target = ref {target};\n" +
            "    if (value)\n" +
            $"        target |= 0x{mask:X2};\n" +
            "    else\n" +
            $"        target &= 0x{255 ^ mask:X2};\n" +
            "}}";
        return true;
    }

    private bool TryGetUnsafeStorageReference(bool writable, out string reference)
    {
        reference = string.Empty;
        if (!TryGetUnsafeStorageTarget(writable, out string target))
            return false;

        reference = $"ref {target}";
        return true;
    }

    private bool TryGetUnsafeStorageTarget(bool writable, out string target)
    {
        target = string.Empty;
        if (!UsesUnsafeAccess || BackingFieldType == BackingFieldType.Integral)
            return false;

        string source = BackingField.TypeString == "byte[]"
            ? writable ? "((Span<Byte>){4})" : "((ReadOnlySpan<Byte>){4})"
            : writable ? SetterSource() : GetterSource();
        target = $"MemoryMarshal.GetReference({source})";
        return true;
    }

    private bool UsesFasterCheckedStorageSpecialization()
    {
        int width = FieldType switch
        {
            BitFieldType.SByte or BitFieldType.Byte => 8,
            BitFieldType.Int16 or BitFieldType.UInt16 => 16,
            BitFieldType.Int32 or BitFieldType.UInt32 => 32,
            BitFieldType.Int64 or BitFieldType.UInt64 => 64,
            _ => 0
        };

        return UsesUnsafeAccess &&
               BackingFieldType is BackingFieldType.Memory or BackingFieldType.Span or BackingFieldType.InlineArray &&
               width != 0 &&
               BitCount == width &&
               (BitOffset & 7) == 0;
    }

    /// <summary>
    /// Creates a direct endian-aware read for byte-aligned, full-width byte storage.
    /// </summary>
    protected bool TryGetDirectStorageReadExpression(out string expression)
    {
        expression = string.Empty;

        if (!TryGetDirectStorageInfo(writable: false, out int width, out string typeName, out string source))
            return false;

        if (width == 8)
        {
            expression = $"unchecked(({typeName}){source}[0])";
            return true;
        }

        string endianness = BitOrder == BitOrder.MostSignificant ? "BigEndian" : "LittleEndian";
        expression = $"BinaryPrimitives.Read{typeName}{endianness}({source})";
        return true;
    }

    /// <summary>
    /// Creates a direct endian-aware write for byte-aligned, full-width byte storage.
    /// </summary>
    protected bool TryGetDirectStorageWriteExpression(string valueExpression, out string expression)
    {
        expression = string.Empty;

        if (!TryGetDirectStorageInfo(writable: true, out int width, out string typeName, out string source))
            return false;

        string value = $"unchecked(({typeName})({valueExpression}))";
        if (width == 8)
        {
            expression = $"{source}[0] = unchecked((Byte){value})";
            return true;
        }

        string endianness = BitOrder == BitOrder.MostSignificant ? "BigEndian" : "LittleEndian";
        expression = $"BinaryPrimitives.Write{typeName}{endianness}({source}, {value})";
        return true;
    }

    private bool TryGetDirectStorageInfo(bool writable, out int width, out string typeName, out string source)
    {
        width = FieldType switch
        {
            BitFieldType.SByte or BitFieldType.Byte => 8,
            BitFieldType.Int16 or BitFieldType.UInt16 => 16,
            BitFieldType.Int32 or BitFieldType.UInt32 => 32,
            BitFieldType.Int64 or BitFieldType.UInt64 => 64,
            _ => 0
        };
        typeName = FieldType?.ToString() ?? string.Empty;

        if (!TryGetByteStorageSource(writable, out source))
            return false;

        int byteOffset = BitOffset >> 3;
        if (byteOffset != 0)
            source += $".Slice({byteOffset})";

        return width != 0 &&
               BitCount == width &&
               (BitOffset & 7) == 0;
    }

    private bool TryGetByteStorageSource(bool writable, out string source)
    {
        source = BackingFieldType switch
        {
            BackingFieldType.Memory => "{4}.Span",
            BackingFieldType.Span when BackingField.TypeString == "byte[]" =>
                writable ? "((Span<Byte>){4})" : "((ReadOnlySpan<Byte>){4})",
            BackingFieldType.Span => "{4}",
            BackingFieldType.InlineArray when BackingField.TypeString == "byte" =>
                writable ? "((Span<Byte>)this)" : "((ReadOnlySpan<Byte>)this)",
            _ => string.Empty
        };

        return source.Length != 0;
    }

    /// <summary>
    /// Creates a direct byte test for boolean fields in byte-addressable storage.
    /// </summary>
    protected bool TryGetDirectStorageBooleanReadExpression(out string expression)
    {
        expression = string.Empty;
        if (BackingFieldType == BackingFieldType.Memory)
            return false;

        if (!TryGetByteStorageSource(writable: false, out string source))
            return false;

        int byteOffset = BitOffset >> 3;
        int bitInByte = BitOffset & 7;
        int mask = 1 << (BitOrder == BitOrder.MostSignificant ? 7 - bitInByte : bitInByte);
        expression = $"({source}[{byteOffset}] & 0x{mask:X2}) != 0";
        return true;
    }

    /// <summary>
    /// Creates a direct read-modify-write for boolean fields in byte-addressable storage.
    /// </summary>
    protected bool TryGetDirectStorageBooleanWriteTemplate(out string template)
    {
        template = string.Empty;
        if (BackingFieldType == BackingFieldType.Memory)
            return false;

        if (!TryGetByteStorageSource(writable: true, out string source))
            return false;

        int byteOffset = BitOffset >> 3;
        int bitInByte = BitOffset & 7;
        int mask = 1 << (BitOrder == BitOrder.MostSignificant ? 7 - bitInByte : bitInByte);
        if (BackingFieldType == BackingFieldType.Span)
        {
            template =
                "{0} {1}\n" +
                "{{\n" +
                "    if (value)\n" +
                $"        {source}[{byteOffset}] |= 0x{mask:X2};\n" +
                "    else\n" +
                $"        {source}[{byteOffset}] &= 0x{255 ^ mask:X2};\n" +
                "}}";
            return true;
        }

        template =
            "{0} {1}\n" +
            "{{\n" +
            $"    Span<Byte> source = {source};\n" +
            $"    source[{byteOffset}] = unchecked((Byte)((source[{byteOffset}] & 0x{255 ^ mask:X2}) | " +
            $"(value ? 0x{mask:X2} : 0)));\n" +
            "}}";
        return true;
    }

    /// <summary>
    /// Creates a fixed-window read for frequently used multi-byte field widths.
    /// </summary>
    protected bool TryGetDirectFixedWidthReadTemplate(out string template)
    {
        template = string.Empty;
        if (!TryGetFixedWidthStorageInfo(
            writable: false,
            out string source,
            out int byteCount,
            out int shift,
            out string typeName))
        {
            return false;
        }

        int loadWidth = byteCount <= 4 ? 4 : 8;
        string fastWindow =
            $"unchecked((UInt64)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<UInt{loadWidth * 8}>(" +
            "ref MemoryMarshal.GetReference(source)))";
        if (BitOrder == BitOrder.MostSignificant)
            fastWindow = $"BinaryPrimitives.ReverseEndianness({fastWindow}) >> {64 - loadWidth * 8}";

        string returnType = ReturnType ?? typeName;
        int fastShift = BitOrder == BitOrder.MostSignificant ?
            loadWidth * 8 - (BitOffset & 7) - BitCount :
            shift;
        string fastResult = GetFixedWidthResultExpression(returnType, "current", fastShift);
        string exactResult = GetFixedWidthResultExpression(returnType, "current", shift);
        string exactWindow = GetWindowReadExpression("source", byteCount, BitOrder);

        template =
            "{0} {1}\n" +
            "{{\n" +
            $"    ReadOnlySpan<Byte> source = {source};\n" +
            $"    if (source.Length < {loadWidth})\n" +
            "        return ReadExact(source);\n" +
            $"    UInt64 current = {fastWindow};\n" +
            $"    return {fastResult};\n" +
            "\n" +
            "    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\n" +
            $"    static {returnType} ReadExact(ReadOnlySpan<Byte> source)\n" +
            "    {{\n" +
            $"        source = source.Slice(0, {byteCount});\n" +
            $"        UInt64 current = {exactWindow};\n" +
            $"        return {exactResult};\n" +
            "    }}\n" +
            "}}";
        return true;
    }

    /// <summary>
    /// Creates a fixed-window read-modify-write for frequently used multi-byte field widths.
    /// </summary>
    protected bool TryGetDirectFixedWidthWriteTemplate(string valueExpression, out string template)
    {
        template = string.Empty;
        if (!TryGetFixedWidthStorageInfo(
            writable: true,
            out string source,
            out int byteCount,
            out int shift,
            out _))
        {
            return false;
        }

        ulong valueMask = (1UL << BitCount) - 1;
        int loadWidth = byteCount <= 4 ? 4 : 8;
        int fastShift = BitOrder == BitOrder.MostSignificant ?
            loadWidth * 8 - (BitOffset & 7) - BitCount :
            shift;
        ulong fastFieldMask = valueMask << fastShift;
        ulong exactFieldMask = valueMask << shift;
        string fastRead =
            $"unchecked((UInt64)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<UInt{loadWidth * 8}>(" +
            "ref MemoryMarshal.GetReference(source)))";
        string fastWriteValue = "current";
        if (BitOrder == BitOrder.MostSignificant)
        {
            fastRead = $"BinaryPrimitives.ReverseEndianness({fastRead}) >> {64 - loadWidth * 8}";
            fastWriteValue =
                $"BinaryPrimitives.ReverseEndianness(current << {64 - loadWidth * 8})";
        }

        string typeName = FieldType!.Value.ToString();
        string exactRead = GetWindowReadExpression("source", byteCount, BitOrder);
        string exactWrites = GetWindowWriteStatements("source", "current", byteCount, BitOrder);

        template =
            "{0} {1}\n" +
            "{{\n" +
            $"    Span<Byte> source = {source};\n" +
            $"    if (source.Length < {loadWidth})\n" +
            "    {{\n" +
            $"        WriteExact(source, unchecked(({typeName})({valueExpression})));\n" +
            "        return;\n" +
            "    }}\n" +
            $"    UInt64 current = {fastRead};\n" +
            $"    current = (current & ~0x{fastFieldMask:X}UL) | " +
            $"((unchecked((UInt64)({valueExpression})) << {fastShift}) & 0x{fastFieldMask:X}UL);\n" +
            $"    System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(source), unchecked((UInt{loadWidth * 8})({fastWriteValue})));\n" +
            "\n" +
            "    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\n" +
            $"    static void WriteExact(Span<Byte> source, {typeName} value)\n" +
            "    {{\n" +
            $"        source = source.Slice(0, {byteCount});\n" +
            $"        UInt64 current = {exactRead};\n" +
            $"        current = (current & ~0x{exactFieldMask:X}UL) | " +
            $"((unchecked((UInt64)(value)) << {shift}) & 0x{exactFieldMask:X}UL);\n" +
            $"        {exactWrites}\n" +
            "    }}\n" +
            "}}";
        return true;
    }

    private string GetFixedWidthResultExpression(string returnType, string value, int shift)
    {
        ulong valueMask = (1UL << BitCount) - 1;
        string extracted = $"(({value} >> {shift}) & 0x{valueMask:X}UL)";

        if (FieldType is BitFieldType.SByte or
            BitFieldType.Int16 or
            BitFieldType.Int32 or
            BitFieldType.Int64)
        {
            int signShift = 64 - BitCount;
            return $"unchecked(({returnType})(unchecked((Int64)({extracted} << {signShift})) >> {signShift}))";
        }

        return $"unchecked(({returnType}){extracted})";
    }

    private bool TryGetFixedWidthStorageInfo(
        bool writable,
        out string source,
        out int byteCount,
        out int shift,
        out string typeName)
    {
        source = string.Empty;
        byteCount = 0;
        shift = 0;
        typeName = FieldType?.ToString() ?? string.Empty;

        if (BitCount is not (11 or 12 or 24 or 48) ||
            FieldType is not (BitFieldType.SByte or
                              BitFieldType.Byte or
                              BitFieldType.Int16 or
                              BitFieldType.UInt16 or
                              BitFieldType.Int32 or
                              BitFieldType.UInt32 or
                              BitFieldType.Int64 or
                              BitFieldType.UInt64) ||
            !TryGetByteStorageSource(writable, out source))
        {
            return false;
        }

        int bitInByte = BitOffset & 7;
        byteCount = (bitInByte + BitCount + 7) >> 3;
        int byteOffset = BitOffset >> 3;
        if (byteOffset != 0)
            source += $".Slice({byteOffset})";
        shift = BitOrder == BitOrder.MostSignificant ?
            byteCount * 8 - bitInByte - BitCount :
            bitInByte;
        return true;
    }

    private static string GetWindowReadExpression(string source, int byteCount, BitOrder bitOrder)
    {
        if (bitOrder == BitOrder.LeastSignificant)
        {
            return byteCount switch
            {
                2 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt16LittleEndian({source}))",
                3 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt16LittleEndian({source}) | ((UInt64){source}[2] << 16))",
                4 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt32LittleEndian({source}))",
                6 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt32LittleEndian({source}) | ((UInt64)BinaryPrimitives.ReadUInt16LittleEndian({source}.Slice(4)) << 32))",
                7 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt32LittleEndian({source}) | ((UInt64)BinaryPrimitives.ReadUInt16LittleEndian({source}.Slice(4)) << 32) | ((UInt64){source}[6] << 48))",
                _ => throw new NotSupportedException()
            };
        }

        return byteCount switch
        {
            2 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt16BigEndian({source}))",
            3 => $"unchecked(((UInt64)BinaryPrimitives.ReadUInt16BigEndian({source}) << 8) | {source}[2])",
            4 => $"unchecked((UInt64)BinaryPrimitives.ReadUInt32BigEndian({source}))",
            6 => $"unchecked(((UInt64)BinaryPrimitives.ReadUInt32BigEndian({source}) << 16) | BinaryPrimitives.ReadUInt16BigEndian({source}.Slice(4)))",
            7 => $"unchecked(((UInt64)BinaryPrimitives.ReadUInt32BigEndian({source}) << 24) | ((UInt64)BinaryPrimitives.ReadUInt16BigEndian({source}.Slice(4)) << 8) | {source}[6])",
            _ => throw new NotSupportedException()
        };
    }

    private static string GetWindowWriteStatements(
        string source,
        string value,
        int byteCount,
        BitOrder bitOrder)
    {
        if (bitOrder == BitOrder.LeastSignificant)
        {
            return byteCount switch
            {
                2 => $"BinaryPrimitives.WriteUInt16LittleEndian({source}, unchecked((UInt16){value}));",
                3 => $"BinaryPrimitives.WriteUInt16LittleEndian({source}, unchecked((UInt16){value})); {source}[2] = unchecked((Byte)({value} >> 16));",
                4 => $"BinaryPrimitives.WriteUInt32LittleEndian({source}, unchecked((UInt32){value}));",
                6 => $"BinaryPrimitives.WriteUInt32LittleEndian({source}, unchecked((UInt32){value})); BinaryPrimitives.WriteUInt16LittleEndian({source}.Slice(4), unchecked((UInt16)({value} >> 32)));",
                7 => $"BinaryPrimitives.WriteUInt32LittleEndian({source}, unchecked((UInt32){value})); BinaryPrimitives.WriteUInt16LittleEndian({source}.Slice(4), unchecked((UInt16)({value} >> 32))); {source}[6] = unchecked((Byte)({value} >> 48));",
                _ => throw new NotSupportedException()
            };
        }

        return byteCount switch
        {
            2 => $"BinaryPrimitives.WriteUInt16BigEndian({source}, unchecked((UInt16){value}));",
            3 => $"BinaryPrimitives.WriteUInt16BigEndian({source}, unchecked((UInt16)({value} >> 8))); {source}[2] = unchecked((Byte){value});",
            4 => $"BinaryPrimitives.WriteUInt32BigEndian({source}, unchecked((UInt32){value}));",
            6 => $"BinaryPrimitives.WriteUInt32BigEndian({source}, unchecked((UInt32)({value} >> 16))); BinaryPrimitives.WriteUInt16BigEndian({source}.Slice(4), unchecked((UInt16){value}));",
            7 => $"BinaryPrimitives.WriteUInt32BigEndian({source}, unchecked((UInt32)({value} >> 24))); BinaryPrimitives.WriteUInt16BigEndian({source}.Slice(4), unchecked((UInt16)({value} >> 8))); {source}[6] = unchecked((Byte){value});",
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    /// Creates a specialized scalar read for fixed-width integral backing fields.
    /// </summary>
    protected bool TryGetDirectIntegralReadExpression(out string expression)
    {
        expression = string.Empty;

        if (!TryGetDirectIntegralInfo(
            out _,
            out int workingWidth,
            out string unsignedType))
        {
            return false;
        }

        string backingType = FieldType!.Value.ToString();
        string unsignedSource = $"unchecked(({unsignedType}){{4}})";
        if (BitOrder == BitOrder.MostSignificant)
        {
            string extracted =
                $"(BinaryPrimitives.ReverseEndianness({unsignedSource}) << {BitOffset}) >> {workingWidth - BitCount}";

            if (FieldType is BitFieldType.SByte or
                BitFieldType.Int16 or
                BitFieldType.Int32 or
                BitFieldType.Int64)
            {
                string signedType = workingWidth == 64 ? "Int64" : "Int32";
                int signShift = workingWidth - BitCount;
                expression =
                    $"unchecked(({backingType})((unchecked(({signedType})({extracted})) << {signShift}) >> {signShift}))";
            }
            else
            {
                expression = $"unchecked(({backingType})({extracted}))";
            }

            return true;
        }

        if (FieldType is BitFieldType.SByte or
            BitFieldType.Int16 or
            BitFieldType.Int32 or
            BitFieldType.Int64)
        {
            string signedType = workingWidth == 64 ? "Int64" : "Int32";
            int leftShift = workingWidth - BitOffset - BitCount;
            int rightShift = workingWidth - BitCount;
            expression =
                $"unchecked(({backingType})(unchecked(({signedType})({unsignedSource} << {leftShift})) >> {rightShift}))";
        }
        else
        {
            ulong valueMask = BitCount == 64 ? ulong.MaxValue : (1UL << BitCount) - 1;
            string mask = FormatMask(valueMask, workingWidth);
            expression =
                $"unchecked(({backingType})(({unsignedSource} >> {BitOffset}) & {mask}))";
        }

        return true;
    }

    /// <summary>
    /// Creates a specialized scalar bit test for integral backing fields.
    /// </summary>
    protected bool TryGetDirectIntegralBooleanReadExpression(out string expression)
    {
        expression = string.Empty;

        if (!TryGetDirectIntegralInfo(
            out _,
            out int workingWidth,
            out string unsignedType))
        {
            return false;
        }

        if (BitOrder == BitOrder.MostSignificant)
        {
            expression =
                $"((BinaryPrimitives.ReverseEndianness(unchecked(({unsignedType}){{4}})) << {BitOffset}) >> {workingWidth - 1}) != 0";
        }
        else
        {
            string mask = FormatMask(1UL << BitOffset, workingWidth);
            expression = $"(unchecked(({unsignedType}){{4}}) & {mask}) != 0";
        }

        return true;
    }

    /// <summary>
    /// Creates a specialized scalar assignment for fixed-width integral backing fields.
    /// </summary>
    protected bool TryGetDirectIntegralWriteExpression(string valueExpression, out string expression)
    {
        expression = string.Empty;

        if (!TryGetDirectIntegralInfo(
            out int backingWidth,
            out int workingWidth,
            out string unsignedType))
        {
            return false;
        }

        ulong valueMask = BitCount == 64 ? ulong.MaxValue : (1UL << BitCount) - 1;
        string valueMaskLiteral = FormatMask(valueMask, workingWidth);
        string backingType = FieldType!.Value.ToString();

        if (BitOrder == BitOrder.MostSignificant)
        {
            ulong shiftedMask = 0;
            for (int i = 0; i < BitCount; i++)
            {
                int logicalBit = BitOffset + i;
                int physicalBit = (logicalBit & ~7) + (7 - (logicalBit & 7));
                shiftedMask |= 1UL << physicalBit;
            }

            string fieldMask = FormatMask(shiftedMask, workingWidth);
            int shift = backingWidth - BitCount - BitOffset;
            string value = $"unchecked(({unsignedType})({valueExpression})) & {valueMaskLiteral}";
            string alignedValue = backingWidth switch
            {
                8 => $"({value}) << {shift}",
                16 => $"unchecked((UInt32)BinaryPrimitives.ReverseEndianness(unchecked((UInt16)(({value}) << {shift}))))",
                32 => $"BinaryPrimitives.ReverseEndianness(({value}) << {shift})",
                64 => $"BinaryPrimitives.ReverseEndianness(({value}) << {shift})",
                _ => throw new NotSupportedException()
            };

            expression =
                $"{{4}} = unchecked(({backingType})((unchecked(({unsignedType}){{4}}) & ~{fieldMask}) | {alignedValue}))";
            return true;
        }

        int lsbShift = BitOffset;
        ulong lsbShiftedMask = valueMask << lsbShift;
        string lsbFieldMask = FormatMask(lsbShiftedMask, workingWidth);

        expression =
            $"{{4}} = unchecked(({backingType})((unchecked(({unsignedType}){{4}}) & ~{lsbFieldMask}) | " +
            $"((unchecked(({unsignedType})({valueExpression})) << {lsbShift}) & {lsbFieldMask})))";
        return true;
    }

    private bool TryGetDirectIntegralInfo(
        out int backingWidth,
        out int workingWidth,
        out string unsignedType)
    {
        backingWidth = FieldType switch
        {
            BitFieldType.SByte or BitFieldType.Byte => 8,
            BitFieldType.Int16 or BitFieldType.UInt16 => 16,
            BitFieldType.Int32 or BitFieldType.UInt32 => 32,
            BitFieldType.Int64 or BitFieldType.UInt64 => 64,
            _ => 0
        };
        workingWidth = backingWidth == 64 ? 64 : 32;
        unsignedType = workingWidth == 64 ? "UInt64" : "UInt32";

        return BackingFieldType == BackingFieldType.Integral &&
               backingWidth != 0 &&
               BitCount > 0 &&
               BitOffset >= 0 &&
               BitOffset + BitCount <= backingWidth;
    }

    private static string FormatMask(ulong mask, int width) =>
        width == 64 ? $"0x{mask:X}UL" : $"0x{mask:X}U";

    /// <summary>
    /// Determines if this field is ReadOnly based on it's BackingField and Modifiers
    /// </summary>
    protected bool IsReadOnly()
    {
        string backingType = BackingField.TypeString;

        return BackingField.IsReadOnly ||
               backingType == "System.ReadOnlySpan<byte>" ||
               backingType == "System.ReadOnlyMemory<byte>" ||
               Modifiers.HasFlag(BitFieldModifiers.ReadOnly);
    }

    /// <summary>
    /// Determines if this property can have a readonly instance member
    /// </summary>
    /// <returns></returns>
    private bool SupportsReadOnlyGetter()
    {
        return _containingTypeIsStruct &&
               BackingFieldType != BackingFieldType.Pointer &&
               BackingFieldType != BackingFieldType.InlineArray &&
               !IsReadOnly();
    }
}
