param([string]$Destination=(Join-Path $PSScriptRoot 'dependencies\prism'))
$ErrorActionPreference='Stop'
$record=(Get-Content (Join-Path $PSScriptRoot 'docs\DEPENDENCY-SOURCES.json') -Raw|ConvertFrom-Json).prism
New-Item -ItemType Directory -Path $Destination -Force|Out-Null
$temporary=Join-Path ([IO.Path]::GetTempPath()) ('sticky-prism-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary|Out-Null
try {
    $archive=Join-Path $temporary 'runtime.zip'
    Invoke-WebRequest -Uri $record.archive -OutFile $archive -UseBasicParsing
    if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $record.archiveSha256){throw 'Prism release archive checksum mismatch.'}
    Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $temporary 'unpacked')
    $dll=Join-Path $temporary ('unpacked\'+$record.binaryPath)
    if((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash -ne $record.dllSha256){throw 'Prism DLL checksum mismatch.'}
    Copy-Item -LiteralPath $dll -Destination (Join-Path $Destination 'prism.dll')
} finally {
    # Only this explicitly created GUID directory is removed.
    $resolved=[IO.Path]::GetFullPath($temporary)
    $root=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if(!$resolved.StartsWith($root,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($resolved) -notmatch '^sticky-prism-[0-9a-f]{32}$'){throw 'Unexpected temporary cleanup path.'}
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Host 'Pinned Prism 0.18.3 runtime downloaded and verified. Players receive this DLL in the package.'
