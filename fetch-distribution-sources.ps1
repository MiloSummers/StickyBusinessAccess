param([string]$Destination=(Join-Path $PSScriptRoot 'third-party-sources'))
$ErrorActionPreference='Stop'
$record=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'docs\DEPENDENCY-SOURCES.json') -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
foreach($entry in $record.distributionSources){
    $target=Join-Path $Destination $entry.file
    if(!(Test-Path -LiteralPath $target)){
        $temporary=$target+'.download'
        try {
            Invoke-WebRequest -Uri $entry.url -OutFile $temporary -UseBasicParsing
            if((Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash -ne $entry.sha256){throw "Source archive checksum mismatch: $($entry.file)"}
            Move-Item -LiteralPath $temporary -Destination $target
        } finally {if(Test-Path -LiteralPath $temporary){Remove-Item -LiteralPath $temporary}}
    }
    if((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $entry.sha256){throw "Existing source archive checksum mismatch: $($entry.file)"}
    Write-Host "Verified: $($entry.file)"
}
