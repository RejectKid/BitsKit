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
            .WithAnalyzers([new BitFieldAnalyser()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
