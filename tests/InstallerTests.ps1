param([Parameter(Mandatory=$true)][string]$Package,[Parameter(Mandatory=$true)][string]$CleanGame,[Parameter(Mandatory=$true)][string]$Scratch)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$Package=(Resolve-Path -LiteralPath $Package).Path
if(Test-Path -LiteralPath (Join-Path $Package 'support\setup.ps1')){$Package=Join-Path $Package 'support'}
$CleanGame=(Resolve-Path -LiteralPath $CleanGame).Path
if(Test-Path -LiteralPath $Scratch){throw 'Choose a new scratch folder.'}
New-Item -ItemType Directory -Path $Scratch|Out-Null
$Scratch=(Resolve-Path -LiteralPath $Scratch).Path
$game=Join-Path $Scratch 'Clean game with spaces'
New-Item -ItemType Directory -Path $game|Out-Null
$names=@('StickyBusiness.exe','UnityPlayer.dll','GameAssembly.dll','StickyBusiness_Data\data.unity3d','StickyBusiness_Data\il2cpp_data\Metadata\global-metadata.dat')
foreach($name in $names){
    $to=Join-Path $game $name
    New-Item -ItemType Directory -Path (Split-Path -Parent $to) -Force|Out-Null
    Copy-Item -LiteralPath (Join-Path $CleanGame $name) -Destination $to
}
function Assert($ok,$message){if(!$ok){throw $message};Write-Host "PASS: $message"}
function Hash($path){(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash}
function Refuses($action,$message){$failed=$false;try{& $action}catch{$failed=$true;Write-Host "Expected refusal: $($_.Exception.Message)"};Assert $failed $message}
$before=@{};foreach($name in $names){$before[$name]=Hash (Join-Path $game $name)}
. (Join-Path $Package 'setup-common.ps1')
Assert (@(Find-StickyBusiness -SteamRoots @((Join-Path $Scratch 'no Steam'))).Count -eq 0) 'Missing Steam detection returns no candidates'
$steam=Join-Path $Scratch 'Steam';$library=Join-Path $Scratch 'Other drive library'
New-Item -ItemType Directory -Path "$steam\steamapps","$library\steamapps\common\Sticky Business" -Force|Out-Null
Copy-Item -LiteralPath (Join-Path $CleanGame 'StickyBusiness.exe') -Destination "$library\steamapps\common\Sticky Business"
'"libraryfolders" { "1" { "path" "'+$library.Replace('\','\\')+'" } }'|Set-Content -LiteralPath "$steam\steamapps\libraryfolders.vdf"
'"AppState" { "appid" "2303350" "installdir" "Sticky Business" }'|Set-Content -LiteralPath "$library\steamapps\appmanifest_2303350.acf"
$found=@(Find-StickyBusiness -SteamRoots @($steam))
Assert ($found.Count -eq 1 -and $found[0] -eq "$library\steamapps\common\Sticky Business") 'Detects secondary Steam library with spaces'
& (Join-Path $Package 'setup.ps1') -GameDir $game
$receipt=Join-Path $game 'StickyBusinessAccess-install.json'
$plugin=Join-Path $game 'BepInEx\plugins\StickyBusinessAccess\StickyBusinessAccess.dll'
Assert ((Test-Path -LiteralPath $plugin) -and (Test-Path -LiteralPath "$game\dotnet\coreclr.dll")) 'Clean installation includes plugin and runtime'
Assert ((Test-Path -LiteralPath "$game\.doorstop_version")) 'Hidden Doorstop dependency included'
$settings=Join-Path $game 'BepInEx\config\local.stickybusiness.access.cfg'
$catalogue=Join-Path $game 'BepInEx\plugins\StickyBusinessAccess\descriptions.en.json'
Set-Content -LiteralPath $settings -Value '[AccessibilityKeys] Action116 = 123'
Set-Content -LiteralPath $catalogue -Value 'custom description fixture'
Set-Content -LiteralPath "$game\BepInEx\config\BepInEx.cfg" -Value '[Logging.Console] Enabled = false # custom'
New-Item -ItemType Directory -Path "$game\BepInEx\interop","$game\BepInEx\unity-libs" -Force|Out-Null
Set-Content -LiteralPath "$game\BepInEx\interop\Generated.dll" -Value 'generated fixture'
Set-Content -LiteralPath "$game\BepInEx\unity-libs\UnityEngine.dll" -Value 'generated fixture'
Set-Content -LiteralPath "$game\BepInEx\LogOutput.log" -Value 'generated log'
$prefs=@{};foreach($path in @($settings,$catalogue,"$game\BepInEx\config\BepInEx.cfg")){$prefs[$path]=Hash $path}
$pluginHash=Hash $plugin
& (Join-Path $Package 'setup.ps1') -GameDir $game
foreach($path in $prefs.Keys){Assert ((Hash $path) -eq $prefs[$path]) "Update preserves $([IO.Path]::GetFileName($path))"}
$backup=(Get-ChildItem -LiteralPath "$Package\backups" -Directory|Sort-Object Name -Descending|Select-Object -First 1).FullName
& (Join-Path $Package 'update.ps1') -GameDir $game -RestoreBackup $backup
Assert ((Hash $plugin) -eq $pluginHash) 'Rollback restores plugin'
& (Join-Path $Package 'uninstall.ps1') -GameDir $game
Assert (!(Test-Path -LiteralPath "$game\winhttp.dll") -and !(Test-Path -LiteralPath $plugin)) 'Uninstall removes loader injection and plugin'
Assert ((Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json).status -eq 'uninstalled') 'Uninstall provenance retained'
& (Join-Path $Package 'setup.ps1') -GameDir $game
foreach($path in $prefs.Keys){Assert ((Hash $path) -eq $prefs[$path]) "Reinstall preserves $([IO.Path]::GetFileName($path))"}
foreach($name in $names){Assert ((Hash (Join-Path $game $name)) -eq $before[$name]) "Original unchanged: $name"}
Refuses {& (Join-Path $Package 'install.ps1') -GameDir $game} 'Direct fresh installer refuses installed receipt'
Set-Content -LiteralPath $plugin -Value 'modified binary fixture'
Refuses {& (Join-Path $Package 'update.ps1') -GameDir $game} 'Update protects modified plugin'
& (Join-Path $Package 'uninstall.ps1') -GameDir $game
Assert ((Get-Content -LiteralPath $plugin) -eq 'modified binary fixture') 'Uninstall preserves modified executable'
Refuses {& (Join-Path $Package 'install.ps1') -GameDir $game} 'Reinstall refuses modified executable'
$conflict=Join-Path $Scratch 'Existing loader game';New-Item -ItemType Directory -Path $conflict|Out-Null
foreach($name in $names){$to=Join-Path $conflict $name;New-Item -ItemType Directory -Path (Split-Path -Parent $to) -Force|Out-Null;Copy-Item -LiteralPath (Join-Path $CleanGame $name) -Destination $to}
Set-Content -LiteralPath "$conflict\winhttp.dll" -Value 'another mod loader'
Refuses {& (Join-Path $Package 'install.ps1') -GameDir $conflict} 'Unknown loader protected'
Assert (!(Test-Path -LiteralPath "$conflict\StickyBusinessAccess-install.json")) 'Conflict leaves no installation changes'
$payloadDll=Join-Path $Package 'payload\BepInEx\plugins\StickyBusinessAccess\StickyBusinessAccess.dll'
$bytes=[IO.File]::ReadAllBytes($payloadDll)
try {
    Set-Content -LiteralPath $payloadDll -Value 'corrupt download fixture'
    Refuses {Test-SetupPayload $Package} 'Corrupt release refused before installation'
} finally {[IO.File]::WriteAllBytes($payloadDll,$bytes)}
$manifestPath=Join-Path $Package 'payload-sha256.json'
$manifestBytes=[IO.File]::ReadAllBytes($manifestPath)
try {
    $manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
    $manifest[0].path='..\outside-payload.dll'
    $manifest|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $manifestPath -Encoding UTF8
    Refuses {Test-SetupPayload $Package} 'Manifest traversal refused'
} finally {[IO.File]::WriteAllBytes($manifestPath,$manifestBytes)}
$controller=Join-Path $Package 'payload\BepInEx\plugins\StickyBusinessAccess\prism.dll'
$controllerBytes=[IO.File]::ReadAllBytes($controller)
try {
    Remove-Item -LiteralPath $controller
    Refuses {Test-SetupPayload $Package} 'Incomplete extraction refused'
} finally {[IO.File]::WriteAllBytes($controller,$controllerBytes)}
Write-Host 'Installer lifecycle and safety checks complete. No human NVDA testing is claimed.'

