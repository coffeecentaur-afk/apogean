param([Parameter(Mandatory)][string]$LogPath,[string]$Viewport='2560x1369')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$lines=Get-Content -LiteralPath $LogPath
foreach ($case in 'run-flat','run-diagonal') {
    $results=@($lines | Where-Object {$_ -match "RUN MOTION RESULT: case=$case;"})
    if (-not $results.Count) { throw "MISSING_RUN: $case" }
    $r=$results[-1]
    if ($r -notmatch "viewport=$Viewport; checks=(\d+); failures=0; travelX=([\d.]+); travelY=([\d.]+); maxError=([\d.]+); horizontal=0.200; vertical=0.060; simulatedCamera=True;") { throw "BAD_RUN: $r" }
    if ([int]$Matches[1] -lt 500 -or [double]$Matches[2] -lt 3599 -or [double]$Matches[4] -gt 1.05) { throw "SHORT_OR_REACTIVE_RUN: $case" }
    $yTravel=[double]$Matches[3]
    if (($case -eq 'run-flat' -and $yTravel -gt .01) -or ($case -eq 'run-diagonal' -and $yTravel -lt 127)) { throw "WRONG_RUN_PATH: $case" }
    $samples=@($lines | Where-Object {$_ -match "RUN MOTION SAMPLE: case=$case;"})
    if ($samples.Count -lt 15) { throw "MISSING_RUN_SAMPLES: $case" }
    $seen=@{}; $comparisons=0
    foreach ($s in $samples) {
        if ($s -notmatch 'cell=(-?\d+); cameraX=([-\d.]+); cameraY=([-\d.]+); pixelX=([-\d.]+); pixelY=([-\d.]+); horizontal=0.200; vertical=0.060;') { throw "BAD_RUN_SAMPLE: $s" }
        $cell=$Matches[1]
        $now=@{X=[double]$Matches[2];Y=[double]$Matches[3];PX=[double]$Matches[4];PY=[double]$Matches[5]}
        if ($seen.ContainsKey($cell)) {
            $old=$seen[$cell]
            if ([math]::Abs($now.PX-$old.PX+($now.X-$old.X)*.20) -gt 1.06 -or [math]::Abs($now.PY-$old.PY+($now.Y-$old.Y)*.06) -gt 1.06) { throw "REACTIVE_RUN_SAMPLE: $case/$cell" }
            $comparisons++
        }
        $seen[$cell]=$now
    }
    if ($comparisons -lt 10) { throw "SHORT_SHARED_SECTION_TRACE: $case" }
    Write-Host "PASS: $case at $Viewport; $comparisons independently replayed native position differences."
}
Write-Host 'Simulated running-camera response only. User comfort, physical running, coverage and real-biome routing remain separate checks.'
