# Changelog

Notable changes to the community-maintained fork are documented here. This project follows [Semantic Versioning](https://semver.org/).

## Unreleased

### Changed

- `BitStreamReader` now uses pooled read-ahead buffering to reduce stream calls during sequential reads and supports seeking within buffered data.

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
