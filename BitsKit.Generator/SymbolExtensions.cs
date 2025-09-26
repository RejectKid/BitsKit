using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace BitsKit.Generator
{
    public static class SymbolExtensions
    {
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
    }
}