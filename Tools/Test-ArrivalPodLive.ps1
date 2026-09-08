param([Parameter(Mandatory)][string]$LogPath, [switch]$RequireReload)
$ErrorActionPreference = 'Stop'
$lines = @(Get-Content -LiteralPath $LogPath)
$start = -1
for ($i=0; $i -lt $lines.Count; $i++) { if ($lines[$i].Contains('ARRIVAL POD MATRIX START:')) { $start=$i } }
if ($start -lt 0) { throw 'NO_MATRIX_START' }
$run = @($lines[$start..($lines.Count-1)])
$expected = @('clean-test-slot-and-no-preexisting-pod-drops','registered-atlas-and-origin','non-solid-not-housing-or-light')
$expected += @('legacy-checkpoint-reproduces-terrain-frame-failure','saved-state-ignores-recomputed-terrain-frames',
    'saved-state-rejects-pod-frame-damage','saved-state-rejects-paint-change','saved-state-rejects-missing-ground',
    'snapshot-controls-restore-all-cells')
for($x=0;$x -lt 5;$x++){for($y=0;$y -lt 6;$y++){$expected += "copper-pick-power-cell-$x-$y-one-drop-no-remnants"}}
for($x=0;$x -lt 5;$x++){$expected += "reject-missing-anchor-$x"; $expected += "support-loss-$x-one-drop"}
foreach($liquid in @(0,1)){$expected += "liquid-$liquid-placement-and-framing"; $expected += "liquid-$liquid-recovery"}
$expected += @('native-explosion-veto','collision-free-full-footprint','recoverable-placeable-lava-safe-item',
    're-placement-then-removal-one-drop','world-and-bed-spawn-unchanged')
$actual = @($run | Where-Object { $_ -match 'ARRIVAL POD CHECK PASS: (.+)$' } | ForEach-Object { $_ -replace '^.*ARRIVAL POD CHECK PASS: ','' })
if($actual.Count -ne $expected.Count -or @(Compare-Object ($actual|Sort-Object) ($expected|Sort-Object)).Count){throw 'MISSING_OR_DUPLICATE_CHECK'}
$summary = @($run | Where-Object { $_ -match "ARRIVAL POD MATRIX PASS: checks=$($expected.Count); checkpoint=[A-F0-9]{64};" })
if($summary.Count -ne 1){throw 'MATRIX_NOT_COMPLETED'}
if(($run -join "`n") -match 'LIVE VALIDATION REQUEST FAILED: arrival-pod-|Pod assertion failed:'){throw 'LATER_FAILURE'}
if(!($run -match 'ARRIVAL POD GROVE GUARD: unchanged=True;')){throw 'NO_GROVE_GUARD'}
if($RequireReload){
    $hash = [regex]::Match($summary[0], 'checkpoint=([A-F0-9]{64})').Groups[1].Value
    if(!($run -match "ARRIVAL POD RELOAD PASS: unchanged=$hash; three-objects-and-empty-test-slot=True; no-build-on-load=True")){throw 'RELOAD_NOT_PROVEN'}
}
Write-Output "PASS: $($expected.Count) native programmatic checks, complete last run and grove guard; reload=$RequireReload. Not manual input, visual approval, multiplayer, or worldgen evidence."
