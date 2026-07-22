using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BitsKit.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class BitObjectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TypeSymbolProcessor> typeDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                StringConstants.BitObjectAttributeFullName,
                predicate: IsValidTypeDeclaration,
                transform: ProcessSyntaxNode)
            .Where(x => x is not null)
            .WithTrackingName("Main")!;

        context.RegisterSourceOutput(typeDeclarations, GenerateSourceCode);
    }

    private static TypeSymbolProcessor? ProcessSyntaxNode(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        if (syntaxContext.TargetNode is not TypeDeclarationSyntax typeDeclaration)
            return null;

        ISymbol? symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, token);

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        AttributeData? attribute = typeSymbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == StringConstants.BitObjectAttributeFullName);

        return attribute is null ? null : new(typeSymbol, attribute);
    }

    private static void GenerateSourceCode(SourceProductionContext context, TypeSymbolProcessor processor)
    {
        if (!processor.IsValid)
            return;

        StringBuilder stringBuilder = new(StringConstants.Header);
        stringBuilder.AppendLine();

        if (processor.Namespace is not null)
            stringBuilder
                .AppendLine($"namespace {processor.Namespace}")
                .AppendLine("{");

        processor.GenerateCSharpSource(stringBuilder);
        stringBuilder.RemoveLastLine();

        if (processor.Namespace is not null)
            stringBuilder.AppendLine("}");

        context.AddSource(processor.HintName, stringBuilder.ToString());
    }

    private static bool IsValidTypeDeclaration(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax;
}
