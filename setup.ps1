param([string]$GameDir)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
try {
    . (Join-Path $PSScriptRoot 'setup-common.ps1')
    Write-Host 'Sticky Access setup. Close Sticky Business before continuing.'
    Write-Host 'The required loader and runtime are included. No separate dependency download or configuration is needed.'
    $GameDir=Resolve-StickyBusiness -GameDir $GameDir -Interactive
    Write-Host "Game folder: $GameDir"
    $receipt=Join-Path $GameDir 'StickyBusinessAccess-install.json'
    $hasReceipt=Test-Path -LiteralPath $receipt
    $installed=$false
    if(Test-Path -LiteralPath $receipt){
        $state=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
        $installed=$state.status -eq 'installed'
    }
    if($installed){Write-Host 'Updating the existing mod; your settings will be preserved.'; & (Join-Path $PSScriptRoot 'update.ps1') -GameDir $GameDir}
    else {
        $existing=$false
        foreach($name in @('winhttp.dll','version.dll','BepInEx\core\BepInEx.Unity.IL2CPP.dll')){
            if(Test-Path -LiteralPath (Join-Path $GameDir $name)){$existing=$true}
        }
        if(!$hasReceipt -and (Test-Path -LiteralPath (Join-Path $GameDir 'BepInEx'))){$existing=$true}
        if($existing){
            Test-CompatibleExistingLoader -GameDir $GameDir -Package $PSScriptRoot
            Write-Host 'You already have a compatible BepInEx installation.'
            Write-Host 'Accept: add Sticky Access only. Your existing loader, its settings, and other mods stay in place.'
            Write-Host 'Decline: cancel setup without installing anything.'
            Write-Host 'Use the existing loader? Type Y to accept or N to decline (Enter also declines), then press Enter.'
            $answer=Read-Host
            if($answer.Trim() -notmatch '^(y|yes)$'){Write-Host 'Declined. No files were installed or changed.';return}
            & (Join-Path $PSScriptRoot 'install.ps1') -GameDir $GameDir -UseExistingLoader
        } else {
            Write-Host 'Installing Sticky Access and its bundled dependencies.'
            & (Join-Path $PSScriptRoot 'install.ps1') -GameDir $GameDir
        }
    }
    Write-Host 'Setup complete. Start your supported screen reader, then launch Sticky Business through Steam.'
    Write-Host 'Keep an internet connection for the first launch. Initial setup may take several minutes.'
} catch {
    Write-Host "Setup did not complete: $($_.Exception.Message)"
    Write-Host 'Your original game files and saves have not been replaced by setup.'
    exit 1
}

