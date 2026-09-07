param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
# HISTORICAL opacity policy. Current source uses capped geometry instead;
# run Test-WastesHeightLock.ps1. Retained only for old-revision replay.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$checks=0
foreach($surface in 350,500,649) {
    $ground=($surface-50)*16
    $sky=$surface*16*.35
    $previous=1.0
    foreach($step in 0..1000) {
        $fraction=$step/1000.0
        $height=$ground-($ground-$sky)*$fraction
        $alt=[apogean.Common.Backgrounds.WastesCameraProjection]::Altitude($surface,$height)
        $opacity=[apogean.Common.Backgrounds.WastesCameraProjection]::MiddleOpacity($alt)
        if([math]::Abs($alt-$fraction) -gt .00001){throw 'FAIL: altitude does not follow the fixed world-height span'}
        if($opacity -lt 0 -or $opacity -gt 1 -or $opacity -gt $previous+.00001){throw 'FAIL: nonmonotone/invalid middle opacity'}
        if([math]::Abs($opacity-$previous) -gt .005){throw 'FAIL: abrupt altitude transition'}
        if($fraction -le 1.0/3.0 -and $opacity -ne 1){throw 'FAIL: middle scenery not fully present at first third'}
        if($fraction -ge .701 -and $opacity -ne 0){throw 'FAIL: middle scenery still covers far layer high in sky'}
        $previous=$opacity
        $checks++
    }
}
Write-Host "PASS: $checks altitude steps across three world-height references; smooth middle-to-far staging. Not live art approval."
