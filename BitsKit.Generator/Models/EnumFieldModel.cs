using System.IO;
using Microsoft.CodeAnalysis;

namespace BitsKit.Generator.Models;

/// <summary>
/// Parsed data from EnumFieldAttribute. Intermediate data only (don't store in incremental pipeline)
/// </summary>
internal class EnumFieldAttributeModel
{
    public string? Name { get; }
    public INamedTypeSymbol? EnumType { get; }
    public int BitCount { get; set; }

    public EnumFieldAttributeModel(AttributeData attributeData)
    {
        switch (attributeData.ConstructorArguments.Length)
        {
            case 1: // padding constructor
                BitCount = (byte)attributeData.ConstructorArguments[0].Value!;
                break;
            case 3: // enum constructor
                Name = (string)attributeData.ConstructorArguments[0].Value!;
                BitCount = (byte)attributeData.ConstructorArguments[1].Value!;
                EnumType = attributeData.ConstructorArguments[2].Value as INamedTypeSymbol;
                break;
            default:
                throw new InvalidDataException($"unknown number of enum attribute constructor arguments: {attributeData.ConstructorArguments.Length}");
        }
    }
}

/// <summary>
/// A model representing an enum bit-field
/// </summary>
internal sealed record EnumFieldModel : BitFieldModel
{
    public EnumFieldModel(AttributeData attributeData, TypeSymbolProcessor? typeSymbol) : base(attributeData, typeSymbol)
    {
        var attributeModel = new EnumFieldAttributeModel(attributeData);
        Name = attributeModel.Name!; // todo: the nullability on this is well.. wrong. padding fields have no name
        BitCount = attributeModel.BitCount;

        ReturnType = attributeModel.EnumType?.ToDisplayString();
        FieldType = attributeModel.EnumType?.EnumUnderlyingType?.SpecialType.ToBitFieldType();

        if (string.IsNullOrEmpty(Name))
            FieldType = BitFieldType.Padding;
    }

    protected override string GetGetterTemplate()
    {
        if (TryGetDirectIntegralReadExpression(out string expression))
            return $"{{0}} {{1}} => ({ReturnType})({expression});";

        return string.Format(StringConstants.ExplicitGetterTemplate, GetterSource(), ReturnType);
    }

    protected override string GetSetterTemplate()
    {
        if (TryGetDirectIntegralWriteExpression("value", out string expression))
            return "{0} {1} => " + expression + ";";

        return string.Format(StringConstants.ExplicitSetterTemplate, SetterSource(), FieldType);
    }
}
