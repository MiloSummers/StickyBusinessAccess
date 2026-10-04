param([string]$GameDir,[switch]$UseExistingLoader)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'setup-common.ps1')
$GameDir=Resolve-StickyBusiness -GameDir $GameDir
if(Get-Process -Name StickyBusiness -ErrorAction SilentlyContinue){throw 'Close Sticky Business before installation.'}
$originalNames=@('StickyBusiness.exe','UnityPlayer.dll','GameAssembly.dll','StickyBusiness_Data\data.unity3d','StickyBusiness_Data\il2cpp_data\Metadata\global-metadata.dat')
foreach($file in $originalNames){if(!(Test-Path -LiteralPath (Join-Path $GameDir $file))){throw "Not the expected game folder: $file is missing."}}
$stream=[IO.File]::OpenRead((Join-Path $GameDir 'StickyBusiness.exe'))
try {
    $reader=New-Object IO.BinaryReader($stream)
    $stream.Position=60;$pe=$reader.ReadInt32();$stream.Position=$pe
    if($reader.ReadUInt32() -ne 17744 -or $reader.ReadUInt16() -ne 34404){throw 'This package requires the Windows x64 game.'}
} finally {$stream.Dispose()}
Test-SetupLinks $GameDir
$files=@(Test-SetupPayload $PSScriptRoot)
$payload=Join-Path $PSScriptRoot 'payload'
$receipt=Join-Path $GameDir 'StickyBusinessAccess-install.json'
$reinstall=$false
if(Test-Path -LiteralPath $receipt){
    $prior=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if($prior.schemaVersion -ne 1 -or $prior.gameDir -ne $GameDir){throw 'Installation receipt does not match this folder.'}
    if($prior.status -eq 'installed'){throw 'Already installed. Run Setup.bat to update and preserve settings.'}
    if($prior.status -ne 'uninstalled'){throw 'An interrupted installation remains. Run Uninstall.bat before retrying Setup.bat.'}
    $reinstall=$true
}
if($UseExistingLoader){Test-CompatibleExistingLoader -GameDir $GameDir -Package $PSScriptRoot}
else {
foreach($name in @('winhttp.dll','doorstop_config.ini','.doorstop_version','version.dll')){
    if(Test-Path -LiteralPath (Join-Path $GameDir $name)){throw "Existing loader file found: $name. No files were changed. This package supports an unmodded game or completed Sticky Access uninstall."}
}
foreach($name in @('BepInEx','dotnet')){
    $root=Join-Path $GameDir $name
    if(!(Test-Path -LiteralPath $root)){continue}
    if(!$reinstall){throw "Existing $name folder without a completed Sticky Access uninstall receipt. No files were changed. This is an existing-loader conflict, not a missing dependency."}
    foreach($item in @(Get-ChildItem -LiteralPath $root -Recurse -File -Force)){
        $relative=$item.FullName.Substring($GameDir.Length+1)
        $passive=$relative -match '^BepInEx\\(config|cache|interop|unity-libs)\\' -or $relative -match '^BepInEx\\[^\\]+\.log$' -or $relative -eq 'BepInEx\plugins\StickyBusinessAccess\descriptions.en.json'
        if(!$passive){throw "Retained executable or unknown loader file needs review: $relative. It has not been overwritten."}
    }
}
}
$copy=@()
foreach($file in $files){
    $relative=$file.FullName.Substring($payload.Length+1)
    if($UseExistingLoader -and $relative -notmatch '^BepInEx\\plugins\\StickyBusinessAccess\\'){continue}
    $target=Get-SafeSetupPath $GameDir $relative
    if(Test-Path -LiteralPath $target){
        if($reinstall -and $relative -in @('BepInEx\config\BepInEx.cfg','BepInEx\plugins\StickyBusinessAccess\descriptions.en.json')){Write-Host "Preserving preference file: $relative";continue}
        throw "Installation would overwrite $relative. No files were changed."
    }
    $copy+=$file
}
$probe=Join-Path $GameDir ('.sticky-access-write-test-'+[Guid]::NewGuid().ToString('N'))
try {[IO.File]::WriteAllText($probe,'setup');Remove-Item -LiteralPath $probe}
catch {throw 'Windows denied write access to the game folder. Close the game. If your Steam folder requires administrator access, run Setup.bat as administrator.'}
$originals=@()
foreach($name in $originalNames){$originals+=@{path=$name;sha256=(Get-FileHash -LiteralPath (Join-Path $GameDir $name) -Algorithm SHA256).Hash}}
$installed=@()
$ownership='bundled'
if($UseExistingLoader){$ownership='external'}
$state=@{schemaVersion=1;gameDir=$GameDir;version='0.8.2';setupRevision=1;loaderOwnership=$ownership;originals=$originals;files=@();status='installing'}
$state|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt -Encoding UTF8
try {
    foreach($file in $copy){
        $relative=$file.FullName.Substring($payload.Length+1)
        $target=Get-SafeSetupPath $GameDir $relative
        $installed+=@{path=$relative;sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
        $state.files=@($installed)
        $state|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt -Encoding UTF8
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force|Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $target
    }
    $state.status='installed'
    $state|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt -Encoding UTF8
} catch {throw "Installation interrupted: $($_.Exception.Message). Run Uninstall.bat to remove matching added files, then retry Setup.bat."}
Write-Host "Installed Sticky Access 0.8.2 into $GameDir (loader ownership: $ownership)"
Write-Host 'Game files and saves were not replaced. Start your supported screen reader, then launch through Steam.'

