using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BitsKit.Generator
{
    public static class SymbolExtensions
    {
        internal static int GetInlineArrayLength(this ITypeSymbol type) =>
            (int?)type.GetAttributes()
                .FirstOrDefault(attribute =>
                    attribute.AttributeClass?.ToDisplayString() == StringConstants.InlineArrayAttributeFullName)?
                .ConstructorArguments[0].Value ?? 0;

        public static bool TryGetAttributeWithType(this ISymbol symbol, ITypeSymbol typeSymbol, [NotNullWhen(true)] out AttributeData? attributeData)
        {
            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, typeSymbol))
                {
                    attributeData = attribute;

                    return true;
                }
            }

            attributeData = null;
            return false;
        }

        public static bool TryGetAttributesWithBaseType(this ISymbol symbol, ITypeSymbol typeSymbol, [NotNullWhen(true)] out List<AttributeData>? result)
        {
            result = null;

            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                var attributeClass = attribute.AttributeClass!;
                do
                {
                    if (SymbolEqualityComparer.Default.Equals(attributeClass, typeSymbol))
                    {
                        result ??= [];
                        result.Add(attribute);
                        break;
                    }

                    attributeClass = attributeClass.BaseType;
                } while (attributeClass != null);

            }

            return result != null;
        }
    }
}
