param(
    [Parameter(Mandatory = $true)][string]$Spec
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$failures = [Collections.Generic.List[string]]::new()
$data = Get-Content -Raw -LiteralPath (Resolve-Path -LiteralPath $Spec).Path | ConvertFrom-Json

foreach ($field in @('id', 'progressionGate', 'encounterThesis', 'signatureRule', 'expectedGear', 'arenaAssumptions', 'phases', 'multiplayer', 'loot', 'testMatrix')) {
    if ($null -eq $data.PSObject.Properties[$field] -or $null -eq $data.$field) { $failures.Add("missing root field '$field'") }
}

if ($null -ne $data.phases) {
    $phaseIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($phase in @($data.phases)) {
        if ([string]::IsNullOrWhiteSpace([string]$phase.id)) { $failures.Add('phase without id') }
        elseif (-not $phaseIds.Add([string]$phase.id)) { $failures.Add("duplicate phase id '$($phase.id)'") }
        if ($null -eq $phase.attacks -or @($phase.attacks).Count -eq 0) { $failures.Add("phase '$($phase.id)' has no attacks"); continue }
        foreach ($attack in @($phase.attacks)) {
            foreach ($field in @('id', 'telegraph', 'dodgeAnswer', 'counterWindow', 'authority')) {
                if ($null -eq $attack.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$attack.$field)) {
                    $failures.Add("attack '$($attack.id)' missing '$field'")
                }
            }
            if ($null -eq $attack.telegraphSeconds -or [double]$attack.telegraphSeconds -le 0) {
                $failures.Add("attack '$($attack.id)' needs a positive telegraphSeconds value")
            }
        }
    }
}

if ($null -ne $data.multiplayer) {
    foreach ($field in @('stateAuthority', 'spawnAuthority', 'lateJoin', 'despawn', 'deathAndRevive')) {
        if ($null -eq $data.multiplayer.PSObject.Properties[$field] -or [string]::IsNullOrWhiteSpace([string]$data.multiplayer.$field)) {
            $failures.Add("multiplayer missing '$field'")
        }
    }
}

if ($null -ne $data.testMatrix) {
    foreach ($field in @('difficulties', 'playerCounts', 'classes', 'arenaCases', 'failureCases')) {
        if ($null -eq $data.testMatrix.PSObject.Properties[$field] -or @($data.testMatrix.$field).Count -eq 0) {
            $failures.Add("testMatrix missing non-empty '$field'")
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | Select-Object -Unique | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: boss encounter spec contains progression, decision-readable attacks, multiplayer authority, loot, and a complete test matrix.' -ForegroundColor Green
