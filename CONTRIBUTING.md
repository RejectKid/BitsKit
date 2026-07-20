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

For performance work, run a focused .NET 8 and .NET 10 comparison:

```powershell
dotnet run --project BitsKit.Benchmarks/BitsKit.Benchmarks.csproj --configuration Release --framework net10.0 -- --filter "*MethodName*" --job short --runtimes net8.0 net10.0 --exporters markdown json --artifacts artifacts/benchmarks
```

Attach the generated report and environment metadata to the pull request. Results from shared GitHub-hosted runners are directional and should not be treated as stable baselines.

## Pull requests

- Add tests for fixes and new behavior.
- Preserve the public API unless the change is intentionally breaking and documented.
- Benchmark performance claims using `BitsKit.Benchmarks`.
- Keep source-generator outputs deterministic and incremental.
- Update `README.md`, `CHANGELOG.md`, and `ReleaseNotes.txt` when user-facing behavior changes.

By contributing, you agree that your contribution is licensed under the repository's MIT license.
