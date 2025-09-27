using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Text;
using BitsKit.Generator.Models;
using System.Linq;

namespace BitsKit.Generator;

internal sealed class TypeSymbolProcessor
{
    public INamedTypeSymbol TypeSymbol { get; }
    public IReadOnlyList<BitFieldModel> Fields => _fields;
    public string? Namespace { get; }
    public bool IsStruct { get; }
    public bool IsInlineArray { get; }

    private readonly BitOrder _defaultBitOrder;
    private readonly List<BitFieldModel> _fields = [];
    
    private readonly string _syntaxKeyword;
    private readonly string _syntaxIdentifier;

    public TypeSymbolProcessor(INamedTypeSymbol typeSymbol, AttributeData attribute)
    {
        TypeSymbol = typeSymbol;
        
        _syntaxKeyword = typeSymbol.TypeKind switch
        {
            TypeKind.Struct when typeSymbol.IsRecord => "record struct",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            TypeKind.Class when typeSymbol.IsRecord => "record",
            _ => "class"
        };
        _syntaxIdentifier = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        
        Namespace = typeSymbol.ContainingNamespace.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        if (string.IsNullOrWhiteSpace(Namespace)) Namespace = null;
        
        IsStruct = typeSymbol.TypeKind == TypeKind.Struct;
        IsInlineArray = HasInlineArrayAttribute();

        _defaultBitOrder = (BitOrder)attribute.ConstructorArguments[0].Value!;
    }

    public void GenerateCSharpSource(StringBuilder sb)
    {
        sb.AppendIndentedLine(1,
            StringConstants.TypeDeclarationTemplate,
            _syntaxKeyword,
            _syntaxIdentifier)
          .AppendIndentedLine(1, "{");

        foreach (BitFieldModel field in _fields)
            field.GenerateCSharpSource(sb);

        sb.RemoveLastLine()
          .AppendIndentedLine(1, "}")
          .AppendLine();
    }

    public int EnumerateFields()
    {
        _fields.Clear();

        foreach (IFieldSymbol field in TypeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!IsValidFieldSymbol(field))
                continue;

            BackingFieldType backingType = field.Type.ToDisplayString() switch
            {
                "System.Memory<byte>" => BackingFieldType.Memory,
                "System.ReadOnlyMemory<byte>" => BackingFieldType.Memory,
                "System.Span<byte>" => BackingFieldType.Span,
                "System.ReadOnlySpan<byte>" => BackingFieldType.Span,
                "byte[]" => BackingFieldType.Span,
                "byte*" => BackingFieldType.Pointer,

                _ when field.Type.IsSupportedIntegralType() => IsInlineArray ? 
                    BackingFieldType.InlineArray : 
                    BackingFieldType.Integral,

                _ => BackingFieldType.Invalid
            };

            if (backingType == BackingFieldType.Invalid)
                continue;

            var backingModel = new BackingFieldModel(field, backingType);
            CreateBitFieldModels(field, backingModel);
        }

        return _fields.Count;
    }

    private void CreateBitFieldModels(IFieldSymbol backingField, BackingFieldModel backingModel)
    {
        int offset = 0;

        foreach (AttributeData attribute in backingField.GetAttributes())
        {
            BitFieldModel? bitField = CreateBitFieldFromAttribute(attribute, this);

            if (bitField == null)
                continue;

            bitField.BackingField = backingModel;
            bitField.BitOffset = offset;
            bitField.BitOrder = _defaultBitOrder;

            // padding fields are not generated
            if (bitField is not { FieldType: BitFieldType.Padding })
            {
                // invert the bit order if necessary
                if (bitField.ReverseBitOrder)
                    bitField.BitOrder ^= BitOrder.MostSignificant;

                // integrals inherit their field type from their backing field
                if (backingModel.Type == BackingFieldType.Integral)
                    bitField.FieldType = backingField.Type.SpecialType.ToBitFieldType();

                // allow inline arrays to infer their type
                if (backingModel.Type == BackingFieldType.InlineArray)
                    bitField.FieldType ??= backingField.Type.SpecialType.ToBitFieldType();

                // add to list of fields to generate
                _fields.Add(bitField);
            }

            offset += bitField.BitCount;
        }
    }
    
    public static BitFieldModel? CreateBitFieldFromAttribute(AttributeData attribute, TypeSymbolProcessor processor)
    {
        string? attributeType = attribute.AttributeClass?.ToDisplayString();
            
        return attributeType switch
        {
            StringConstants.BitFieldAttributeFullName => new IntegralFieldModel(attribute, processor),
            StringConstants.BooleanFieldAttributeFullName => new BooleanFieldModel(attribute, processor),
            StringConstants.EnumFieldAttributeFullName => new EnumFieldModel(attribute, processor),
            _ => null
        };
    }

    private bool HasInlineArrayAttribute()
    {
        return (int?)TypeSymbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == StringConstants.InlineArrayAttributeFullName)?
            .ConstructorArguments[0].Value > 0;
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
