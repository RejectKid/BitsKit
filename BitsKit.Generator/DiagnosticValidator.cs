using Microsoft.CodeAnalysis;
using BitsKit.Generator.Models;

namespace BitsKit.Generator;

internal static class DiagnosticValidator
{
    /// <summary>
    /// Adds a diagnostic to the compilation and returns true
    /// if compilation will fail because of it
    /// </summary>
    public static bool ReportDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor, Location location, params object?[]? messageArgs)
    {
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));

        return descriptor is { DefaultSeverity: DiagnosticSeverity.Error };
    }

    /*public static bool HasMissingFieldType(SourceProductionContext context, BitFieldModel bitField, string typeName)
    {
        return bitField.FieldType is null
            && ReportDiagnostic(
                context,
                DiagnosticDescriptors.FieldTypeNotDefined,
                bitField.BackingField.Locations[0],
                typeName,
                bitField.Name);
    }*/
}
