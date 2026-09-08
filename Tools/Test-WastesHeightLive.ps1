param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport,
    [ValidateSet('ground-pan-left','ground-pan-right','space-ascent','space-descent','flight-turnaround')]
    [string[]]$Cases = @('ground-pan-left','ground-pan-right','space-ascent','space-descent'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = Get-Content -LiteralPath $LogPath
$viewHeight=[int]($Viewport.Split('x')[1])
foreach ($case in $Cases) {
    foreach ($kind in 'HEIGHT ALPHA','HEIGHT POSITION','CLOSE ANCHOR','MODULAR') {
        $records = @($lines | Where-Object { $_ -match "WASTES $kind RESULT: case=$case;" })
        if (-not $records.Count) { throw "MISSING_RESULT: $case/$kind" }
        $record = $records[-1]
        if ($kind -like 'HEIGHT*' -and $record -notmatch "viewport=$Viewport;") { throw "WRONG_VIEWPORT: $case/$kind" }
        if ($record -notmatch 'failures=0;') { throw "FAILED_RESULT: $case/$kind $record" }
        if ($kind -ne 'MODULAR' -and ($record -notmatch 'checks=(\d+);' -or [int]$Matches[1] -lt 120)) { throw "SHORT_TRACE: $case/$kind" }
        if ($kind -eq 'CLOSE ANCHOR' -and $case -like 'ground-pan-*' -and ($record -notmatch 'sections=(\d+);' -or [int]$Matches[1] -lt 12)) { throw "SHORT_ANCHOR_SWEEP: $case" }
        if ($kind -eq 'HEIGHT POSITION' -and $case -like 'space-*') {
            if ($record -notmatch 'eased=(\d+); exited=(\d+);' -or [int]$Matches[1] -lt 60 -or [int]$Matches[2] -lt 60) { throw "MISSING_EASE_AND_EXIT: $case" }
        }
    }
    if ($case -like 'ground-pan-*') {
        $sweep = @($lines | Where-Object { $_ -match "WASTES V1 SWEEP: case=$case;" })
        if (-not $sweep.Count -or $sweep[-1] -notmatch 'coveragePass=True;') { throw "INCOMPLETE_HORIZONTAL_SWEEP: $case" }
        $lock = @($lines | Where-Object { $_ -match "WASTES V1 GROUND LOCK: case=$case;" })
        if (-not $lock.Count -or $lock[-1] -notmatch 'pass=True;') { throw "GROUND_PROJECTION_FAILURE: $case" }
    }
    $alphaSamples = @($lines | Where-Object { $_ -match "WASTES HEIGHT ALPHA SAMPLE: case=$case;" })
    if ($alphaSamples.Count -lt 3) { throw "MISSING_ALPHA_SAMPLES: $case" }
    foreach ($sample in $alphaSamples) {
        if ($sample -notmatch 'factor=1\.00000;' -or $sample -match 'failed=True') { throw "HEIGHT_FADE: $case" }
    }
    if ($case -like 'space-*' -or $case -eq 'flight-turnaround') {
        foreach ($layer in 0,1,2) {
            $samples = @($lines | Where-Object { $_ -match "WASTES HEIGHT POSITION SAMPLE: case=$case; layer=$layer; viewport=$Viewport;" })
            if ($samples.Count -lt 10) { throw "MISSING_POSITION_SAMPLES: $case/$layer" }
            $sawCapped = $false; $sawUncapped = $false; $sawExit = $false; $sawHold = $false
            $previousByRegion = @{}
            $sawRise=$false; $sawDescent=$false; $sawMidHold=$false; $lastAltitude=0.0
            foreach ($s in $samples) {
                # Historical hard-cap traces cannot approve the new smooth policy.
                if (-not $s.EndsWith('profile=smooth-flight-v1')) { throw "WRONG_FLIGHT_PROFILE: $case/$layer" }
                if ($s -notmatch 'cameraCenter=([-\d.]+); ground=([-\d.]+); space=([-\d.]+); regional=([-\d.]+); top=([-\d.]+); expected=([-\d.]+); zoom=([\d.]+); exited=(True|False); failed=False; tick=(\d+);') { throw "BAD_POSITION: $s" }
                $center = [double]::Parse($Matches[1],[cultureinfo]::InvariantCulture)
                $ground = [double]::Parse($Matches[2],[cultureinfo]::InvariantCulture)
                $space = [double]::Parse($Matches[3],[cultureinfo]::InvariantCulture)
                $regional = [double]::Parse($Matches[4],[cultureinfo]::InvariantCulture)
                $top = [double]::Parse($Matches[5],[cultureinfo]::InvariantCulture)
                $expected = [double]::Parse($Matches[6],[cultureinfo]::InvariantCulture)
                $zoom = [double]::Parse($Matches[7],[cultureinfo]::InvariantCulture)
                $exited=$Matches[8] -eq 'True'; $tick=[int]$Matches[9]
                if($ground -le $space -or $zoom -le 0){throw 'BAD_REFERENCE'}
                $a=($ground-$center)/($ground-$space)
                $u=[math]::Max(0.0,($a-.1)/.9)
                $area=if($u -lt 1){[math]::Pow($u,3)-.5*[math]::Pow($u,4)}else{$u-.5}
                $extra=2*[math]::Max(0.0,$viewHeight+64-($viewHeight*.57-740-$viewHeight*.05*.012)-($ground-$space)*.012)*@(1.0,1.3,1.6)[$layer]*$area
                $oracle=if($layer -eq 2){$viewHeight*.5-48*$zoom+($regional-$center)*.06-330+$extra}else{$viewHeight*(.57+$layer*.025)-740+($ground-$center-$viewHeight*.05)*@(.012,.03)[$layer]+$extra+@(0,220)[$layer]}
                if([math]::Abs($top-$oracle) -gt .025){throw "INDEPENDENT_POSITION_MISMATCH: $case/$layer"}
                if($exited -ne ($top -ge $viewHeight)){throw 'EXIT_FLAG_MISMATCH'}
                $sawCapped = $sawCapped -or $a -gt .1
                $sawUncapped = $sawUncapped -or $a -le .1
                $sawExit = $sawExit -or $exited
                if ([math]::Abs($top-$expected) -gt .025) { throw "POSITION_MISMATCH: $case/$layer" }
                # Multiple Close sections are logged in each frame. Compare a
                # region with its own prior sample, not its adjacent neighbor.
                $key=$regional.ToString('R',[cultureinfo]::InvariantCulture)
                if ($previousByRegion.ContainsKey($key)) {
                    $previous=$previousByRegion[$key]
                    if($tick -gt $previous.tick){
                        $sawRise=$sawRise -or $center -lt $previous.center
                        $sawDescent=$sawDescent -or $center -gt $previous.center
                        if($center -eq $previous.center -and $tick-$previous.tick -le 60){
                            if([math]::Abs($top-$previous.top) -gt .002){throw 'STOP_DRIFT'}
                            $sawHold=$true
                            $sawMidHold=$sawMidHold -or ($a -gt .3 -and $a -lt .8)
                        }
                    }
                }
                $previousByRegion[$key] = @{center=$center;top=$top;tick=$tick}
                $lastAltitude=$a
            }
            if (-not ($sawCapped -and $sawUncapped -and $sawHold -and ($sawExit -or $case -eq 'flight-turnaround'))) { throw "INCOMPLETE_FLIGHT: $case/$layer" }
            if($case -eq 'flight-turnaround' -and -not ($sawRise -and $sawDescent -and $sawMidHold -and [math]::Abs($lastAltitude) -lt .0002)) {throw "INCOMPLETE_TURNAROUND: $case/$layer"}
        }
    }
    Write-Host "PASS: native $case at $Viewport"
}
Write-Host 'Native camera/alpha/anchor telemetry only. Screenshots, real-biome routing and other viewports remain separate evidence.'
