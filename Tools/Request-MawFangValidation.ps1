param([Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','save-and-quit')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$request=if ($Case -eq 'save-and-quit') {'qa-save-and-quit'} else {"maw-fang-$Case"}
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request $request
Write-Host "Queued $request. Only gg / Apogee Native Visual V3 / SP. Inspect native log for consumption; queueing is not proof."
