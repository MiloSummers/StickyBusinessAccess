param(
    [Parameter(Mandatory=$true)][string]$LoaderDir,
    [string]$PrismDll=(Join-Path $PSScriptRoot 'dependencies\prism\prism.dll'),
    [Parameter(Mandatory=$true)][string]$Destination
)
$ErrorActionPreference='Stop'
$loaderVersion=(Get-Item -LiteralPath (Join-Path $LoaderDir 'BepInEx\core\BepInEx.Unity.IL2CPP.dll')).VersionInfo.ProductVersion
if($loaderVersion -ne '6.0.0-be.788+5b766a3b7f6c164d4798924a93f3acf4db769d06'){throw 'Use the pinned BepInEx Unity.IL2CPP win-x64 be.788 loader, not BepInEx 5 or a Mono build.'}
$dependencyRecord=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'docs\DEPENDENCY-SOURCES.json') -Raw|ConvertFrom-Json
if((Get-FileHash -LiteralPath $PrismDll -Algorithm SHA256).Hash -ne $dependencyRecord.prism.dllSha256){throw 'Use the recorded official Prism 0.18.3 Windows x64 dynamic release DLL.'}
$sourceArchives=@('BepInEx-5b766a3b7f6c.zip','Il2CppInterop-dbda1cb353b0.zip','UnityDoorstop-v4.5.0.zip','Prism-v0.18.3.zip')
foreach($archive in $sourceArchives){
    $archivePath=Join-Path $PSScriptRoot ('third-party-sources\'+$archive)
    if(!(Test-Path -LiteralPath $archivePath)){throw "Required corresponding source archive missing: $archive. Run fetch-distribution-sources.ps1."}
    $record=@($dependencyRecord.distributionSources | Where-Object file -eq $archive)
    if($record.Count -ne 1 -or (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash -ne $record[0].sha256){throw "Corresponding source checksum failed: $archive. Run fetch-distribution-sources.ps1."}
}
if(Test-Path -LiteralPath $Destination){throw 'Choose a new empty release directory.'}
$Destination=[IO.Path]::GetFullPath($Destination)
New-Item -ItemType Directory -Path $Destination | Out-Null
$support=Join-Path $Destination 'support'
New-Item -ItemType Directory -Path $support | Out-Null
$payload=Join-Path $support 'payload'
New-Item -ItemType Directory -Path $payload | Out-Null
# Explicit allowlist: never copy a game directory or BepInEx/interop.
foreach($name in @('winhttp.dll','doorstop_config.ini','.doorstop_version')){Copy-Item -LiteralPath (Join-Path $LoaderDir $name) -Destination $payload}
New-Item -ItemType Directory -Path (Join-Path $payload 'BepInEx') | Out-Null
Copy-Item -LiteralPath (Join-Path $LoaderDir 'BepInEx\core') -Destination (Join-Path $payload 'BepInEx') -Recurse
Copy-Item -LiteralPath (Join-Path $LoaderDir 'dotnet') -Destination $payload -Recurse
$loaderConfig=Join-Path $payload 'BepInEx\config'
New-Item -ItemType Directory -Path $loaderConfig -Force | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\loader-config\BepInEx.cfg" -Destination (Join-Path $loaderConfig 'BepInEx.cfg')
$plugin=Join-Path $payload 'BepInEx\plugins\StickyBusinessAccess'
New-Item -ItemType Directory -Path $plugin | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\src\bin\Release\net6.0\StickyBusinessAccess.dll" -Destination $plugin
Copy-Item -LiteralPath $PrismDll -Destination (Join-Path $plugin 'prism.dll')
Copy-Item -LiteralPath "$PSScriptRoot\catalogue\descriptions.en.json" -Destination $plugin
foreach($name in @('START-HERE.txt','Setup.bat','Uninstall.bat')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $Destination}
foreach($name in @('README.md','setup.ps1','setup-common.ps1','update.ps1','install.ps1','uninstall.ps1','LICENSE','THIRD-PARTY.md')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $support}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'licenses') -Destination $support -Recurse
$playerDocs=Join-Path $support 'docs'
New-Item -ItemType Directory -Path $playerDocs | Out-Null
foreach($name in @('ACCESSIBILITY-0.8.md','DEPENDENCY-SOURCES.json','RELEASE-VERIFICATION.json','RELEASE-AUDIT-0.8.md','BEPINEX-TROUBLESHOOTING.md','PRISM-MIGRATION.md','PRISM-SOURCE-AUDIT.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('docs\'+$name)) -Destination $playerDocs}
$sourceFolder=Join-Path $support 'third-party-sources'
New-Item -ItemType Directory -Path $sourceFolder | Out-Null
foreach($name in $sourceArchives){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('third-party-sources\'+$name)) -Destination $sourceFolder}
$hashes=Get-ChildItem -LiteralPath $payload -Recurse -File -Force | ForEach-Object {
    [PSCustomObject]@{path=$_.FullName.Substring($payload.Length+1);sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
}
$hashes | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $support 'payload-sha256.json') -Encoding UTF8
Add-Type -AssemblyName System.IO.Compression.FileSystem
# Compress-Archive skips hidden files such as .doorstop_version on Windows.
[IO.Compression.ZipFile]::CreateFromDirectory($Destination,$Destination+'.zip')
Write-Host "Release created: $Destination.zip"

