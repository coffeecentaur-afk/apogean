param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
# The user replaced tile-speed Close motion with a farther-back depth plane.
# Retain the ground-reference offset and high-flight exit, not the old speed.
$checks=0
foreach($height in 1080,1369,1440) {
    foreach($zoom in 1.0,(4.0/3.0),2.0) {
        foreach($lift in -1200,-96,0,96,600,1200,2400,4200) {
            $camera=9584-$height*.55-$lift
            $top=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,$camera,$height,1280,2,$zoom)
            $soil=$top+[apogean.Common.Backgrounds.WastesCameraProjection]::CloseSoilRow
            $center=$camera+$height*.5
            $cap=9584-(9584-3648)*.15
            $expected=$height*.5-48*$zoom+(9584-[math]::Max($center,$cap))*.06+[math]::Max(0.0,[double]($cap-$center))*$zoom
            if([math]::Abs($soil-$expected) -gt .02){throw 'FAIL: soil lip disagrees with anchored depth-plane projection'}
            $bottom=[apogean.Common.Backgrounds.WastesCameraProjection]::CoveredBottom($top,1280,$height)
            if($bottom -lt $height){throw 'FAIL: lower strata do not cover the screen'}
            if($lift -ge 2400 -and $top -le $height){throw 'FAIL: close layer still follows high flight'}
            $checks++
        }
    }
}
Write-Host "PASS: $checks depth-plane/viewport/zoom/altitude checks preserve the reference offset, flight exit and diagnostic lower strata coverage."
