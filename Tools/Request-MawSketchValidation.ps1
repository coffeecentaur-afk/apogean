param([Parameter(Mandatory)][ValidateSet('build','pristine','reload','entrance','ribs','cave','lower','light-on','light-off','capture','release')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-sketch-$Case"
Write-Output 'Queued only; await fresh MAW SKETCH COMPLETE and subsequent render. Visits never hold player movement.'
