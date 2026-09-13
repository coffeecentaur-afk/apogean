param(
	[Parameter(Mandatory)][ValidateSet('build','test','day','inside','night','reload','release')][string]$Case,
	[string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader')
)
$ErrorActionPreference = 'Stop'
$directory = Join-Path $TModLoaderRoot 'Captures'
New-Item -ItemType Directory -Path $directory -Force | Out-Null
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "arrival-pod-$Case" -CaptureDirectory $directory
Write-Host "Requested arrival-pod-$Case (gg / disposable QA world / single-player only)."
