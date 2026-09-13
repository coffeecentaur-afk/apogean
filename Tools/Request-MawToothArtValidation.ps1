param(
 [Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','save-and-quit')][string]$Case,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$request=if($Case -eq 'save-and-quit'){'qa-save-and-quit'}else{"maw-tooth-art-$Case"}
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request $request -CaptureDirectory $CaptureDirectory
Write-Host "Queued $request. Require gg / V3 / SP, then inspect actual consumption. Art only."
