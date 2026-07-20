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

        var model = typeDeclarations.Collect();
        context.RegisterSourceOutput(model, GenerateSourceCode);
    }

    private static TypeSymbolProcessor? ProcessSyntaxNode(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        if (syntaxContext.TargetNode is not TypeDeclarationSyntax typeDeclaration)
            return null;

        ISymbol? symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, token);

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        AttributeData attribute = typeSymbol
            .GetAttributes()
            .Single(a => a.AttributeClass?.ToDisplayString() == StringConstants.BitObjectAttributeFullName);

        return new(typeSymbol, attribute);
    }

    private static void GenerateSourceCode(SourceProductionContext context, ImmutableArray<TypeSymbolProcessor> processors)
    {
        if (processors.Length == 0)
            return;

        StringBuilder stringBuilder = new(StringConstants.Header);

        // group the objects by their respective namespace
        var namespaceGroups = processors.GroupBy(x => x.Namespace);

        foreach (var namespaceGroup in namespaceGroups)
        {
            stringBuilder.AppendLine();

            // print the current namespace
            if (namespaceGroup.Key is not null)
                stringBuilder
                    .AppendLine($"namespace {namespaceGroup.Key}")
                    .AppendLine("{");

            foreach (TypeSymbolProcessor processor in namespaceGroup)
            {
                processor.GenerateCSharpSource(stringBuilder);
            }

            // remove typesymbol seperator
            stringBuilder.RemoveLastLine();

            // apply closing namespace bracket
            if (namespaceGroup.Key is not null)
                stringBuilder.AppendLine("}");
        }

        context.AddSource("BitsKitGeneratedFields.g.cs", stringBuilder.ToString());
    }

    private static bool IsValidTypeDeclaration(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax;
}
