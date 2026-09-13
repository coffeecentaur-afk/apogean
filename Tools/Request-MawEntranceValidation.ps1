param(
 [Parameter(Mandatory)][ValidateSet('survey','surface','shallow')][string]$Case,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request $Case -CaptureDirectory $CaptureDirectory -RequestFileName 'ApogeanMawEntrance.request'
Write-Host "Requested $Case. Only gg in a disposable Apogee Arrival QA world can consume it. No terrain construction."
