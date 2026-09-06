param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$standing=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,8990,1080,1280,2)
$flying=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,7790,1080,1280,2)
if([math]::Abs($flying-$standing-1200) -gt .01) {
    throw "FAIL: nearest ledge follows camera; 1200px climb moved it only $($flying-$standing)px, expected 1200px."
}
Write-Host 'PASS: nearest ledge follows the ground through a 1200px climb.'
$checks=0
foreach($height in 1080,1369,1440) {
    foreach($zoom in 1.0,(4.0/3.0),2.0) {
        foreach($lift in -1200,-96,0,96,600,1200,2400,4200) {
            $camera=9584-$height*.55-$lift
            $top=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,$camera,$height,1280,2,$zoom)
            $soil=$top+[apogean.Common.Backgrounds.WastesCameraProjection]::CloseSoilRow
            $terrain=(9584-$camera-$height*.5)*$zoom+$height*.5
            if([math]::Abs(($terrain-$soil)/$zoom-48) -gt .01){throw 'FAIL: soil lip is not three world tiles above reference ground'}
            $bottom=[apogean.Common.Backgrounds.WastesCameraProjection]::CoveredBottom($top,1280,$height)
            if($bottom -lt $height){throw 'FAIL: lower strata do not cover the screen'}
            if($lift -ge 2400 -and $top -le $height){throw 'FAIL: close layer still follows high flight'}
            $checks++
        }
    }
}
Write-Host "PASS: $checks viewport/zoom/altitude combinations preserve the 48px world offset and lower strata coverage."
