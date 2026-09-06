param(
    [Parameter(Mandatory)][string]$LogPath,
    [Parameter(Mandatory)][string]$Viewport
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$lines=Get-Content -LiteralPath $LogPath
foreach($case in 'diagonal-left','diagonal-right') {
    $sample=@($lines | Where-Object { $_ -match "PROJECTION SAMPLE: case=$case;" -and $_ -match "viewport=$Viewport;" })
    if(-not $sample.Count){throw "FAIL: missing actual engine matrix sample for $case at $Viewport"}
    $summary=@($lines | Where-Object {$_ -match "PROJECTION: case=$case;"})
    if(-not $summary.Count){throw "FAIL: missing completed projection summary for $case"}
    if($summary[-1] -notmatch 'checks=(\d+); failures=(\d+); maxLeftGap=([\d.-]+); maxRightGap=([\d.-]+); min(?:Close|Middle|Far)BottomMargin=([\d.-]+);') {
        throw "FAIL: malformed projection summary for $case"
    }
    if([int]$Matches[1] -lt 300 -or [int]$Matches[2] -ne 0 -or [double]$Matches[3] -gt 1 -or [double]$Matches[4] -gt 1 -or [double]$Matches[5] -lt -1) {
        throw "FAIL: uncovered submitted geometry during $case at ${Viewport}: $($summary[-1])"
    }
    $sweep=@($lines | Where-Object {$_ -match "SWEEP: case=$case;"})
    if(-not $sweep.Count -or $sweep[-1] -notmatch 'farRepeats=([\d.]+);' -or [double]$Matches[1] -lt 2.5) {
        throw "FAIL: incomplete far-layer repeat coverage for $case"
    }
    Write-Host "PASS: $case at $Viewport covers both edges/bottom through >=2.5 far repeats."
}
Write-Host 'Geometry telemetry only: terrain occlusion, texture-alpha seams, comfort and art require screenshot/user review.'
