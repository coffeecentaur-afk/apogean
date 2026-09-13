param(
 [Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release')][string]$Case,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-material-$Case" -CaptureDirectory $CaptureDirectory
Write-Output "Queued maw-material-$Case for gg/V3/SP only. Await actual consumption."
