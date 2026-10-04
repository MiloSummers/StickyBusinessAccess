# Shared Windows PowerShell 5.1-compatible setup helpers.
# Resolve inbox modules explicitly: inherited PowerShell 7 module paths must
# not shadow Windows PowerShell's own cmdlets when launched from a batch file.
foreach($module in @('Microsoft.PowerShell.Management','Microsoft.PowerShell.Utility')){
    Import-Module (Join-Path $PSHOME ('Modules\'+$module+'\'+$module+'.psd1')) -ErrorAction Stop
}
function Find-StickyBusiness {
    param([string[]]$SteamRoots)
    if(!$SteamRoots){
        $SteamRoots=@("${env:ProgramFiles(x86)}\Steam", "$env:ProgramFiles\Steam")
        $steam=Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue
        if($steam -and $steam.PSObject.Properties['SteamPath']){$SteamRoots+=$steam.SteamPath}
    }
    $roots=@($SteamRoots)
    foreach($root in @($roots)){
        $vdf=Join-Path $root 'steamapps\libraryfolders.vdf'
        if(Test-Path -LiteralPath $vdf){
            foreach($m in [regex]::Matches([IO.File]::ReadAllText($vdf),'"path"\s+"([^"]+)"')){$roots+=$m.Groups[1].Value.Replace('\\','\')}
        }
    }
    $found=@()
    foreach($root in @($roots|Select-Object -Unique)){
        $manifest=Join-Path $root 'steamapps\appmanifest_2303350.acf'
        if(Test-Path -LiteralPath $manifest){
            $m=[regex]::Match([IO.File]::ReadAllText($manifest),'"installdir"\s+"([^"]+)"')
            if($m.Success){
                $candidate=Join-Path $root ('steamapps\common\'+$m.Groups[1].Value)
                if(Test-Path -LiteralPath (Join-Path $candidate 'StickyBusiness.exe')){
                    # PowerShell -contains is case-insensitive for strings.
                    # Steam registry and defaults may spell the same path differently.
                    if($found -notcontains $candidate){$found+=$candidate}
                }
            }
        }
    }
    return @($found|Select-Object -Unique)
}
function Resolve-StickyBusiness {
    param([string]$GameDir,[switch]$Interactive)
    if(!$GameDir){
        $found=@(Find-StickyBusiness)
        if($found.Count -eq 1){$GameDir=$found[0]}
        elseif($Interactive){
            if($found.Count -gt 1){
                Write-Host 'More than one installation was found:'
                for($i=0;$i -lt $found.Count;$i++){Write-Host "$($i+1). $($found[$i])"}
                $choice=Read-Host 'Type its number, or press Enter to cancel'
                $number=0
                if([int]::TryParse($choice,[ref]$number) -and $number -ge 1 -and $number -le $found.Count){$GameDir=$found[$number-1]}
            } else {
                Write-Host 'Steam detection did not find Sticky Business.'
                Write-Host 'In Steam, use Manage > Browse local files and copy the folder address.'
                $GameDir=Read-Host 'Paste or type that folder here, or press Enter to cancel'
            }
        }
    }
    if(!$GameDir){throw 'No game folder selected. Install Sticky Business through Steam, then try Setup.bat again. Advanced use: -GameDir with the actual game folder.'}
    $GameDir=$GameDir.Trim().Trim('"')
    return (Resolve-Path -LiteralPath $GameDir).Path
}
function Get-SafeSetupPath {
    param([string]$Root,[string]$Relative)
    if([IO.Path]::IsPathRooted($Relative)){throw "Invalid absolute path in setup data: $Relative"}
    $path=[IO.Path]::GetFullPath((Join-Path $Root $Relative))
    if(!$path.StartsWith($Root.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Invalid path in setup data: $Relative"}
    return $path
}
function Test-SetupLinks {
    param([string]$Root)
    if((Get-Item -LiteralPath $Root).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Setup does not follow linked game folders. Select the real folder.'}
    foreach($name in @('BepInEx','dotnet')){
        $path=Join-Path $Root $name
        if(Test-Path -LiteralPath $path){
            $items=@(Get-Item -LiteralPath $path)+@(Get-ChildItem -LiteralPath $path -Recurse -Force)
            if($items|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Setup does not follow links under $name."}
        }
    }
}
function Test-SetupPayload {
    param([string]$Package)
    $payload=Join-Path $Package 'payload'
    if(!(Test-Path -LiteralPath $payload)){throw 'The payload is missing. Extract the entire player release ZIP, then run Setup.bat.'}
    $manifest=Get-Content -LiteralPath (Join-Path $Package 'payload-sha256.json') -Raw|ConvertFrom-Json
    $manifest=@($manifest)
    $files=@(Get-ChildItem -LiteralPath $payload -Recurse -File -Force)
    if($manifest.Count -ne $files.Count){throw 'The release payload is incomplete or contains unexpected files. Extract a fresh release ZIP.'}
    $seen=@{}
    foreach($entry in $manifest){
        $path=Get-SafeSetupPath $payload $entry.path
        if($seen.ContainsKey($path)){throw 'Duplicate payload manifest entry.'}
        $seen[$path]=$true
        if(!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $entry.sha256){throw "Release checksum failed: $($entry.path). Extract a fresh release ZIP."}
    }
    foreach($required in @('winhttp.dll','doorstop_config.ini','.doorstop_version','dotnet\coreclr.dll','BepInEx\core\BepInEx.Unity.IL2CPP.dll','BepInEx\plugins\StickyBusinessAccess\StickyBusinessAccess.dll','BepInEx\plugins\StickyBusinessAccess\prism.dll')){
        if(!(Test-Path -LiteralPath (Join-Path $payload $required))){throw "Required dependency missing: $required"}
    }
    return $files
}

function Test-CompatibleExistingLoader {
    param([string]$GameDir,[string]$Package)
    Test-SetupLinks $GameDir
    $null=Test-SetupPayload $Package
    if(Test-Path -LiteralPath (Join-Path $GameDir 'version.dll')){throw 'Another loader entry point (version.dll) is present. It has not been changed; this combination needs separate review.'}
    $core=Join-Path $GameDir 'BepInEx\core\BepInEx.Unity.IL2CPP.dll'
    if(!(Test-Path -LiteralPath $core)){throw 'This is not the required BepInEx 6 Unity.IL2CPP win-x64 loader. BepInEx 5 and Mono builds cannot be used. Existing files have not been changed.'}
    $version=(Get-Item -LiteralPath $core).VersionInfo.ProductVersion
    if($version -ne '6.0.0-be.788+5b766a3b7f6c164d4798924a93f3acf4db769d06'){throw "Existing loader version is $version. This release supports Unity.IL2CPP win-x64 be.788 only. No loader files were changed."}
    $manifest=Get-Content -LiteralPath (Join-Path $Package 'payload-sha256.json') -Raw|ConvertFrom-Json
    foreach($entry in $manifest){
        if($entry.path -match '^BepInEx\\plugins\\' -or $entry.path -eq 'BepInEx\config\BepInEx.cfg' -or $entry.path -eq 'doorstop_config.ini'){continue}
        $target=Get-SafeSetupPath $GameDir $entry.path
        if(!(Test-Path -LiteralPath $target -PathType Leaf) -or (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $entry.sha256){throw "The existing loader is incomplete or differs from the supported bundle: $($entry.path). No loader files were changed."}
    }
    # Read the required INI values; preserve unrelated custom loader options.
    $ini=Join-Path $GameDir 'doorstop_config.ini'
    if(!(Test-Path -LiteralPath $ini)){throw 'Existing Doorstop configuration is missing.'}
    $section='';$values=@{}
    foreach($line in Get-Content -LiteralPath $ini){
        $text=$line.Trim()
        if($text -match '^\[([^\]]+)\]$'){$section=$Matches[1];continue}
        if($text -match '^([^#;=]+?)\s*=\s*(.*?)\s*$'){
            $key=$section+'.'+$Matches[1].Trim()
            if($values.ContainsKey($key)){throw 'Ambiguous duplicate Doorstop setting. Existing configuration has not been changed.'}
            $values[$key]=$Matches[2].Trim()
        }
    }
    foreach($key in @('General.enabled','General.target_assembly','Il2Cpp.coreclr_path','Il2Cpp.corlib_dir')){
        if(!$values.ContainsKey($key)){throw "Required Doorstop setting missing: $key. Existing configuration has not been changed."}
    }
    if($values['General.enabled'] -ne 'true'){throw 'The existing loader is disabled. Its configuration has not been changed.'}
    foreach($pair in @(@('General.target_assembly','BepInEx\core\BepInEx.Unity.IL2CPP.dll'),@('Il2Cpp.coreclr_path','dotnet\coreclr.dll'),@('Il2Cpp.corlib_dir','dotnet'))){
        if((Get-SafeSetupPath $GameDir $values[$pair[0]]) -ne (Get-SafeSetupPath $GameDir $pair[1])){throw "Unsupported Doorstop path: $($pair[0]). Existing configuration has not been changed."}
    }
    Write-Host 'Compatible existing BepInEx Unity.IL2CPP win-x64 be.788 and runtime verified.'
}

