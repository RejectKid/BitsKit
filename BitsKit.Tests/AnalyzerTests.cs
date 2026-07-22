using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BitsKit.Generator.Analysers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitsKit.Tests;

[TestClass]
public class AnalyzerTests
{
    [TestMethod]
    [DataRow("Span<byte>")]
    [DataRow("ReadOnlySpan<byte>")]
    [DataRow("Memory<byte>")]
    [DataRow("ReadOnlyMemory<byte>")]
    [DataRow("byte[]")]
    [DataRow("byte*")]
    public async Task MissingFieldTypeReportsBitsKit003(string backingType)
    {
        string source = $$"""
            [BitObject(BitOrder.LeastSignificant)]
            public unsafe ref partial struct MissingFieldType
            {
                [BitField("Value", 3)]
                public {{backingType}} BackingField;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Diagnostic diagnostic = diagnostics.Single(d => d.Id == "BITSKIT003");

        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.AreEqual("'MissingFieldType.Value' FieldType cannot be inferred", diagnostic.GetMessage());
    }

    [TestMethod]
    public async Task ExplicitMemoryFieldTypeDoesNotReportBitsKit003()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public ref partial struct ExplicitFieldType
            {
                [BitField("Value", 3, BitFieldType.UInt16)]
                public Span<byte> BackingField;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.IsFalse(diagnostics.Any(d => d.Id == "BITSKIT003"));
    }

    [TestMethod]
    public async Task IntegralBackingFieldInfersType()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public partial struct InferredFieldType
            {
                [BitField("Value", 3)]
                public ushort BackingField;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);

        Assert.IsFalse(diagnostics.Any(d => d.Id == "BITSKIT003"));
    }

    [TestMethod]
    [DataRow("[BitObject((BitOrder)123)]", "BitOrder")]
    [DataRow("[BitObject(BitOrder.LeastSignificant, AccessMode = (BitObjectAccessMode)123)]", "AccessMode")]
    public async Task InvalidBitObjectOptionsReportBitsKit007(string attribute, string option)
    {
        string source = $$"""
            {{attribute}}
            public partial struct InvalidOptions
            {
                [BitField("Value", 1)]
                public byte Backing;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Diagnostic diagnostic = diagnostics.Single(d => d.Id == "BITSKIT007");
        StringAssert.Contains(diagnostic.GetMessage(), option);
    }

    [TestMethod]
    public async Task UnsupportedBackingReportsBitsKit008()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public partial struct UnsupportedBacking
            {
                [BitField("Value", 1)]
                public string Backing;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT008"));
    }

    [TestMethod]
    public async Task CheckedRawPointerReportsBitsKit009()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public unsafe partial struct CheckedPointer
            {
                [BitField("Value", 8, BitFieldType.Byte)]
                public byte* Backing;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT009"));
    }

    [TestMethod]
    public async Task InvalidAndDuplicateNamesReportDiagnostics()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public partial struct InvalidNames
            {
                [BitField("not a name", 1)]
                [BitField("Value", 1)]
                [BitField("Value", 1)]
                public byte Backing;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT010"));
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT011"));
    }

    [TestMethod]
    public async Task InvalidWidthsAndLayoutsReportDiagnostics()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public partial struct InvalidLayouts
            {
                [BitField("TooWideForType", 9, BitFieldType.Byte)]
                public byte[] DynamicBacking;

                [BitField("UnknownType", 1, (BitFieldType)123)]
                public byte[] UnknownTypeBacking;

                [BitField(7)]
                [BitField("TooWideForBacking", 2)]
                public byte FixedBacking;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Assert.AreEqual(2, diagnostics.Count(d => d.Id == "BITSKIT012"));
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT013"));
    }

    [TestMethod]
    public async Task InvalidRequiredReadonlyCombinationReportsBitsKit014()
    {
        const string source = """
            [BitObject(BitOrder.LeastSignificant)]
            public partial struct InvalidModifiers
            {
                [BitField("Value", 1, Modifiers = BitFieldModifiers.Required | BitFieldModifiers.ReadOnly)]
                public byte Backing;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(source);
        Assert.AreEqual(1, diagnostics.Count(d => d.Id == "BITSKIT014"));
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "BitsKit.Tests.AnalyzerInput",
            syntaxTrees: [CSharpSyntaxTree.ParseText(Helpers.GeneratorTestHeader + source)],
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true));

        return await compilation
            .WithAnalyzers([new BitFieldAnalyser(), new BitObjectAnalyser()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
