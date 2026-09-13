param([Parameter(Mandatory)][ValidateSet('view','save-quit')][string]$Case,
 [string]$SaveRoot=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader'))
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request $Case -CaptureDirectory (Join-Path $SaveRoot 'Captures') -RequestFileName 'ApogeanArrivalSite.request'
Write-Host "Requested $Case; requires gg in a new Apogee Arrival QA world. Never edits terrain."
