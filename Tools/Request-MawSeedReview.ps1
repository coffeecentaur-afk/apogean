param([Parameter(Mandatory)][ValidateSet('entrance','ribs','cache','node','outlet','pristine','reload','light-on','light-off','capture','release','save-and-quit')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-seed-$Case"
Write-Output 'Queued only. Requires a named generated Maw Seed QA world and gg/Plain in single player. No terrain builds or held camera. Await fresh MAW SEED REVIEW COMPLETE.'
