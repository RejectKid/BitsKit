using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BitsKit.Generator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BitFieldAnalyser : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
            DiagnosticDescriptors.ConflictingAccessors,
            DiagnosticDescriptors.ConflictingSetters,
        ];
        
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            
            context.RegisterCompilationStartAction(context =>
            {
                var bitFieldAttribute = context.Compilation.GetTypeByMetadataName(StringConstants.BitFieldAttributeFullName);
                if (bitFieldAttribute == null) return;
                
                context.RegisterSymbolAction(context =>
                {
                    var fieldSymbol = (IFieldSymbol)context.Symbol;
                    if (!fieldSymbol.TryGetAttributesWithBaseType(bitFieldAttribute, out var thisAttributes))
                    {
                        return;
                    }

                    foreach (var thisAttribute in thisAttributes)
                    {
                        // todo: code doesn't read the processor, just stores. so we don't need to give one
                        var bitField = TypeSymbolProcessor.CreateBitFieldFromAttribute(thisAttribute, null!);
                        if (bitField == null) continue;
                        
                        var accessorModifiers = bitField.Modifiers & BitFieldModifiers.AccessorMask;
                        if ((accessorModifiers & (accessorModifiers - 1)) != 0 && 
                            accessorModifiers != BitFieldModifiers.ProtectedInternal &&
                            accessorModifiers != BitFieldModifiers.PrivateProtected)
                        {
                            // "protected internal" and "private protected" combos are allowed
                            
                            context.ReportDiagnostic(
                                Diagnostic.Create(DiagnosticDescriptors.ConflictingAccessors, thisAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), fieldSymbol.ContainingType.Name, bitField.Name)
                            );
                        }
                        
                        var setterModifiers = bitField.Modifiers & BitFieldModifiers.SetterMask;
                        if ((setterModifiers & (setterModifiers - 1)) != 0)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(DiagnosticDescriptors.ConflictingSetters, thisAttribute.ApplicationSyntaxReference!.GetSyntax().GetLocation(), fieldSymbol.ContainingType.Name, bitField.Name)
                            );
                        }
                    }
                }, SymbolKind.Field);
            });
        }
    }
}