param([Parameter(Mandatory=$true)][string]$Archive)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$Archive=(Resolve-Path -LiteralPath $Archive).Path
$check=Join-Path ([IO.Path]::GetTempPath()) ('sticky-release-'+[Guid]::NewGuid().ToString('N'))
[IO.Compression.ZipFile]::ExtractToDirectory($Archive,$check)
foreach($name in @('Setup.bat','Uninstall.bat','START-HERE.txt','PLAYER-INSTRUCTIONS.md','support\setup.ps1','support\install.ps1','support\update.ps1','support\uninstall.ps1','support\setup-common.ps1')){
    if(!(Test-Path -LiteralPath (Join-Path $check $name) -PathType Leaf)){throw "Release archive missing: $name (extracted at $check)"}
}
. (Join-Path $check 'support\setup-common.ps1')
$files=@(Test-SetupPayload (Join-Path $check 'support'))
foreach($file in $files){
    $relative=$file.FullName.Substring($check.Length+1)
    if($relative -match '(?i)(^|\\)(interop|StickyBusiness_Data|DummyDll)(\\|$)|(^|\\)(GameAssembly|UnityPlayer)\.dll$|\.exe$'){
        throw "Unexpected game/development file in player package: $relative"
    }
}
Write-Host "Verified extracted release ZIP: $($files.Count) payload files; launchers and support scripts present. Inspection folder: $check"
