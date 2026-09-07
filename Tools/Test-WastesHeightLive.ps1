param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport,
    [ValidateSet('ground-pan-left','ground-pan-right','space-ascent','space-descent')]
    [string[]]$Cases = @('ground-pan-left','ground-pan-right','space-ascent','space-descent'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = Get-Content -LiteralPath $LogPath
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
            if ($record -notmatch 'capped=(\d+); exited=(\d+);' -or [int]$Matches[1] -lt 60 -or [int]$Matches[2] -lt 60) { throw "MISSING_CAP_AND_EXIT: $case" }
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
    if ($case -like 'space-*') {
        foreach ($layer in 0,1) {
            $samples = @($lines | Where-Object { $_ -match "WASTES HEIGHT POSITION SAMPLE: case=$case; layer=$layer; viewport=$Viewport;" })
            if ($samples.Count -lt 10) { throw "MISSING_POSITION_SAMPLES: $case/$layer" }
            $sawCapped = $false; $sawUncapped = $false; $sawExit = $false
            $previous = $null
            foreach ($s in $samples) {
                # Do not pass an archived higher-ceiling trace as today's build.
                # Replay pre-lowering evidence with the validator at commit0ec6d6c.
                $fraction = if ($layer -eq 1) { '0.250' } else { '0.500' }
                if (-not $s.EndsWith("capFraction=$fraction")) { throw "WRONG_CAP_PROFILE: $case/$layer" }
                if ($s -notmatch 'cameraCenter=([-\d.]+); cap=([-\d.]+); top=([-\d.]+); expected=([-\d.]+); zoom=([\d.]+); exited=(True|False); failed=False') { throw "BAD_POSITION: $s" }
                $center = [double]::Parse($Matches[1],[cultureinfo]::InvariantCulture)
                $cap = [double]::Parse($Matches[2],[cultureinfo]::InvariantCulture)
                $top = [double]::Parse($Matches[3],[cultureinfo]::InvariantCulture)
                $expected = [double]::Parse($Matches[4],[cultureinfo]::InvariantCulture)
                $zoom = [double]::Parse($Matches[5],[cultureinfo]::InvariantCulture)
                $sawCapped = $sawCapped -or $center -lt $cap
                $sawUncapped = $sawUncapped -or $center -ge $cap
                $sawExit = $sawExit -or $Matches[6] -eq 'True'
                if ([math]::Abs($top-$expected) -gt .025) { throw "POSITION_MISMATCH: $case/$layer" }
                if ($null -ne $previous -and $center -lt $cap -and $previous.center -lt $cap) {
                    # Native logs round zoom to three decimals. Bound the error
                    # from that quantization; do not require unlogged precision.
                    $tolerance = .03 + [math]::Abs($previous.center-$center)*.0005
                    if ([math]::Abs(($top-$previous.top)-($previous.center-$center)*$zoom) -gt $tolerance) { throw "SCREEN_FOLLOWING_CAP: $case/$layer" }
                }
                $previous = @{center=$center;top=$top}
            }
            if (-not ($sawCapped -and $sawUncapped -and $sawExit)) { throw "INCOMPLETE_FLIGHT: $case/$layer" }
        }
    }
    Write-Host "PASS: native $case at $Viewport"
}
Write-Host 'Native camera/alpha/anchor telemetry only. Screenshots, real-biome routing and other viewports remain separate evidence.'
