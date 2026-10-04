param([string]$GameDir)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'setup-common.ps1')
$GameDir=Resolve-StickyBusiness -GameDir $GameDir
Test-SetupLinks $GameDir
if(Get-Process -Name StickyBusiness -ErrorAction SilentlyContinue){throw 'Close Sticky Business before removal.'}
$receipt=Join-Path $GameDir 'StickyBusinessAccess-install.json'
$state=Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
if($state.schemaVersion -ne 1 -or $state.gameDir -ne $GameDir){throw 'Receipt does not match this installation folder.'}
$remaining=@()
$external=$state.PSObject.Properties['loaderOwnership'] -and $state.loaderOwnership -eq 'external'
foreach($entry in $state.files){
    $null=Get-SafeSetupPath $GameDir $entry.path
    if($external -and $entry.path -notmatch '^BepInEx\\plugins\\StickyBusinessAccess\\'){throw 'External-loader receipt contains a non-plugin path. No removal performed.'}
    if($entry.path -notmatch '^(BepInEx|dotnet)\\' -and $entry.path -notin @('winhttp.dll','doorstop_config.ini','.doorstop_version')){throw 'Receipt contains a path outside the mod installation. No removal performed.'}
}
foreach($entry in $state.files){
    $target=Get-SafeSetupPath $GameDir $entry.path
    if(!(Test-Path -LiteralPath $target)){continue}
    if((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -eq $entry.sha256){Remove-Item -LiteralPath $target}
    else {$remaining+=$entry;Write-Warning "Kept modified file: $($entry.path)"}
}
# Remove empty directories only. Generated logs/configs/interop and later-added mods remain.
$cleanupRoots=@('BepInEx','dotnet')
if($external){$cleanupRoots=@('BepInEx\plugins\StickyBusinessAccess')}
foreach($rootName in $cleanupRoots){
    $root=Join-Path $GameDir $rootName
    if(Test-Path -LiteralPath $root){
        $dirs=@(Get-ChildItem -LiteralPath $root -Recurse -Directory | Sort-Object {$_.FullName.Length} -Descending)
        foreach($dir in $dirs){if(@(Get-ChildItem -LiteralPath $dir.FullName -Force).Count -eq 0){Remove-Item -LiteralPath $dir.FullName}}
        if(@(Get-ChildItem -LiteralPath $root -Force).Count -eq 0){Remove-Item -LiteralPath $root}
    }
}
foreach($original in $state.originals){
    $path=Join-Path $GameDir $original.path
    if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $original.sha256){Write-Warning "Original file changed since install (possibly a Steam update): $($original.path). This uninstaller has not restored or modified it."}
}
$state.files=@($remaining)
$state.status='uninstalled'
$state | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $receipt -Encoding UTF8
if($remaining.Count){Write-Warning 'Modified files were preserved. Modified executable files need review before reinstalling.'}
Write-Host 'The uninstall receipt is retained so Setup.bat recognizes generated files and preserves preferences on reinstall.'
Write-Host 'Matching installed files removed. Saves are untouched. Generated logs/configs/interop or modified files may remain for review.'
