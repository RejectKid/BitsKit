# Changelog

Notable changes to the community-maintained fork are documented here. This project follows [Semantic Versioning](https://semver.org/).

## Unreleased

## 1.5.0 - 2026-07-21

### Added

- `BitStreamWriter` supports sequential output to non-seekable streams such as compression, encryption, and network streams.
- An on-demand performance regression workflow compares runtime operations with the original BitsKit fork point, including equivalent models compiled by each revision's source generator, and can enforce a configurable slowdown tolerance.

### Changed

- Integral-backed LSB and MSB generated getters now emit specialized mask-and-shift expressions instead of calling the general-purpose bit primitives.
- The source generator emits direct mask-and-shift setters for valid fixed-width LSB and MSB scalar fields, while retaining `BitPrimitives` for memory-backed, native-integer, and invalid-range cases.
- Byte-aligned `Memory<byte>`, span, array, and byte inline-array fields now use specialized endian-aware generated accessors.
- Generated span and byte inline-array Boolean fields use direct mask operations, and eligible 11-, 12-, 24-, and 48-bit fields use guarded wide-window accessors.
- Sequential `BitStreamReader` and `BitStreamWriter` single-bit operations use dedicated buffered fast paths while preserving existing seeking, EOF, and non-seekable-stream behavior.

### Fixed

- Boolean fields backed by signed integral fields now read a set bit correctly.
- Benchmark reports normalize batched measurements to a single library operation and include generated scalar accessors in the default feature suite.

## 1.4.0 - 2026-07-20

### Changed

- `BitStreamReader` now uses pooled read-ahead buffering to reduce stream calls during sequential reads and supports seeking within buffered data.
- `BitStreamWriter` now encodes sequential writes directly into a pooled output buffer, reducing calls to the underlying stream while retaining in-place write behavior.

### Fixed

- Stream reads now handle partial `Stream.Read` results and throw `EndOfStreamException` instead of returning stale data when the source ends early.
- In-place stream writes preserve existing data across partial reads and initialize bytes written beyond the end of the stream.
- Stream readers and writers support bit positions whose byte offset exceeds `Int32.MaxValue` and consistently throw `ObjectDisposedException` after disposal.

## 1.3.1 - 2026-07-20

### Fixed

- Release publication checks out the tagged repository before verifying and creating the GitHub Release.

## 1.3.0 - 2026-07-20

### Added

- Cross-platform continuous integration for .NET 8 and .NET 10.
- Validated, tag-driven NuGet and GitHub release automation.
- CodeQL, Dependabot, package provenance attestations, and contribution guidance.
- Incremental source-generator regression coverage and analyzer-based diagnostics.
- Categorized, scheduled, and on-demand benchmark reports for the library's features, with CI discovery validation.

### Changed

- Maintained packages use the `RejectKid.BitsKit` NuGet ID while retaining the `BitsKit` assembly and namespaces.
- Builds use a pinned .NET SDK and C# 12 instead of an environment-dependent preview language version.
- The source generator no longer retains Roslyn symbols or syntax nodes between runs.
- Library packages target `netstandard2.1`, .NET 8, and .NET 10; the EOL .NET 6 and .NET 7 assets were removed.
- Tests and benchmarks use current MSTest, test SDK, coverage, Roslyn, analyzer, and BenchmarkDotNet packages.
- The benchmark executable accepts standard BenchmarkDotNet command-line filters, jobs, runtimes, and exporters.

### Fixed

- Restored `BITSKIT003` when a memory-backed bit field omits its required `FieldType`.
- Generate writable spans for inline-array setters so consumers compile with modern C# compilers.
- BenchmarkDotNet generated projects resolve repository metadata correctly, and failed benchmark reports now fail the process.
- Wildcard benchmark filters remain literal when the PowerShell runner invokes BenchmarkDotNet.

## 1.2.0 - 2024-11-19

- Added inline-array support.

Earlier release history is available in `ReleaseNotes.txt` and the upstream repository.
