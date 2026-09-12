param([Parameter(Mandatory)][ValidateSet('locate','baseline','pattern','art','pan','origin','zoom','normal-zoom','report','release')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-cave-$Case"
Write-Output 'Await fresh MAW CAVE PROBE COMPLETE. This queues a QA-only, read-only cave-camera experiment, not a build or visual pass.'
