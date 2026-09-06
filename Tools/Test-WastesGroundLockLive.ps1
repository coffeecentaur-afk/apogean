param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$lines=Get-Content -LiteralPath $LogPath
foreach($case in 'ground','wings','diagonal-left','diagonal-right') {
    $samples=@($lines | Where-Object {$_ -match "GROUND SAMPLE: case=$case; viewport=$Viewport;"})
    if(-not $samples.Count){throw "FAIL: missing $case ground projection at $Viewport"}
    $reports=@($lines | Where-Object {$_ -match "GROUND LOCK: case=$case;"})
    if(-not $reports.Count -or $reports[-1] -notmatch 'checks=(\d+); maxError=([\d.]+); pass=True;' -or [int]$Matches[1] -lt 120 -or [double]$Matches[2] -gt 1.1){throw "FAIL: incomplete or drifting world anchor for $case"}
}
Write-Host "PASS: engine zoom-matrix comparison for all four ground/flight cases at $Viewport. Fixed world datum only, not regional terrain or visual acceptance."
