# Changelog

Notable changes to the community-maintained fork are documented here. This project follows [Semantic Versioning](https://semver.org/).

## Unreleased

### Added

- Cross-platform continuous integration for .NET 8 and .NET 10.
- Validated, tag-driven NuGet and GitHub release automation.
- CodeQL, Dependabot, package provenance attestations, and contribution guidance.
- Incremental source-generator regression coverage and analyzer-based diagnostics.

### Changed

- Maintained packages use the `RejectKid.BitsKit` NuGet ID while retaining the `BitsKit` assembly and namespaces.
- Builds use a pinned .NET SDK and C# 12 instead of an environment-dependent preview language version.
- The source generator no longer retains Roslyn symbols or syntax nodes between runs.
- Library packages target `netstandard2.1`, .NET 8, and .NET 10; the EOL .NET 6 and .NET 7 assets were removed.
- Tests and benchmarks use current MSTest, test SDK, coverage, Roslyn, analyzer, and BenchmarkDotNet packages.

### Fixed

- Restored `BITSKIT003` when a memory-backed bit field omits its required `FieldType`.
- Generate writable spans for inline-array setters so consumers compile with modern C# compilers.

## 1.2.0 - 2024-11-19

- Added inline-array support.

Earlier release history is available in `ReleaseNotes.txt` and the upstream repository.
