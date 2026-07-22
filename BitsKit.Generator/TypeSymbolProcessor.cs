using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitsKit.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BitsKit.Generator;

internal sealed record TypeSymbolProcessor
{
    public EquatableReadOnlyList<BitFieldModel> Fields { get; }
    public string? Namespace { get; }

    public BitOrder DefaultBitOrder { get; }
    public BitObjectAccessMode AccessMode { get; }
    public bool GenerateBatchAccessors { get; }
    public bool IsStruct { get; }
    public bool IsInlineArray { get; }
    public bool IsValid { get; }
    public string HintName { get; }

    private readonly string _syntaxKeyword;
    private readonly string _syntaxIdentifier;

    public TypeSymbolProcessor(INamedTypeSymbol typeSymbol, AttributeData attribute)
    {
        _syntaxKeyword = typeSymbol.TypeKind switch
        {
            TypeKind.Struct when typeSymbol.IsRecord => "record struct",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            TypeKind.Class when typeSymbol.IsRecord => "record",
            _ => "class"
        };
        _syntaxIdentifier = SymbolFormatting.GetTypeDeclarationIdentifier(typeSymbol);

        Namespace = SymbolFormatting.GetNamespace(typeSymbol.ContainingNamespace);
        if (string.IsNullOrWhiteSpace(Namespace)) Namespace = null;

        bool hasValidBitOrder = TryGetBitOrder(attribute, out BitOrder bitOrder);
        bool hasValidAccessMode = TryGetAccessMode(attribute, out BitObjectAccessMode accessMode);
        IsValid = hasValidBitOrder && hasValidAccessMode;
        DefaultBitOrder = bitOrder;
        AccessMode = accessMode;
        GenerateBatchAccessors = GetGenerateBatchAccessors(attribute);
        IsStruct = typeSymbol.TypeKind == TypeKind.Struct;
        IsInlineArray = HasInlineArrayAttribute(typeSymbol);
        HintName = CreateHintName(typeSymbol);

        Fields = IsValid
            ? EnumerateFields(typeSymbol)
            : new List<BitFieldModel>().ToEquatableReadOnlyList();
    }

    internal static bool TryGetBitOrder(AttributeData attribute, out BitOrder bitOrder)
    {
        bitOrder = BitOrder.LeastSignificant;
        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not int value ||
            value is < (int)BitOrder.LeastSignificant or > (int)BitOrder.MostSignificant)
        {
            return false;
        }

        bitOrder = (BitOrder)value;
        return true;
    }

    internal static bool TryGetAccessMode(AttributeData attribute, out BitObjectAccessMode accessMode)
    {
        accessMode = BitObjectAccessMode.Checked;
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == "AccessMode" && argument.Value.Value is int value)
            {
                if (value is < (int)BitObjectAccessMode.Checked or > (int)BitObjectAccessMode.Unsafe)
                    return false;

                accessMode = (BitObjectAccessMode)value;
                return true;
            }
        }

        return true;
    }

    private static bool GetGenerateBatchAccessors(AttributeData attribute)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == "GenerateBatchAccessors" && argument.Value.Value is bool value)
                return value;
        }

        return false;
    }

    public void GenerateCSharpSource(StringBuilder sb)
    {
        sb.AppendIndentedLine(1,
            StringConstants.TypeDeclarationTemplate,
            _syntaxKeyword,
            _syntaxIdentifier)
          .AppendIndentedLine(1, "{");

        foreach (BitFieldModel field in Fields)
            field.GenerateCSharpSource(sb);

        if (GenerateBatchAccessors)
        {
            foreach (BitFieldModel field in Fields)
                field.GenerateBatchAccessors(sb);
        }

        sb.RemoveLastLine()
          .AppendIndentedLine(1, "}")
          .AppendLine();
    }

    private EquatableReadOnlyList<BitFieldModel> EnumerateFields(ITypeSymbol typeSymbol)
    {
        var output = new List<BitFieldModel>();
        var reservedNames = new HashSet<string>(typeSymbol.GetMembers().Select(member => member.Name));
        reservedNames.Add(typeSymbol.Name);

        foreach (IFieldSymbol field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!IsValidFieldSymbol(field))
                continue;

            BackingFieldType backingType = BackingFieldModel.Classify(field);
            if (backingType == BackingFieldType.Integral && IsInlineArray)
                backingType = BackingFieldType.InlineArray;

            if (backingType == BackingFieldType.Invalid)
                continue;

            var backingModel = new BackingFieldModel(field, backingType);
            if (backingModel.IsRawPointer && AccessMode != BitObjectAccessMode.Unsafe)
                continue;

            CreateBitFieldModels(output, field, backingModel, reservedNames);
        }

        return output.ToEquatableReadOnlyList();
    }

    private void CreateBitFieldModels(
        List<BitFieldModel> output,
        IFieldSymbol backingField,
        BackingFieldModel backingModel,
        HashSet<string> reservedNames)
    {
        int offset = 0;

        foreach (AttributeData attribute in backingField.GetAttributes())
        {
            BitFieldModel? bitField = CreateBitFieldFromAttribute(attribute, this);

            if (bitField == null)
                continue;

            bitField.BackingField = backingModel;
            bitField.BitOffset = offset;

            // padding fields are not generated
            if (bitField is not { FieldType: BitFieldType.Padding })
            {
                BitFieldType? declaredFieldType = bitField.FieldType;
                // invert the bit order if necessary
                if (bitField.ReverseBitOrder)
                    bitField.BitOrder ^= BitOrder.MostSignificant;

                // integrals inherit their field type from their backing field
                if (backingModel.Type == BackingFieldType.Integral)
                    bitField.FieldType = backingField.Type.SpecialType.ToBitFieldType();

                // allow inline arrays to infer their type
                if (backingModel.Type == BackingFieldType.InlineArray)
                    bitField.FieldType ??= backingField.Type.SpecialType.ToBitFieldType();

                // Diagnostics are reported by analyzers. Do not generate an invalid
                // property when its primitive type could not be resolved.
                if (bitField.FieldType is null)
                {
                    offset += bitField.BitCount;
                    continue;
                }

                if (!IsSupportedFieldType(bitField.FieldType.Value) ||
                    (bitField is EnumFieldModel &&
                     (declaredFieldType is null ||
                      bitField.BitCount > declaredFieldType.Value.GetBitWidth())) ||
                    !SymbolFormatting.IsValidGeneratedName(bitField.Name) ||
                    !HasValidModifiers(bitField, backingModel))
                {
                    offset += bitField.BitCount;
                    continue;
                }

                string memberName = bitField.Name.StartsWith("@")
                    ? bitField.Name.Substring(1)
                    : bitField.Name;
                if (!reservedNames.Add(memberName))
                {
                    offset += bitField.BitCount;
                    continue;
                }

                if (!IsValidLayout(bitField, backingField, backingModel, offset))
                {
                    offset += bitField.BitCount;
                    continue;
                }

                // add to list of fields to generate
                output.Add(bitField);
            }

            offset += bitField.BitCount;
        }
    }

    public static BitFieldModel? CreateBitFieldFromAttribute(AttributeData attribute, TypeSymbolProcessor? processor)
    {
        string? attributeType = attribute.AttributeClass?.ToDisplayString();

        BitFieldModel? model = attributeType switch
        {
            StringConstants.BitFieldAttributeFullName => new IntegralFieldModel(attribute, processor),
            StringConstants.BooleanFieldAttributeFullName => new BooleanFieldModel(attribute, processor),
            StringConstants.EnumFieldAttributeFullName => new EnumFieldModel(attribute, processor),
            _ => null
        };

        if (model is not null)
            return model;

        for (INamedTypeSymbol? current = attribute.AttributeClass?.BaseType;
             current is not null;
             current = current.BaseType)
        {
            if (current.ToDisplayString() == StringConstants.BitFieldAttributeFullName)
                return new IntegralFieldModel(attribute, processor);
        }

        return null;
    }

    private static bool HasInlineArrayAttribute(ITypeSymbol typeSymbol)
    {
        return (int?)typeSymbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == StringConstants.InlineArrayAttributeFullName)?
            .ConstructorArguments[0].Value > 0;
    }

    private static bool IsSupportedFieldType(BitFieldType fieldType) => fieldType is
        BitFieldType.SByte or BitFieldType.Byte or
        BitFieldType.Int16 or BitFieldType.UInt16 or
        BitFieldType.Int32 or BitFieldType.UInt32 or
        BitFieldType.Int64 or BitFieldType.UInt64 or
        BitFieldType.IntPtr or BitFieldType.UIntPtr or
        BitFieldType.Boolean;

    private bool HasValidModifiers(BitFieldModel bitField, BackingFieldModel backing)
    {
        const BitFieldModifiers knownModifiers =
            BitFieldModifiers.AccessorMask |
            BitFieldModifiers.ReadOnly |
            BitFieldModifiers.InitOnly |
            BitFieldModifiers.Required;
        if ((bitField.Modifiers & ~knownModifiers) != 0)
            return false;

        if (IsStruct &&
            (bitField.Modifiers & BitFieldModifiers.AccessorMask) is
                BitFieldModifiers.Protected or
                BitFieldModifiers.ProtectedInternal or
                BitFieldModifiers.PrivateProtected)
        {
            return false;
        }

        return !bitField.Modifiers.HasFlag(BitFieldModifiers.Required) ||
               (!bitField.Modifiers.HasFlag(BitFieldModifiers.ReadOnly) &&
                !backing.IsReadOnly &&
                !backing.IsReadOnlyStorage);
    }

    private bool IsValidLayout(
        BitFieldModel bitField,
        IFieldSymbol backingField,
        BackingFieldModel backingModel,
        int offset)
    {
        if (bitField.BitCount <= 0 || bitField.BitCount > bitField.FieldType!.Value.GetBitWidth())
            return false;

        int capacity = backingModel.Type switch
        {
            BackingFieldType.Integral => backingField.Type.SpecialType.GetBitWidth(),
            BackingFieldType.Pointer when backingModel.FixedSize > 0 => backingModel.FixedSize * 8,
            BackingFieldType.InlineArray => GetInlineArrayLength(backingField.ContainingType) * backingField.Type.SpecialType.GetBitWidth(),
            _ => int.MaxValue
        };

        return offset <= capacity - bitField.BitCount;
    }

    private static int GetInlineArrayLength(ITypeSymbol typeSymbol) =>
        (int?)typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == StringConstants.InlineArrayAttributeFullName)?
            .ConstructorArguments[0].Value ?? 0;

    private static string CreateHintName(INamedTypeSymbol typeSymbol)
    {
        string identity = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        uint hash = 2166136261;
        foreach (char value in identity)
        {
            hash ^= value;
            hash *= 16777619;
        }

        string safeName = new(typeSymbol.MetadataName
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray());
        return $"BitsKit.{safeName}.{hash:X8}.g.cs";
    }

    private static bool IsValidFieldSymbol(IFieldSymbol member) => member is
    {
        CanBeReferencedByName: true,
        IsConst: false,
        IsStatic: false,
        IsImplicitlyDeclared: false,
        Type: { }
    };
}
