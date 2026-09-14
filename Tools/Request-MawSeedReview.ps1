param([Parameter(Mandatory)][ValidateSet('entrance','mouth','ribs','cache','node','outlet','lower-return','pristine','reload','seams','seam-trials','light-on','light-off','capture','release','save-and-quit')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-seed-$Case"
Write-Output 'Queued only. Requires a named generated Maw Seed QA world and gg/Plain in single player. No saved-layout rebuild or held camera; seam checks use a restored empty scratch. Await fresh MAW SEED REVIEW COMPLETE.'
