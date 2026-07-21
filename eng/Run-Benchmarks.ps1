[CmdletBinding()]
param(
    [string] $Filter = '*',

    [ValidateSet('Dry', 'Short', 'Medium', 'Long')]
    [string] $Job = 'Short',

    [string[]] $Category = @(
        'BitsKit',
        'GeneratedAccessor',
        'BitReader',
        'MemoryBitReader',
        'BitStreamReader',
        'BitWriter',
        'MemoryBitWriter',
        'BitStreamWriter'
    ),

    [string] $ArtifactsPath = 'artifacts/benchmarks',

    [switch] $NoRestore,

    [switch] $List
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
    '--anyCategories'
)
$arguments += $Category

if ($List) {
    $arguments += @('--list', 'flat')
}
else {
    $arguments += @(
        '--job', $Job,
        '--exporters', 'markdown', 'json',
        '--artifacts', $ArtifactsPath
    )
}

# PowerShell expands native-command wildcard arguments before invoking the
# process. ArgumentList preserves filters such as '*' as literal arguments.
$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = 'dotnet'
$startInfo.UseShellExecute = $false

foreach ($argument in $arguments) {
    [void] $startInfo.ArgumentList.Add($argument)
}

$process = [System.Diagnostics.Process]::Start($startInfo)
$process.WaitForExit()

if ($process.ExitCode -ne 0) {
    exit $process.ExitCode
}
