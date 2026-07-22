using Microsoft.CodeAnalysis;

namespace BitsKit.Generator.Models;

/// <summary>
/// A model representing an integral bit-field
/// </summary>
internal sealed record IntegralFieldModel : BitFieldModel
{
    private bool IsTypeCast => this is
    {
        BackingFieldType: BackingFieldType.Integral,
        ReturnType.Length: > 0
    };

    public IntegralFieldModel(AttributeData attributeData, TypeSymbolProcessor? typeSymbol) : base(attributeData, typeSymbol)
    {
        switch (attributeData.ConstructorArguments.Length)
        {
            case 1: // Padding constructor
                if (attributeData.ConstructorArguments[0].Value is byte paddingSize)
                    BitCount = paddingSize;
                break;
            case 2: // Integral backed constructor
                Name = attributeData.ConstructorArguments[0].Value as string ?? string.Empty;
                if (attributeData.ConstructorArguments[1].Value is byte integralSize)
                    BitCount = integralSize;
                break;
            case 3: // Memory backed OR Type Cast constructor
                Name = attributeData.ConstructorArguments[0].Value as string ?? string.Empty;
                if (attributeData.ConstructorArguments[1].Value is byte explicitSize)
                    BitCount = explicitSize;
                if (attributeData.ConstructorArguments[2].Value is int fieldType)
                    FieldType = (BitFieldType)fieldType;
                break;
            default:
                return;
        }

        if (FieldType is not null && FieldType.Value.GetBitWidth() > 0)
            ReturnType = FieldType.Value.ToTypeName();

        if (string.IsNullOrEmpty(Name))
            FieldType = BitFieldType.Padding;
    }

    protected override string GetGetterTemplate()
    {
        if (TryGetUnsafeReadExpression(out string unsafeExpression))
            return "{0} {1} => " + unsafeExpression + ";";

        if (TryGetDirectFixedWidthReadTemplate(out string template))
            return template;

        if (TryGetDirectStorageReadExpression(out string expression) ||
            TryGetDirectIntegralReadExpression(out expression))
        {
            if (IsTypeCast)
                expression = $"({ReturnType})({expression})";

            return "{0} {1} => " + expression + ";";
        }

        if (IsTypeCast)
            return string.Format(StringConstants.ExplicitGetterTemplate, GetterSource(), ReturnType);

        return string.Format(StringConstants.IntegralGetterTemplate, GetterSource());
    }

    protected override string GetSetterTemplate()
    {
        if (TryGetUnsafeWriteExpression("value", out string unsafeExpression))
            return "{0} {1} => " + unsafeExpression + ";";

        if (TryGetDirectFixedWidthWriteTemplate("value", out string template))
            return template;

        if (TryGetDirectStorageWriteExpression("value", out string expression) ||
            TryGetDirectIntegralWriteExpression("value", out expression))
            return "{0} {1} => " + expression + ";";

        if (IsTypeCast)
            return string.Format(StringConstants.ExplicitSetterTemplate, SetterSource(), FieldType);

        return string.Format(StringConstants.IntegralSetterTemplate, SetterSource());
    }
}
