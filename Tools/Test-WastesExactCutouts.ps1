param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$baseline=Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/Derived'
$candidate=Join-Path $repoRoot 'Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1'
$runner=(Get-Process -Id $PID).Path
$probe=Join-Path $PSScriptRoot 'Inspect-WastesCutoutFeedback.ps1'
# Replay the same semantic connectivity check against archived bad and repaired
# assets. Broad pale-edge warnings remain visible, not reclassified as approval.
foreach($case in @(@{Root=$baseline;Pass=$false},@{Root=$candidate;Pass=$true})) {
    $result=& $runner -NoProfile -File $probe -AssetRoot $case.Root -RequireTreeConnections 2>&1
    $passed=$LASTEXITCODE -eq 0
    if($passed -ne $case.Pass){throw "WRONG_CONNECTIVITY_VERDICT: $($case.Root)`n$result"}
    if(-not $passed -and "$result" -notmatch 'TREE_CONNECTION_FAILURE'){throw 'BASELINE_FAILED_FOR_WRONG_REASON'}
    Write-Output "PASS expected tree connectivity=$($case.Pass): $($case.Root)"
}
& (Join-Path $PSScriptRoot 'Test-BackgroundReplacement.ps1') -ReferencePath (Join-Path $baseline 'Station.png') -CandidatePath (Join-Path $candidate 'Station.png') -AllowedRectangles '30,365,56,75','410,350,60,91'
if($LASTEXITCODE -ne 0){throw 'STATION_EXPORT_FAILED'}
& (Join-Path $PSScriptRoot 'Test-BackgroundReplacement.ps1') -ReferencePath (Join-Path $baseline 'Foreground-Deep.png') -CandidatePath (Join-Path $candidate 'Foreground-Deep.png') -AllowedRectangles '310,195,190,90','640,225,85,80','1120,230,125,75' -PreserveAlphaMask
if($LASTEXITCODE -ne 0){throw 'FOREGROUND_EXPORT_FAILED'}
Write-Output 'PASS: bounded export/connectivity proof only; live appearance and general halo clearance remain pending.'
