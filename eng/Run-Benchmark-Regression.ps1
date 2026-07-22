[CmdletBinding()]
param(
    [string] $BaselineCommit = '57b6b1fdae2dbc36f6d1a61bdcae4cb6052c215a',

    [ValidateSet('Dry', 'Short', 'Medium', 'Long')]
    [string] $Job = 'Medium',

    [ValidateRange(0, 100)]
    [double] $RegressionTolerancePercent = 5,

    [string[]] $MethodFilter = @('*'),

    [string] $ArtifactsPath = 'artifacts/benchmark-regression',

    [switch] $Enforce
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$projectPath = Join-Path $repositoryRoot 'BitsKit.Benchmarks.Regression/BitsKit.Benchmarks.Regression.csproj'
$resolvedArtifactsPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $ArtifactsPath))
$runName = '{0}-{1}' -f (Get-Date -Format 'yyyyMMdd-HHmmss'), [Guid]::NewGuid().ToString('N').Substring(0, 8)
$runArtifactsPath = Join-Path $resolvedArtifactsPath "runs/$runName"
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("BitsKit-regression-{0}" -f [Guid]::NewGuid().ToString('N'))
$baselineRoot = Join-Path $temporaryRoot 'baseline'
$worktreeCreated = $false

if ($Enforce -and $Job -eq 'Dry') {
    throw 'Dry benchmarks have only one measurement and cannot enforce performance regressions.'
}

function Invoke-Tool {
    param(
        [Parameter(Mandatory)]
        [string] $FileName,

        [Parameter(Mandatory)]
        [string[]] $Arguments,

        [string] $WorkingDirectory = $repositoryRoot
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false

    foreach ($argument in $Arguments) {
        [void] $startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "'$FileName' exited with code $($process.ExitCode)."
    }
}

function Get-ComparableBenchmarkMethods {
    $methods = [Collections.Generic.List[string]]::new()

    foreach ($operation in 'Read', 'Write') {
        foreach ($order in 'LSB', 'MSB') {
            foreach ($width in 1, 4, 16, 32, 64) {
                $methods.Add("${operation}${width}Bit${order}_BitsKit")
            }
        }
    }

    foreach ($prefix in 'BitReader', 'MemoryBitReader', 'BitStreamReader', 'BitWriter', 'MemoryBitWriter', 'BitStreamWriter') {
        $methods.Add("${prefix}Bit")
        foreach ($suffix in 'UInt04', 'UInt08', 'UInt16', 'UInt32', 'UInt64') {
            $methods.Add("${prefix}${suffix}")
        }
    }

    foreach ($method in
        'GeneratedAccessorGetUInt32LSB',
        'GeneratedAccessorSetUInt32LSB',
        'GeneratedAccessorGetInt32LSB',
        'GeneratedAccessorGetBooleanLSB',
        'GeneratedAccessorGetEnumLSB',
        'GeneratedAccessorGetUInt64LSB',
        'GeneratedAccessorGetUInt32MSB',
        'GeneratedAccessorSetUInt32MSB',
        'GeneratedAccessorSetUInt64MSB',
        'GeneratedAccessorSetBooleanMSB',
        'GeneratedAccessorSetEnumMSB',
        'GeneratedAccessorGetMemoryLSB',
        'GeneratedAccessorSetMemoryLSB',
        'GeneratedAccessorGetAlignedMemoryUInt32LSB',
        'GeneratedAccessorSetAlignedMemoryUInt32LSB',
        'GeneratedAccessorGetAlignedMemoryUInt64LSB',
        'GeneratedAccessorSetAlignedMemoryUInt64LSB',
        'GeneratedAccessorGetInlineArrayLSB') {
        $methods.Add($method)
    }

    if ($methods.Count -ne 74) {
        throw "Expected 74 comparable benchmark methods, found $($methods.Count)."
    }

    return $methods
}

function Invoke-BenchmarkVariant {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [string] $BitsKitProjectPath,

        [Parameter(Mandatory)]
        [string] $BitsKitGeneratorProjectPath,

        [Parameter(Mandatory)]
        [string[]] $Methods
    )

    $variantArtifacts = Join-Path $runArtifactsPath $Name
    $buildProperties = @(
        "-p:BitsKitProjectPath=$BitsKitProjectPath",
        "-p:BitsKitGeneratorProjectPath=$BitsKitGeneratorProjectPath"
    )
    $cleanArguments = @(
        'clean',
        $projectPath,
        '--configuration', 'Release',
        '--framework', 'net10.0'
    ) + $buildProperties
    Invoke-Tool -FileName 'dotnet' -Arguments $cleanArguments

    $restoreArguments = @(
        'restore',
        $projectPath,
        '--force-evaluate'
    ) + $buildProperties
    Invoke-Tool -FileName 'dotnet' -Arguments $restoreArguments

    $arguments = @(
        'run',
        '--project', $projectPath,
        '--configuration', 'Release',
        '--framework', 'net10.0',
        '--no-restore'
    ) + $buildProperties + @(
        '--',
        '--filter'
    )

    foreach ($method in $Methods) {
        $arguments += "*$method"
    }

    $arguments += @(
        '--job', $Job,
        '--exporters', 'markdown', 'json',
        '--artifacts', $variantArtifacts
    )

    Invoke-Tool -FileName 'dotnet' -Arguments $arguments

    $jsonReport = Get-ChildItem (Join-Path $variantArtifacts 'results') -Filter '*-report-full-compressed.json' |
        Select-Object -First 1
    if ($null -eq $jsonReport) {
        throw "BenchmarkDotNet did not produce a JSON report for '$Name'."
    }

    return Get-Content $jsonReport.FullName -Raw | ConvertFrom-Json
}

function Get-CommitHash {
    param([string] $Revision)

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'git'
    $startInfo.WorkingDirectory = $repositoryRoot
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    [void] $startInfo.ArgumentList.Add('rev-parse')
    [void] $startInfo.ArgumentList.Add($Revision)

    $process = [Diagnostics.Process]::Start($startInfo)
    $output = $process.StandardOutput.ReadToEnd().Trim()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "Unable to resolve git revision '$Revision'."
    }

    return $output
}

New-Item -ItemType Directory -Force -Path $resolvedArtifactsPath | Out-Null

try {
    $temporaryPath = [IO.Path]::GetFullPath($temporaryRoot)
    $systemTemporaryPath = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (!$temporaryPath.StartsWith($systemTemporaryPath, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Temporary worktree path is outside the system temporary directory: $temporaryPath"
    }

    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    Invoke-Tool -FileName 'git' -Arguments @('worktree', 'add', '--detach', $baselineRoot, $BaselineCommit)
    $worktreeCreated = $true

    $allMethods = @(Get-ComparableBenchmarkMethods)
    $methods = @($allMethods | Where-Object {
        $method = $_
        @($MethodFilter | Where-Object { $method -like $_ }).Count -ne 0
    })
    if ($methods.Count -eq 0) {
        throw "No benchmark methods matched: $($MethodFilter -join ', ')."
    }
    $baselineProjectPath = Join-Path $baselineRoot 'BitsKit/BitsKit.csproj'
    $baselineGeneratorProjectPath = Join-Path $baselineRoot 'BitsKit.Generator/BitsKit.Generator.csproj'
    $currentProjectPath = Join-Path $repositoryRoot 'BitsKit/BitsKit.csproj'
    $currentGeneratorProjectPath = Join-Path $repositoryRoot 'BitsKit.Generator/BitsKit.Generator.csproj'

    Write-Host "Running fork-point benchmarks from $BaselineCommit..."
    $baseline = Invoke-BenchmarkVariant -Name 'baseline' -BitsKitProjectPath $baselineProjectPath -BitsKitGeneratorProjectPath $baselineGeneratorProjectPath -Methods $methods

    Write-Host 'Running current benchmarks...'
    $current = Invoke-BenchmarkVariant -Name 'current' -BitsKitProjectPath $currentProjectPath -BitsKitGeneratorProjectPath $currentGeneratorProjectPath -Methods $methods

    $baselineByMethod = @{}
    foreach ($benchmark in $baseline.Benchmarks) {
        $baselineByMethod[$benchmark.Method] = $benchmark
    }

    $currentByMethod = @{}
    foreach ($benchmark in $current.Benchmarks) {
        $currentByMethod[$benchmark.Method] = $benchmark
    }

    $missingBaseline = @($methods | Where-Object { !$baselineByMethod.ContainsKey($_) })
    $missingCurrent = @($methods | Where-Object { !$currentByMethod.ContainsKey($_) })
    if ($missingBaseline.Count -ne 0 -or $missingCurrent.Count -ne 0) {
        throw "Benchmark result mismatch. Missing baseline: $($missingBaseline -join ', '); missing current: $($missingCurrent -join ', ')."
    }

    foreach ($property in 'OsVersion', 'ProcessorName', 'RuntimeVersion', 'Architecture') {
        $baselineValue = $baseline.HostEnvironmentInfo.$property
        $currentValue = $current.HostEnvironmentInfo.$property
        if ($baselineValue -ne $currentValue) {
            throw "Benchmark environments differ for '$property': baseline '$baselineValue', current '$currentValue'."
        }
    }

    $comparison = foreach ($method in $methods) {
        $baselineStatistics = $baselineByMethod[$method].Statistics
        $currentStatistics = $currentByMethod[$method].Statistics
        $baselineMean = [double] $baselineStatistics.Mean
        $currentMean = [double] $currentStatistics.Mean
        $changePercent = (($currentMean / $baselineMean) - 1) * 100
        $hasConfidenceIntervals = $Job -ne 'Dry' -and
            [int] $baselineStatistics.N -gt 1 -and
            [int] $currentStatistics.N -gt 1

        if ($hasConfidenceIntervals) {
            $baselineLower = [double] $baselineStatistics.ConfidenceInterval.Lower
            $baselineUpper = [double] $baselineStatistics.ConfidenceInterval.Upper
            $currentLower = [double] $currentStatistics.ConfidenceInterval.Lower
            $currentUpper = [double] $currentStatistics.ConfidenceInterval.Upper
            $allowedRatio = 1 + ($RegressionTolerancePercent / 100)

            $status = if ($currentLower -gt $baselineUpper * $allowedRatio) {
                'Regression'
            }
            elseif ($currentUpper -lt $baselineLower) {
                'Confirmed faster'
            }
            elseif ($changePercent -le $RegressionTolerancePercent) {
                'Within tolerance'
            }
            else {
                'Inconclusive'
            }
        }
        else {
            $status = 'Dry estimate'
        }

        [pscustomobject]@{
            Method = $method
            BaselineNanoseconds = $baselineMean
            CurrentNanoseconds = $currentMean
            ChangePercent = $changePercent
            Status = $status
            ConfidenceQualified = $hasConfidenceIntervals
        }
    }

    $comparison = @($comparison | Sort-Object ChangePercent -Descending)
    $regressions = @($comparison | Where-Object Status -eq 'Regression')
    $inconclusive = @($comparison | Where-Object Status -eq 'Inconclusive')
    $geometricMeanRatio = [Math]::Exp((($comparison | ForEach-Object {
        [Math]::Log($_.CurrentNanoseconds / $_.BaselineNanoseconds)
    } | Measure-Object -Average).Average))
    $geometricMeanChange = ($geometricMeanRatio - 1) * 100
    $baselineHash = Get-CommitHash $BaselineCommit
    $currentHash = Get-CommitHash 'HEAD'

    $report = [Collections.Generic.List[string]]::new()
    $report.Add('# Original BitsKit performance regression report')
    $report.Add('')
    $report.Add("- Baseline: ``$baselineHash``")
    $report.Add("- Current: ``$currentHash``")
    $report.Add("- Job: ``$Job``")
    $report.Add("- Host: $($baseline.HostEnvironmentInfo.ProcessorName), $($baseline.HostEnvironmentInfo.RuntimeVersion)")
    $report.Add("- Allowed regression: $($RegressionTolerancePercent.ToString('0.##'))%")
    $report.Add("- Method filter: $($MethodFilter -join ', ')")
    $report.Add("- Comparable operations: $($comparison.Count)")
    $report.Add("- Geometric-mean change: $($geometricMeanChange.ToString('+0.00;-0.00;0.00'))%")
    $report.Add("- Regressions beyond tolerance: $($regressions.Count)")
    $report.Add("- Inconclusive beyond-mean-threshold results: $($inconclusive.Count)")
    $report.Add('')
    $report.Add('| Method | Original (ns) | Current (ns) | Change | Result |')
    $report.Add('|---|---:|---:|---:|---|')

    foreach ($row in $comparison) {
        $report.Add("| ``$($row.Method)`` | $($row.BaselineNanoseconds.ToString('0.0000')) | $($row.CurrentNanoseconds.ToString('0.0000')) | $($row.ChangePercent.ToString('+0.00;-0.00;0.00'))% | $($row.Status) |")
    }

    $reportPath = Join-Path $resolvedArtifactsPath 'comparison.md'
    $jsonPath = Join-Path $resolvedArtifactsPath 'comparison.json'
    $report | Set-Content $reportPath -Encoding utf8
    $comparison | ConvertTo-Json -Depth 3 | Set-Content $jsonPath -Encoding utf8
    $report | ForEach-Object { Write-Host $_ }

    if (![string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
        $report | Add-Content $env:GITHUB_STEP_SUMMARY -Encoding utf8
    }

    if ($Enforce -and $regressions.Count -ne 0) {
        throw "$($regressions.Count) benchmark regression(s) exceeded the $RegressionTolerancePercent% tolerance."
    }
}
finally {
    if ($worktreeCreated) {
        Invoke-Tool -FileName 'git' -Arguments @('worktree', 'remove', '--force', $baselineRoot)
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Force
    }
}
