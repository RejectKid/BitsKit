# Contributing

Contributions are welcome. For larger API or generator changes, open an issue first so compatibility and generated-code behavior can be discussed before implementation.

## Development setup

Install the SDK selected by `global.json`, then run:

```powershell
dotnet restore BitsKit.sln --locked-mode
dotnet build BitsKit.sln --configuration Release --no-restore
dotnet test BitsKit.Tests/BitsKit.Tests.csproj --configuration Release --no-build
```

If a dependency changes, run `dotnet restore BitsKit.sln --force-evaluate` and commit the updated `packages.lock.json` files.

For performance work, run a focused benchmark of the affected feature:

```powershell
./eng/Run-Benchmarks.ps1 -Filter "*MethodName*"
```

Attach the generated report and environment metadata to the pull request. Use stable local hardware for performance-regression claims.

For optimization work, compare all 56 operations shared with the original repository on the same machine:

```powershell
./eng/Run-Benchmark-Regression.ps1 -Job Medium -RegressionTolerancePercent 5
```

The script builds both libraries for `net8.0`, runs them sequentially under the same .NET 10 BenchmarkDotNet host, and writes Markdown and JSON ratio reports under `artifacts/benchmark-regression`. Add `-Enforce` to return a failure when any operation exceeds the allowed slowdown. The **Original performance regression** workflow exposes the same settings for on-demand hosted runs; use `Dry` only to validate the harness, not for performance conclusions.

## Pull requests

- Add tests for fixes and new behavior.
- Preserve the public API unless the change is intentionally breaking and documented.
- Benchmark performance claims using `BitsKit.Benchmarks` and the original-performance regression harness where applicable.
- Keep source-generator outputs deterministic and incremental.
- Update `README.md`, `CHANGELOG.md`, and `ReleaseNotes.txt` when user-facing behavior changes.

By contributing, you agree that your contribution is licensed under the repository's MIT license.
