[CmdletBinding()]
param(
    [string] $Filter = '*',

    [ValidateSet('Dry', 'Short', 'Medium', 'Long')]
    [string] $Job = 'Short',

    [string[]] $Category = @(
        'BitsKit',
        'BitReader',
        'MemoryBitReader',
        'BitStreamReader',
        'BitWriter',
        'MemoryBitWriter',
        'BitStreamWriter'
    ),

    [string] $ArtifactsPath = 'artifacts/benchmarks',

    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$projectPath = Join-Path $repositoryRoot 'BitsKit.Benchmarks/BitsKit.Benchmarks.csproj'

$arguments = @(
    'run',
    '--project', $projectPath,
    '--configuration', 'Release',
    '--framework', 'net10.0'
)

if ($NoRestore) {
    $arguments += '--no-restore'
}

$arguments += @(
    '--',
    '--filter', $Filter,
    '--job', $Job,
    '--anyCategories'
)
$arguments += $Category
$arguments += @(
    '--exporters', 'markdown', 'json',
    '--artifacts', $ArtifactsPath
)

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
