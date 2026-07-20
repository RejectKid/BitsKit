[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [string] $ExpectedVersion,

    [string] $ExpectedPackageId = 'RejectKid.BitsKit'
)

$ErrorActionPreference = 'Stop'
$resolvedPackage = (Resolve-Path -LiteralPath $PackagePath).Path

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackage)

try {
    $entryNames = @($archive.Entries | ForEach-Object FullName)
    $requiredEntries = @(
        'README.md',
        'LICENSE.txt',
        'analyzers/dotnet/cs/BitsKit.Generator.dll',
        'lib/netstandard2.1/BitsKit.dll',
        'lib/net8.0/BitsKit.dll',
        'lib/net10.0/BitsKit.dll'
    )

    foreach ($entry in $requiredEntries) {
        if ($entryNames -cnotcontains $entry) {
            throw "Package is missing required entry '$entry'."
        }
    }

    $nuspecEntry = $archive.Entries | Where-Object FullName -Like '*.nuspec' | Select-Object -First 1
    if ($null -eq $nuspecEntry) {
        throw 'Package does not contain a .nuspec file.'
    }

    $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
    try {
        [xml] $nuspec = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }

    $metadata = $nuspec.package.metadata
    if ($metadata.id -ne $ExpectedPackageId) {
        throw "Expected package ID '$ExpectedPackageId', found '$($metadata.id)'."
    }

    if ($metadata.version -ne $ExpectedVersion) {
        throw "Expected package version '$ExpectedVersion', found '$($metadata.version)'."
    }

    if ($metadata.repository.url -ne 'https://github.com/RejectKid/BitsKit') {
        throw "Unexpected repository URL '$($metadata.repository.url)'."
    }

    Write-Host "Validated $ExpectedPackageId $ExpectedVersion ($($entryNames.Count) entries)."
}
finally {
    $archive.Dispose()
}
