using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BitsKit.Generator.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BitObjectAnalyser : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
            DiagnosticDescriptors.MustBePartial,
            DiagnosticDescriptors.NestedNotAllowed
        ];

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var bitObjectAttribute = context.Compilation.GetTypeByMetadataName(StringConstants.BitObjectAttributeFullName);
                if (bitObjectAttribute == null) return;

                context.RegisterSymbolAction(context =>
                {
                    var type = (INamedTypeSymbol)context.Symbol;

                    if (!type.TryGetAttributeWithType(bitObjectAttribute, out _))
                    {
                        return;
                    }

                    if (type.DeclaringSyntaxReferences[0].GetSyntax() is not TypeDeclarationSyntax typeDeclarationSyntax)
                    {
                        return;
                    }

                    if (!typeDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(DiagnosticDescriptors.MustBePartial, typeDeclarationSyntax.GetLocation(), type.Name)
                        );
                    }

                    if (type.ContainingType != null)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(DiagnosticDescriptors.NestedNotAllowed, typeDeclarationSyntax.GetLocation(), type.Name)
                        );
                    }
                }, SymbolKind.NamedType);
            });
        }
    }
}
