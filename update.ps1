param([string]$GameDir,[string]$RestoreBackup)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'setup-common.ps1')
$GameDir=Resolve-StickyBusiness -GameDir $GameDir
Test-SetupLinks $GameDir
if(Get-Process StickyBusiness -ErrorAction SilentlyContinue){throw 'Close Sticky Business before updating or rolling back.'}
$receiptPath=Join-Path $GameDir 'StickyBusinessAccess-install.json'
$oldReceipt=[IO.File]::ReadAllText($receiptPath)
$state=$oldReceipt|ConvertFrom-Json
if($state.schemaVersion -ne 1 -or $state.status -ne 'installed' -or $state.gameDir -ne $GameDir){throw 'Installation receipt does not match this folder.'}
if($state.PSObject.Properties['loaderOwnership'] -and $state.loaderOwnership -eq 'external'){
    Test-CompatibleExistingLoader -GameDir $GameDir -Package $PSScriptRoot
}
$prefix='BepInEx\plugins\StickyBusinessAccess\'
# The old filename is required only for safe migration/rollback of receipt-owned installations.
$binaryNames=@('StickyBusinessAccess.dll','prism.dll','nvdaControllerClient64.dll')
$paths=@($binaryNames|ForEach-Object {$prefix+$_})
$pluginEntry=@($state.files|Where-Object path -eq ($prefix+'StickyBusinessAccess.dll'))
if($pluginEntry.Count -ne 1){throw 'The receipt must own exactly one plugin DLL.'}
# Never overwrite user changes or an unowned dependency.
foreach($relative in $paths){
    $entry=@($state.files|Where-Object path -eq $relative)
    if($entry.Count -gt 1){throw "Duplicate ownership entry: $relative"}
    $target=Get-SafeSetupPath $GameDir $relative
    if($entry.Count -eq 1){
        if(!(Test-Path -LiteralPath $target) -or (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $entry[0].sha256){throw "Installed binary was modified or missing: $relative. Nothing changed."}
    } elseif(Test-Path -LiteralPath $target){throw "Unowned binary needs review: $relative. Nothing changed."}
}
$operations=@()
if($RestoreBackup){
    $newReceipt=[IO.File]::ReadAllText((Join-Path $RestoreBackup 'receipt.json'))
    $restored=$newReceipt|ConvertFrom-Json
    if($restored.schemaVersion -ne 1 -or $restored.status -ne 'installed' -or $restored.gameDir -ne $GameDir){throw 'Backup does not match this installation.'}
    foreach($name in $binaryNames){
        $relative=$prefix+$name
        $expected=@($restored.files|Where-Object path -eq $relative)
        if($expected.Count -gt 1){throw 'Duplicate backup ownership entry.'}
        $source=$null
        if($expected.Count -eq 1){
            $source=Join-Path $RestoreBackup $name
            if(!(Test-Path -LiteralPath $source) -or (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $expected[0].sha256){throw "Backup binary checksum mismatch: $name"}
        }
        $operations+=@{path=$relative;source=$source}
    }
} else {
    $null=Test-SetupPayload $PSScriptRoot
    $manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'payload-sha256.json') -Raw|ConvertFrom-Json
    $state.files=@($state.files|Where-Object {$_.path -notin $paths})
    foreach($name in $binaryNames){
        $relative=$prefix+$name; $source=$null
        if($name -ne 'nvdaControllerClient64.dll'){
            $expected=@($manifest|Where-Object path -eq $relative)
            if($expected.Count -ne 1){throw "Release manifest must contain $name"}
            $source=Join-Path $PSScriptRoot ('payload\'+$relative)
            $state.files+=[PSCustomObject]@{path=$relative;sha256=$expected[0].sha256}
        }
        $operations+=@{path=$relative;source=$source}
    }
    $state.version='0.8.2'
    $newReceipt=$state|ConvertTo-Json -Depth 8
}
$backup=Join-Path $PSScriptRoot ('backups\'+[DateTime]::Now.ToString('yyyyMMdd-HHmmss-ffff'))
New-Item -ItemType Directory -Path $backup | Out-Null
foreach($name in $binaryNames){
    $target=Join-Path $GameDir ($prefix+$name)
    if(Test-Path -LiteralPath $target){Copy-Item -LiteralPath $target -Destination (Join-Path $backup $name)}
}
[IO.File]::WriteAllText((Join-Path $backup 'receipt.json'),$oldReceipt)
try {
    foreach($op in $operations){
        $target=Get-SafeSetupPath $GameDir $op.path
        if($op.source){Copy-Item -LiteralPath $op.source -Destination $target}
        elseif(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target}
    }
    [IO.File]::WriteAllText($receiptPath,$newReceipt)
} catch {
    foreach($name in $binaryNames){
        $target=Join-Path $GameDir ($prefix+$name); $saved=Join-Path $backup $name
        if(Test-Path -LiteralPath $saved){Copy-Item -LiteralPath $saved -Destination $target}
        elseif(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target}
    }
    [IO.File]::WriteAllText($receiptPath,$oldReceipt)
    throw
}
Write-Host "Updated mod and speech dependency. Backup: $backup"
Write-Host 'Your catalogue, loader, game files and saves were not replaced.'
Write-Host 'To roll back: run update.ps1 -GameDir with this game folder and -RestoreBackup with the backup folder above.'
