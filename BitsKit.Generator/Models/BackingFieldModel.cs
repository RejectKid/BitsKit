using Microsoft.CodeAnalysis;

namespace BitsKit.Generator.Models
{
    internal record BackingFieldModel
    {
        public readonly string OriginalName;
        public readonly string Name;
        public readonly string TypeString;
        public readonly int FixedSize;
        public readonly bool IsReadOnly;
        public readonly bool IsReadOnlyStorage;
        public readonly bool IsByteArray;
        public readonly bool IsRawPointer;

        public readonly BackingFieldType Type;

        public BackingFieldModel(IFieldSymbol fieldSymbol, BackingFieldType type)
        {
            OriginalName = fieldSymbol.Name;
            Name = SymbolFormatting.EscapeIdentifier(fieldSymbol.Name);
            TypeString = SymbolFormatting.GetTypeName(fieldSymbol.Type);
            FixedSize = fieldSymbol.FixedSize;
            IsReadOnly = fieldSymbol.IsReadOnly;
            IsReadOnlyStorage = fieldSymbol.Type is INamedTypeSymbol
            {
                Name: "ReadOnlySpan" or "ReadOnlyMemory",
                ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true }
            };
            IsByteArray = fieldSymbol.Type is IArrayTypeSymbol;
            IsRawPointer = type == BackingFieldType.Pointer && fieldSymbol.FixedSize == 0;

            Type = type;
        }

        public BitFieldType? ResolveFieldType(BitFieldType? declaredType, SpecialType storageType) => Type switch
        {
            BackingFieldType.Integral => storageType.ToBitFieldType(),
            BackingFieldType.InlineArray => declaredType ?? storageType.ToBitFieldType(),
            _ => declaredType
        };

        public int GetCapacity(SpecialType storageType, int inlineArrayLength) => Type switch
        {
            BackingFieldType.Integral => storageType.GetBitWidth(),
            BackingFieldType.Pointer when FixedSize > 0 => FixedSize * 8,
            BackingFieldType.InlineArray => inlineArrayLength * storageType.GetBitWidth(),
            _ => int.MaxValue
        };

        public static BackingFieldType Classify(IFieldSymbol field, int inlineArrayLength = 0)
        {
            if (field.Type is IArrayTypeSymbol
                {
                    Rank: 1,
                    ElementType.SpecialType: SpecialType.System_Byte
                })
            {
                return BackingFieldType.Span;
            }

            if (field.Type is IPointerTypeSymbol
                {
                    PointedAtType.SpecialType: SpecialType.System_Byte
                })
            {
                return BackingFieldType.Pointer;
            }

            if (field.Type is INamedTypeSymbol named &&
                named.TypeArguments.Length == 1 &&
                named.TypeArguments[0].SpecialType == SpecialType.System_Byte &&
                named.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true })
            {
                return named.Name switch
                {
                    "Memory" or "ReadOnlyMemory" => BackingFieldType.Memory,
                    "Span" or "ReadOnlySpan" => BackingFieldType.Span,
                    _ => BackingFieldType.Invalid
                };
            }

            if (!field.Type.IsSupportedIntegralType())
                return BackingFieldType.Invalid;

            return inlineArrayLength > 0 ? BackingFieldType.InlineArray : BackingFieldType.Integral;
        }
    }
}
