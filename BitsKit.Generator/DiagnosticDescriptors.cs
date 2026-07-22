using Microsoft.CodeAnalysis;

namespace BitsKit.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "BitsKit.Generator";

    public static readonly DiagnosticDescriptor MustBePartial = new(
       id: "BITSKIT001",
       title: "BitsKit object must be partial",
       messageFormat: "'{0}' must be partial",
       category: Category,
       defaultSeverity: DiagnosticSeverity.Error,
       isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestedNotAllowed = new(
        id: "BITSKIT002",
        title: "BitsKit object must not be a nested type",
        messageFormat: "'{0}' must not be a nested type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FieldTypeNotDefined = new(
        id: "BITSKIT003",
        title: "Cannot infer FieldType",
        messageFormat: "'{0}.{1}' FieldType cannot be inferred",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingAccessors = new(
        id: "BITSKIT004",
        title: "Conflicting accessability modifiers",
        messageFormat: "'{0}.{1}' has conflicting accessor modifiers",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingSetters = new(
        id: "BITSKIT005",
        title: "Conflicting setter modifiers",
        messageFormat: "'{0}.{1}' has conflicting setter modifiers",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EnumTypeExpected = new(
        id: "BITSKIT006",
        title: "Enum type argument expected",
        messageFormat: "'{0}.{1}' type argument is not an Enum",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidBitObjectOption = new(
        "BITSKIT007", "Invalid bit object option", "'{0}' has an invalid {1} value '{2}'",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor UnsupportedBackingField = new(
        "BITSKIT008", "Unsupported bit-field backing type", "'{0}.{1}' uses unsupported backing type '{2}'",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor RawPointerRequiresUnsafe = new(
        "BITSKIT009", "Raw pointer backing requires unsafe access",
        "'{0}.{1}' is a raw byte pointer and requires BitObjectAccessMode.Unsafe; use a fixed buffer for checked access",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidFieldName = new(
        "BITSKIT010", "Invalid generated bit-field name", "'{0}.{1}' is not a valid generated member name",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor GeneratedMemberConflict = new(
        "BITSKIT011", "Generated bit-field member conflicts with another member",
        "'{0}' already contains a member named '{1}'", Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidFieldWidth = new(
        "BITSKIT012", "Invalid bit-field width",
        "'{0}.{1}' width {2} is invalid for '{3}', whose maximum width is {4} bits",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor LayoutExceedsBacking = new(
        "BITSKIT013", "Bit-field layout exceeds its backing storage",
        "'{0}.{1}' ends at bit {2}, beyond the {3}-bit capacity of backing field '{4}'",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidModifiers = new(
        "BITSKIT014", "Invalid bit-field modifiers", "'{0}.{1}' has invalid modifiers: {2}",
        Category, DiagnosticSeverity.Error, true);
}
