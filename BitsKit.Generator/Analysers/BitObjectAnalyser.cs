using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BitsKit.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BitsKit.Generator.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BitObjectAnalyser : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        DiagnosticDescriptors.MustBePartial,
        DiagnosticDescriptors.NestedNotAllowed,
        DiagnosticDescriptors.InvalidBitObjectOption,
        DiagnosticDescriptors.UnsupportedBackingField,
        DiagnosticDescriptors.RawPointerRequiresUnsafe,
        DiagnosticDescriptors.InvalidFieldName,
        DiagnosticDescriptors.GeneratedMemberConflict,
        DiagnosticDescriptors.InvalidFieldWidth,
        DiagnosticDescriptors.LayoutExceedsBacking,
        DiagnosticDescriptors.InvalidModifiers
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            INamedTypeSymbol? bitObjectAttribute = startContext.Compilation.GetTypeByMetadataName(
                StringConstants.BitObjectAttributeFullName);
            INamedTypeSymbol? bitFieldAttribute = startContext.Compilation.GetTypeByMetadataName(
                StringConstants.BitFieldAttributeFullName);
            if (bitObjectAttribute is null || bitFieldAttribute is null)
                return;

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeType(
                    symbolContext,
                    bitObjectAttribute,
                    bitFieldAttribute),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(
        SymbolAnalysisContext context,
        INamedTypeSymbol bitObjectAttribute,
        INamedTypeSymbol bitFieldAttribute)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!type.TryGetAttributeWithType(bitObjectAttribute, out AttributeData? bitObjectData))
            return;

        TypeDeclarationSyntax? declaration = type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax(context.CancellationToken))
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();
        if (declaration is null)
            return;

        if (!declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MustBePartial,
                declaration.GetLocation(),
                type.Name));
        }

        if (type.ContainingType is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NestedNotAllowed,
                declaration.GetLocation(),
                type.Name));
        }

        AnalyzeOptions(context, type, bitObjectData, declaration.GetLocation());
        AnalyzeFields(context, type, bitObjectData, bitFieldAttribute);
    }

    private static void AnalyzeOptions(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        AttributeData attribute,
        Location location)
    {
        if (!TypeSymbolProcessor.TryGetBitOrder(attribute, out _))
        {
            object? value = attribute.ConstructorArguments.Length == 0
                ? null
                : attribute.ConstructorArguments[0].Value;
            ReportInvalidOption(context, type, location, "BitOrder", value);
        }

        if (!TypeSymbolProcessor.TryGetAccessMode(attribute, out _))
        {
            object? value = attribute.NamedArguments
                .FirstOrDefault(argument => argument.Key == "AccessMode")
                .Value.Value;
            ReportInvalidOption(context, type, location, "AccessMode", value);
        }
    }

    private static void ReportInvalidOption(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        Location location,
        string option,
        object? value) => context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.InvalidBitObjectOption,
            location,
            type.Name,
            option,
            value ?? "null"));

    private static void AnalyzeFields(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        AttributeData bitObjectAttribute,
        INamedTypeSymbol bitFieldAttribute)
    {
        TypeSymbolProcessor.TryGetAccessMode(bitObjectAttribute, out BitObjectAccessMode accessMode);
        var generatedNames = new HashSet<string>();
        var existingNames = new HashSet<string>(type.GetMembers()
            .Where(member => !IsBitsKitGeneratedMember(member))
            .Select(member => member.Name));
        existingNames.Add(type.Name);
        int inlineArrayLength = GetInlineArrayLength(type);

        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!field.TryGetAttributesWithBaseType(bitFieldAttribute, out List<AttributeData>? attributes))
                continue;

            BackingFieldType backingType = BackingFieldModel.Classify(field);
            if (backingType == BackingFieldType.Integral && inlineArrayLength > 0)
                backingType = BackingFieldType.InlineArray;

            Location fieldLocation = field.Locations.FirstOrDefault() ?? Location.None;
            if (backingType == BackingFieldType.Invalid)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedBackingField,
                    fieldLocation,
                    type.Name,
                    field.Name,
                    field.Type.ToDisplayString()));
                continue;
            }

            var backing = new BackingFieldModel(field, backingType);
            if (backing.IsRawPointer && accessMode != BitObjectAccessMode.Unsafe)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.RawPointerRequiresUnsafe,
                    fieldLocation,
                    type.Name,
                    field.Name));
            }

            AnalyzeFieldAttributes(
                context,
                type,
                field,
                backing,
                backingType,
                attributes,
                inlineArrayLength,
                existingNames,
                generatedNames,
                fieldLocation);
        }
    }

    private static void AnalyzeFieldAttributes(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        IFieldSymbol field,
        BackingFieldModel backing,
        BackingFieldType backingType,
        List<AttributeData> attributes,
        int inlineArrayLength,
        HashSet<string> existingNames,
        HashSet<string> generatedNames,
        Location fieldLocation)
    {
        int offset = 0;
        foreach (AttributeData attribute in attributes)
        {
            BitFieldModel? model = TypeSymbolProcessor.CreateBitFieldFromAttribute(attribute, null);
            if (model is null)
                continue;

            Location location = attribute.ApplicationSyntaxReference?
                .GetSyntax(context.CancellationToken).GetLocation() ?? fieldLocation;
            bool isPadding = model.FieldType == BitFieldType.Padding;
            BitFieldType? declaredFieldType = model.FieldType;
            BitFieldType? effectiveFieldType = model.FieldType;
            if (backingType == BackingFieldType.Integral)
                effectiveFieldType = field.Type.SpecialType.ToBitFieldType();
            else if (backingType == BackingFieldType.InlineArray)
                effectiveFieldType ??= field.Type.SpecialType.ToBitFieldType();

            if (!isPadding)
            {
                AnalyzeName(context, type, model, location, existingNames, generatedNames);
                AnalyzeModifiers(context, type, field, backing, model, location);

                int maximumWidth = model is EnumFieldModel
                    ? declaredFieldType?.GetBitWidth() ?? 0
                    : effectiveFieldType?.GetBitWidth() ?? 0;
                if (model.BitCount <= 0 || maximumWidth == 0 || model.BitCount > maximumWidth)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.InvalidFieldWidth,
                        location,
                        type.Name,
                        model.Name,
                        model.BitCount,
                        model.ReturnType ?? effectiveFieldType?.ToString() ?? "unknown type",
                        maximumWidth));
                }
            }

            int capacity = GetCapacity(backingType, backing, field, inlineArrayLength);
            long end = (long)offset + model.BitCount;
            if (capacity != int.MaxValue && end > capacity)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.LayoutExceedsBacking,
                    location,
                    type.Name,
                    isPadding ? "<padding>" : model.Name,
                    end,
                    capacity,
                    field.Name));
            }

            offset += model.BitCount;
        }
    }

    private static void AnalyzeName(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        BitFieldModel model,
        Location location,
        HashSet<string> existingNames,
        HashSet<string> generatedNames)
    {
        if (!SymbolFormatting.IsValidGeneratedName(model.Name))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidFieldName,
                location,
                type.Name,
                model.Name ?? "<null>"));
            return;
        }

        string name = model.Name.StartsWith("@") ? model.Name.Substring(1) : model.Name;
        if (existingNames.Contains(name) || !generatedNames.Add(name))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.GeneratedMemberConflict,
                location,
                type.Name,
                name));
        }
    }

    private static void AnalyzeModifiers(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        IFieldSymbol field,
        BackingFieldModel backing,
        BitFieldModel model,
        Location location)
    {
        const BitFieldModifiers knownModifiers =
            BitFieldModifiers.AccessorMask |
            BitFieldModifiers.ReadOnly |
            BitFieldModifiers.InitOnly |
            BitFieldModifiers.Required;
        string? reason = null;

        if ((model.Modifiers & ~knownModifiers) != 0)
            reason = "unknown modifier bits";
        else if (type.TypeKind == TypeKind.Struct &&
                 (model.Modifiers & BitFieldModifiers.AccessorMask) is
                     BitFieldModifiers.Protected or
                     BitFieldModifiers.ProtectedInternal or
                     BitFieldModifiers.PrivateProtected)
            reason = "struct members cannot be protected";
        else if (model.Modifiers.HasFlag(BitFieldModifiers.Required) &&
                 (model.Modifiers.HasFlag(BitFieldModifiers.ReadOnly) ||
                  field.IsReadOnly || backing.IsReadOnlyStorage))
            reason = "required fields must have a writable setter";

        if (reason is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidModifiers,
                location,
                type.Name,
                model.Name,
                reason));
        }
    }

    private static int GetCapacity(
        BackingFieldType backingType,
        BackingFieldModel backing,
        IFieldSymbol field,
        int inlineArrayLength) => backingType switch
    {
        BackingFieldType.Integral => field.Type.SpecialType.GetBitWidth(),
        BackingFieldType.Pointer when backing.FixedSize > 0 => backing.FixedSize * 8,
        BackingFieldType.InlineArray => inlineArrayLength * field.Type.SpecialType.GetBitWidth(),
        _ => int.MaxValue
    };

    private static int GetInlineArrayLength(INamedTypeSymbol type) =>
        (int?)type.GetAttributes()
            .FirstOrDefault(attribute =>
                attribute.AttributeClass?.ToDisplayString() == StringConstants.InlineArrayAttributeFullName)?
            .ConstructorArguments[0].Value ?? 0;

    private static bool IsBitsKitGeneratedMember(ISymbol member)
    {
        foreach (Location location in member.Locations)
        {
            if (location.SourceTree?.FilePath is not string path)
                continue;

            int separator = System.Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            string fileName = path.Substring(separator + 1);
            if (fileName.StartsWith("BitsKit.") && fileName.EndsWith(".g.cs"))
                return true;
        }

        return false;
    }
}
