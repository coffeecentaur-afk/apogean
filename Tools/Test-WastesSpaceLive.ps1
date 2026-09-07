param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport)
# HISTORICAL log replay only. Current motion contract uses Test-WastesHeightLive.ps1.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = Get-Content -LiteralPath $LogPath
foreach ($case in 'ground','space-fade','space-edge','sky','space-ascent','space-descent') {
    $records = @($lines | Where-Object { $_ -match "WASTES LAND RESULT: case=$case; viewport=$Viewport;" })
    if (-not $records.Count) { throw "MISSING_RESULT: $case at $Viewport" }
    if ($records[-1] -notmatch 'checks=(\d+); spaceChecks=(\d+); groundChecks=(\d+); partialFarChecks=(\d+); failures=(\d+);') { throw "BAD_RESULT: $case" }
    $checks=[int]$Matches[1]; $space=[int]$Matches[2]; $ground=[int]$Matches[3]; $partial=[int]$Matches[4]; $failures=[int]$Matches[5]
    if ($checks -lt 120 -or $failures -ne 0) { throw "LAND_FAILURE: $case" }
    if ($case -in 'space-edge','sky','space-ascent','space-descent' -and $space -lt 30) { throw "MISSING_SPACE_ABSENCE: $case" }
    if ($case -eq 'ground' -and $ground -lt 3) { throw "MISSING_GROUND_PRESENCE: $case" }
    if ($case -in 'space-fade','space-ascent','space-descent' -and $partial -lt 10) { throw "MISSING_GRADUAL_FADE: $case" }
    $samples=@($lines | Where-Object { $_ -match "WASTES LAND SAMPLE: case=$case;" -and $_ -match "viewport=$Viewport;" })
    if (-not $samples.Count) { throw "MISSING_DRAW_SAMPLE: $case" }
    foreach ($sample in $samples) {
        if ($sample -match 'failed=True') { throw "FAILED_DRAW_SAMPLE: $case" }
        if ($sample -match '(?:space|cameraSpace)=True;' -and $sample -notmatch 'factor=0\.00000; submittedAlpha=0;') { throw "VISIBLE_SPACE_SAMPLE: $case" }
    }
    if ($case -in 'space-ascent','space-descent') {
        $last=-1.0; $first=-1.0; $sawPartial=$false
        foreach ($sample in $samples) {
            if ($sample -notmatch 'layer=0;' -or $sample -notmatch 'factor=([\d.]+);') { continue }
            $value=[double]::Parse($Matches[1],[cultureinfo]::InvariantCulture)
            if ($first -lt 0) { $first=$value }
            if ($last -ge 0) {
                if (($case -eq 'space-ascent' -and $value -gt $last+.0001) -or ($case -eq 'space-descent' -and $value -lt $last-.0001)) { throw "NONMONOTONE_LIVE_FADE: $case" }
                if ([math]::Abs($last-$value) -gt .3) { throw "ABRUPT_LIVE_FADE: $case" }
            }
            if ($value -gt .05 -and $value -lt .95) { $sawPartial=$true }
            $last=$value
        }
        if (-not $sawPartial) { throw "MISSING_PARTIAL_DRAW_SAMPLE: $case" }
        # Camera rounding can put the ground endpoint half a pixel above the
        # datum. Assert actual full-color endpoints, not its rounded depth count.
        if ($case -eq 'space-ascent' -and ($first -ne 1 -or $last -ne 0)) { throw 'INCOMPLETE_ASCENT_ENDPOINTS' }
        if ($case -eq 'space-descent' -and ($first -ne 0 -or $last -ne 1)) { throw 'INCOMPLETE_DESCENT_ENDPOINTS' }
    }
    Write-Host "PASS: actual terrestrial color/presence/absence $case at $Viewport"
}
Write-Host 'Submitted-color evidence only; screenshots own visual composition and native Space handoff approval.'
