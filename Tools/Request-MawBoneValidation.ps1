param(
 [Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','reference')][string]$Case,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-bone-$Case" -CaptureDirectory $CaptureDirectory
Write-Host "Queued maw-bone-$Case. Only gg in Apogee Native Visual V3 single-player may consume it."
Write-Host 'Wait for consumption and inspect client.log before the next request. An unfocused game may pause; a queued request is not a test pass.'
