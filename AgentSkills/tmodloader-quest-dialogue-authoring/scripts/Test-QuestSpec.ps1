param(
    [Parameter(Mandatory = $true)][string]$Spec
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$data = Get-Content -Raw -LiteralPath (Resolve-Path -LiteralPath $Spec).Path | ConvertFrom-Json
$failures = [Collections.Generic.List[string]]::new()

foreach ($field in @('id', 'schemaVersion', 'availability', 'entry', 'stages', 'reward', 'dialogue', 'multiplayer', 'failure', 'nextAction')) {
    if ($null -eq $data.PSObject.Properties[$field] -or $null -eq $data.$field) { $failures.Add("missing root field '$field'") }
}
if ($null -ne $data.schemaVersion -and [int]$data.schemaVersion -lt 1) { $failures.Add('schemaVersion must be positive') }

if ($null -ne $data.availability) {
    foreach ($field in @('gate', 'cue')) {
        if ($null -eq $data.availability.PSObject.Properties[$field] -or $null -eq $data.availability.$field) { $failures.Add("availability missing '$field'") }
    }
    if (@($data.availability.cue).Count -lt 2) { $failures.Add('availability needs at least two redundant cues') }
}

$stageIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($stage in @($data.stages)) {
    foreach ($field in @('id', 'verb', 'target', 'completionEvent', 'worldEvidence')) {
        if ($null -eq $stage.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$stage.$field)) { $failures.Add("stage '$($stage.id)' missing '$field'") }
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$stage.id) -and -not $stageIds.Add([string]$stage.id)) { $failures.Add("duplicate stage '$($stage.id)'") }
}
if (@($data.stages).Count -eq 0) { $failures.Add('quest needs at least one stage') }

if ($null -ne $data.reward) {
    foreach ($field in @('ownership', 'delivery', 'duplicateProtection', 'fullInventory')) {
        if ($null -eq $data.reward.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$data.reward.$field)) { $failures.Add("reward missing '$field'") }
    }
}
if ($null -ne $data.dialogue) {
    foreach ($field in @('startNode', 'fallbackNode', 'nodes')) {
        if ($null -eq $data.dialogue.PSObject.Properties[$field] -or $null -eq $data.dialogue.$field) { $failures.Add("dialogue missing '$field'") }
    }
}
if ($null -ne $data.multiplayer) {
    foreach ($field in @('authority', 'lateJoin', 'mixedProgress', 'disconnect')) {
        if ($null -eq $data.multiplayer.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$data.multiplayer.$field)) { $failures.Add("multiplayer missing '$field'") }
    }
}
if ($null -ne $data.failure) {
    foreach ($field in @('death', 'abandonment', 'missingLandmark')) {
        if ($null -eq $data.failure.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$data.failure.$field)) { $failures.Add("failure missing '$field'") }
    }
}

if ($failures.Count -gt 0) {
    $failures | Select-Object -Unique | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: quest spec defines availability, observable stages, recoverable rewards, dialogue fallback, multiplayer ownership, and failure recovery.' -ForegroundColor Green
