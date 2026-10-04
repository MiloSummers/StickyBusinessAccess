param([Parameter(Mandatory=$true)][string]$LoaderDir,[Parameter(Mandatory=$true)][string]$InteropDir)
$ErrorActionPreference='Stop'
dotnet build "$PSScriptRoot\src\StickyBusinessAccess.csproj" -c Release "-p:LoaderDir=$LoaderDir" "-p:InteropDir=$InteropDir"
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
