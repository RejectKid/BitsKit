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
        HintName = CreateHintName(typeSymbol);

        Fields = IsValid
            ? EnumerateFields(typeSymbol)
            : default;
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
        int inlineArrayLength = typeSymbol.GetInlineArrayLength();

        foreach (IFieldSymbol field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!IsValidFieldSymbol(field))
                continue;

            BackingFieldType backingType = BackingFieldModel.Classify(field, inlineArrayLength);
            if (backingType == BackingFieldType.Invalid)
                continue;

            var backingModel = new BackingFieldModel(field, backingType);
            if (backingModel.IsRawPointer && AccessMode != BitObjectAccessMode.Unsafe)
                continue;

            CreateBitFieldModels(output, field, backingModel, inlineArrayLength, reservedNames);
        }

        return output.ToEquatableReadOnlyList();
    }

    private void CreateBitFieldModels(
        List<BitFieldModel> output,
        IFieldSymbol backingField,
        BackingFieldModel backingModel,
        int inlineArrayLength,
        HashSet<string> reservedNames)
    {
        SpecialType storageType = backingField.Type.SpecialType;
        int capacity = backingModel.GetCapacity(storageType, inlineArrayLength);
        int offset = 0;

        foreach (AttributeData attribute in backingField.GetAttributes())
        {
            BitFieldModel? bitField = CreateBitFieldFromAttribute(attribute, this);
            if (bitField is null)
                continue;

            bitField.BitOffset = offset;
            offset += bitField.BitCount;

            if (bitField.FieldType == BitFieldType.Padding)
                continue;

            bitField.BackingField = backingModel;
            if (bitField.ReverseBitOrder)
                bitField.BitOrder ^= BitOrder.MostSignificant;

            BitFieldType? declaredFieldType = bitField.FieldType;
            bitField.FieldType = backingModel.ResolveFieldType(declaredFieldType, storageType);

            // The analyzer reports invalid fields. Skip emitting them here.
            int fieldWidth = bitField.FieldType?.GetBitWidth() ?? 0;
            if (fieldWidth == 0 ||
                (bitField is EnumFieldModel &&
                 (declaredFieldType is null || bitField.BitCount > declaredFieldType.Value.GetBitWidth())) ||
                !SymbolFormatting.IsValidGeneratedName(bitField.Name) ||
                bitField.GetInvalidModifierReason(backingModel, IsStruct) is not null)
            {
                continue;
            }

            string memberName = bitField.Name.StartsWith("@")
                ? bitField.Name.Substring(1)
                : bitField.Name;
            if (!reservedNames.Add(memberName))
                continue;

            if (bitField.BitCount <= 0 || bitField.BitCount > fieldWidth ||
                bitField.BitOffset > capacity - bitField.BitCount)
            {
                continue;
            }

            output.Add(bitField);
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
