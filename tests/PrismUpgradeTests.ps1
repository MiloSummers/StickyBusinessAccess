param([Parameter(Mandatory=$true)][string]$Package,[Parameter(Mandatory=$true)][string]$CleanGame,[Parameter(Mandatory=$true)][string]$OldPlugin,[Parameter(Mandatory=$true)][string]$Scratch)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$Package=(Resolve-Path -LiteralPath $Package).Path
if(Test-Path -LiteralPath (Join-Path $Package 'support\setup.ps1')){$Package=Join-Path $Package 'support'}
if(Test-Path -LiteralPath $Scratch){throw 'Choose a new scratch directory.'}
$game=Join-Path ([IO.Path]::GetFullPath($Scratch)) 'Prism upgrade game'
New-Item -ItemType Directory -Path $game -Force|Out-Null
foreach($name in @('StickyBusiness.exe','UnityPlayer.dll','GameAssembly.dll','StickyBusiness_Data\data.unity3d','StickyBusiness_Data\il2cpp_data\Metadata\global-metadata.dat')){
    $target=Join-Path $game $name
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force|Out-Null
    Copy-Item -LiteralPath (Join-Path $CleanGame $name) -Destination $target
}
function Hash($path){(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash}
function Check($condition,$message){if(!$condition){throw $message};Write-Host "PASS: $message"}
function Refuses($action,$message){$failed=$false;try{& $action}catch{$failed=$true};Check $failed $message}
& (Join-Path $Package 'install.ps1') -GameDir $game
$prefix='BepInEx\plugins\StickyBusinessAccess\'
$plugin=Join-Path $game ($prefix+'StickyBusinessAccess.dll')
$prism=Join-Path $game ($prefix+'prism.dll')
$controller=Join-Path $game ($prefix+'nvdaControllerClient64.dll')
$receipt=Join-Path $game 'StickyBusinessAccess-install.json'
# Model a real prior mod DLL and a receipt-owned old controller; no fixture is launched.
Copy-Item -LiteralPath $OldPlugin -Destination $plugin
Remove-Item -LiteralPath $prism
[IO.File]::WriteAllText($controller,'old owned controller fixture')
$state=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
$state.files=@($state.files|Where-Object {$_.path -ne ($prefix+'prism.dll')})
($state.files|Where-Object path -eq ($prefix+'StickyBusinessAccess.dll')).sha256=Hash $plugin
$state.files+=[PSCustomObject]@{path=$prefix+'nvdaControllerClient64.dll';sha256=Hash $controller}
$state.version='0.8.1'
$state|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt
$oldReceiptHash=Hash $receipt; $oldPluginHash=Hash $plugin; $oldControllerHash=Hash $controller
$settings=Join-Path $game 'BepInEx\config\local.stickybusiness.access.cfg'
[IO.File]::WriteAllText($settings,'saved keybindings fixture')
$settingsHash=Hash $settings
$controllerBytes=[IO.File]::ReadAllBytes($controller)
[IO.File]::WriteAllText($controller,'modified controller fixture')
Refuses {& (Join-Path $Package 'update.ps1') -GameDir $game} 'Modified old dependency refused before migration'
Check ((Hash $receipt) -eq $oldReceiptHash -and (Hash $plugin) -eq $oldPluginHash -and !(Test-Path -LiteralPath $prism)) 'Refusal leaves receipt/plugin unchanged and no Prism installed'
[IO.File]::WriteAllBytes($controller,$controllerBytes)
[IO.File]::WriteAllText($prism,'unowned Prism fixture')
Refuses {& (Join-Path $Package 'update.ps1') -GameDir $game} 'Unowned Prism dependency refused'
Remove-Item -LiteralPath $prism
& (Join-Path $Package 'setup.ps1') -GameDir $game
Check ((Hash $prism) -eq (Hash (Join-Path $Package ('payload\'+$prefix+'prism.dll')))) 'Upgrade installs exact packaged Prism dependency'
Check (!(Test-Path -LiteralPath $controller)) 'Upgrade removes only the receipt-owned unchanged old controller'
Check ((Hash $settings) -eq $settingsHash) 'Upgrade preserves saved settings/keybindings'
$state=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
Check ($state.version -eq '0.8.2' -and @($state.files|Where-Object path -eq ($prefix+'prism.dll')).Count -eq 1 -and @($state.files|Where-Object path -eq ($prefix+'nvdaControllerClient64.dll')).Count -eq 0) 'Receipt updates dependency ownership'
$backup=(Get-ChildItem -LiteralPath (Join-Path $Package 'backups') -Directory|Sort-Object Name -Descending|Select-Object -First 1).FullName
& (Join-Path $Package 'update.ps1') -GameDir $game -RestoreBackup $backup
Check ((Hash $plugin) -eq $oldPluginHash -and (Hash $controller) -eq $oldControllerHash -and !(Test-Path -LiteralPath $prism)) 'Rollback restores prior mod/controller and removes newly owned Prism'
Check ((Hash $receipt) -eq $oldReceiptHash) 'Rollback restores exact prior receipt'
& (Join-Path $Package 'update.ps1') -GameDir $game
& (Join-Path $Package 'uninstall.ps1') -GameDir $game
Check (!(Test-Path -LiteralPath $prism) -and !(Test-Path -LiteralPath $plugin) -and (Hash $settings) -eq $settingsHash) 'Uninstall removes mod/Prism while preserving settings'
Write-Host '10 Prism upgrade/rollback checks passed; no game or screen reader was launched.'
