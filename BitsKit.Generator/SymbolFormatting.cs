using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BitsKit.Generator;

internal static class SymbolFormatting
{
    private static readonly SymbolDisplayFormat TypeFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string EscapeIdentifier(string identifier)
    {
        string value = identifier.StartsWith("@") ? identifier.Substring(1) : identifier;
        return SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None ||
               SyntaxFacts.GetContextualKeywordKind(value) != SyntaxKind.None
            ? "@" + value
            : value;
    }

    public static bool IsValidGeneratedName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        string value = name![0] == '@' ? name.Substring(1) : name;
        return SyntaxFacts.IsValidIdentifier(value) ||
               SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None ||
               SyntaxFacts.GetContextualKeywordKind(value) != SyntaxKind.None;
    }

    public static string GetNamespace(INamespaceSymbol namespaceSymbol)
    {
        var segments = new Stack<string>();
        for (INamespaceSymbol? current = namespaceSymbol;
             current is { IsGlobalNamespace: false };
             current = current.ContainingNamespace)
        {
            segments.Push(EscapeIdentifier(current.Name));
        }

        return string.Join(".", segments);
    }

    public static string GetTypeDeclarationIdentifier(INamedTypeSymbol typeSymbol)
    {
        string identifier = EscapeIdentifier(typeSymbol.Name);
        if (typeSymbol.TypeParameters.Length == 0)
            return identifier;

        return identifier + "<" + string.Join(", ",
            typeSymbol.TypeParameters.Select(parameter => EscapeIdentifier(parameter.Name))) + ">";
    }

    public static string GetTypeName(ITypeSymbol typeSymbol) => typeSymbol.ToDisplayString(TypeFormat);
}
