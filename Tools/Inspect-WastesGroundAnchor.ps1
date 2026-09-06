param([switch]$RequireLocalGround)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$source=@('WastesCameraProjection','WastesParallaxContract','WastesModularLayout') | ForEach-Object {
    (Get-Content -Raw (Join-Path $repoRoot "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;',''
}
Add-Type -TypeDefinition ("using System;`n"+($source -join "`n"))
$failures=0
# Coordinate fixtures, not claimed measurements of a saved world's terrain.
# Identical standing views on two different elevations must put the nominal soil
# lip 48 world pixels above THEIR ground, not above an unrelated global datum.
foreach($floor in @(9584,9904)) {
    $camera=$floor-540
    $top=[apogean.Common.Backgrounds.WastesModularLayout]::Top(649,$camera,1080,2,1)
    $actual=$top+[apogean.Common.Backgrounds.WastesModularLayout]::CloseSoilRow
    $expected=492
    $anchorError=[Math]::Abs($actual-$expected)
    if($anchorError -gt 1){$failures++}
    Write-Output "floorWorldY=$floor; actualSoilScreenY=$actual; expectedHeadHeightY=$expected; errorPx=$anchorError; localGroundPass=$($anchorError -le 1)"
}
if($RequireLocalGround -and $failures -gt 0){throw 'LOCAL_GROUND_ANCHOR_MISSING: current renderer accepts only the global worldSurface datum'}
