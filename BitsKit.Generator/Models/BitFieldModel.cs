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

    public BitFieldModel(AttributeData attributeData, TypeSymbolProcessor? typeSymbol)
    {
        if (typeSymbol != null)
        {
            // todo: for now, analyser passes null
            // these fields don't matter for it

            _containingTypeIsStruct = typeSymbol.IsStruct;
            BitOrder = typeSymbol.DefaultBitOrder;
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
        string accessor = (Modifiers & BitFieldModifiers.AccessorMask) switch
        {
            BitFieldModifiers.Public => "public",
            BitFieldModifiers.Private => "private",
            BitFieldModifiers.Protected => "protected",
            BitFieldModifiers.Internal => "internal",
            BitFieldModifiers.ProtectedInternal => "protected internal",
            BitFieldModifiers.PrivateProtected => "private protected",
            _ => "public",
        };

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
