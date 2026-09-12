param([Parameter(Mandatory)][ValidateSet('snapshot','start','stop','allocations','shallow-static','shallow-sweep','conversions-small','conversions-large')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "qa-perf-$Case"
if($Case -eq 'allocations') {
    Write-Output 'Queued allocation probe; inspect the fresh QA RENDER ALLOCATION summary, including skipped cases and output equality. Queue success is not a pass.'
} else {
    Write-Output "Queued qa-perf-$Case; wait for fresh native QA PERFORMANCE EXPORT/COMPLETE. Queue success is not a measurement."
}
