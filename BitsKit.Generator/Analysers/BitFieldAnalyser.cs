using System.Collections.Immutable;
using BitsKit.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BitsKit.Generator.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BitFieldAnalyser : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        DiagnosticDescriptors.FieldTypeNotDefined,
        DiagnosticDescriptors.ConflictingAccessors,
        DiagnosticDescriptors.ConflictingSetters,
        DiagnosticDescriptors.EnumTypeExpected
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            INamedTypeSymbol? bitFieldAttribute = startContext.Compilation.GetTypeByMetadataName(
                StringConstants.BitFieldAttributeFullName);

            if (bitFieldAttribute is null)
                return;

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeField(symbolContext, bitFieldAttribute),
                SymbolKind.Field);
        });
    }

    private static void AnalyzeField(SymbolAnalysisContext context, ITypeSymbol bitFieldAttribute)
    {
        if (context.Symbol is not IFieldSymbol fieldSymbol ||
            !fieldSymbol.TryGetAttributesWithBaseType(bitFieldAttribute, out var attributes))
        {
            return;
        }

        foreach (AttributeData attribute in attributes)
        {
            BitFieldModel? bitField = TypeSymbolProcessor.CreateBitFieldFromAttribute(attribute, null);
            if (bitField is null)
                continue;

            Location location = attribute.ApplicationSyntaxReference?
                .GetSyntax(context.CancellationToken)
                .GetLocation() ?? fieldSymbol.Locations[0];

            if (bitField is IntegralFieldModel { FieldType: null } && RequiresExplicitFieldType(fieldSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.FieldTypeNotDefined,
                    location,
                    fieldSymbol.ContainingType.Name,
                    bitField.Name));
            }

            BitFieldModifiers accessorModifiers = bitField.Modifiers & BitFieldModifiers.AccessorMask;
            if ((accessorModifiers & (accessorModifiers - 1)) != 0 &&
                accessorModifiers != BitFieldModifiers.ProtectedInternal &&
                accessorModifiers != BitFieldModifiers.PrivateProtected)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ConflictingAccessors,
                    location,
                    fieldSymbol.ContainingType.Name,
                    bitField.Name));
            }

            BitFieldModifiers setterModifiers = bitField.Modifiers & BitFieldModifiers.SetterMask;
            if ((setterModifiers & (setterModifiers - 1)) != 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ConflictingSetters,
                    location,
                    fieldSymbol.ContainingType.Name,
                    bitField.Name));
            }

            if (bitField is EnumFieldModel)
            {
                var enumField = new EnumFieldAttributeModel(attribute);
                if (enumField.EnumType is { EnumUnderlyingType: null })
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.EnumTypeExpected,
                        location,
                        fieldSymbol.ContainingType.Name,
                        bitField.Name));
                }
            }
        }
    }

    private static bool RequiresExplicitFieldType(IFieldSymbol fieldSymbol) =>
        fieldSymbol.Type.ToDisplayString() is
            "System.Memory<byte>" or
            "System.ReadOnlyMemory<byte>" or
            "System.Span<byte>" or
            "System.ReadOnlySpan<byte>" or
            "byte[]" or
            "byte*";
}
