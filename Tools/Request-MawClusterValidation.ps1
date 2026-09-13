param(
 [Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','save-and-quit','orientation-build','orientation-test','orientation-day','orientation-night','orientation-capture','orientation-reload','orientation-release','orientation-audit','orientation-curves','orientation-proof','orientation-proof-view')][string]$Case,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$request=if($Case-eq'save-and-quit'){'qa-save-and-quit'}else{"maw-cluster-$Case"}
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request $request -CaptureDirectory $CaptureDirectory
Write-Host "Queued $request; require gg/V3/SP and verify consumption."
