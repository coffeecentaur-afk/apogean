param([switch]$RequireLocalGround,
    [ValidateSet('None','GlobalDatum','ShiftedSocket')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$source=@('WastesCameraProjection','WastesParallaxContract','WastesModularLayout','WastesGroundProfile') | ForEach-Object {
    (Get-Content -Raw (Join-Path $repoRoot "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;',''
}
Add-Type -TypeDefinition ("using System;`n"+($source -join "`n"))
$failures=0
$checks=0
# Coordinate fixtures, not claimed measurements of a saved world's terrain.
# Identical standing views on two different elevations must put the nominal soil
# lip 48 world pixels above THEIR ground, not above an unrelated global datum.
$rows=[int[]]@(599,599,619,619)
$profile=[apogean.Common.Backgrounds.WastesGroundProfile]::new($rows)
foreach($x in @(0,1536)) {
    $floor=$profile.GroundAt($x)
    foreach($height in @(1080,1369,1440)) {
        foreach($zoom in @(1.0,(4.0/3.0),2.0)) {
            foreach($lift in @(-400,0,96,1200,4200)) {
                $camera=$floor-$height*.5-$lift
                $ground=if($Mutation -eq 'GlobalDatum'){$null}else{$floor}
                $top=[apogean.Common.Backgrounds.WastesModularLayout]::Top(649,$camera,$height,2,$zoom,$ground)
                $socket=if($Mutation -eq 'ShiftedSocket'){334}else{330}
                $actual=$top+$socket
                $expected=($floor-48-$camera-$height*.5)*$zoom+$height*.5
                $anchorError=[Math]::Abs($actual-$expected)
                if($anchorError -gt .01){$failures++}
                $checks++
                if($height -eq 1080 -and $zoom -eq 1 -and $lift -eq 0) {
                    Write-Output "floorWorldY=$floor; actualSoilScreenY=$actual; expectedHeadHeightY=$expected; errorPx=$anchorError; localGroundPass=$($anchorError -le .01)"
                }
            }
        }
    }
}
# Snapshot ownership and interpolation use the same class as the game.
$rows[0]=100
$copy=$profile.CopyRows(); $copy[0]=100
if($profile.GroundAt(0) -ne 9584){throw 'MUTABLE_SNAPSHOT'}
$reloaded=[apogean.Common.Backgrounds.WastesGroundProfile]::new($profile.CopyRows())
foreach($x in @(-16,0,512,768,1024,1536,99999)) {
    if($profile.GroundAt($x) -ne $reloaded.GroundAt($x)){throw 'SNAPSHOT_RELOAD_DRIFT'}
}
if([Math]::Abs($profile.GroundAt(1024.01)-$profile.GroundAt(1023.99)) -gt .1){throw 'REGION_BOUNDARY_JUMP'}
if($profile.GroundAt(768) -ne 9744){throw 'BAD_INTERPOLATION'}
# Column fixture: sparse debris at y=500 must not replace sustained soil y=619.
$groundPredicate=[Func[int,int,bool]]{param($x,$y) ($y -eq 500) -or ($y -ge 619 -and $y -lt 621) -or ($y -ge 703)}
if([apogean.Common.Backgrounds.WastesGroundProfile]::FindSurface(0,400,745,599,$groundPredicate) -ne 619){throw 'COLUMN_SURFACE_MISSED'}
$emptyPredicate=[Func[int,int,bool]]{param($x,$y) $false}
if([apogean.Common.Backgrounds.WastesGroundProfile]::FindSurface(0,400,700,599,$emptyPredicate) -ne 599){throw 'UNBOUNDED_OR_WRONG_FALLBACK'}
if($RequireLocalGround -and $failures -gt 0){throw "LOCAL_GROUND_ANCHOR_FAILURE: $failures/$checks checks failed"}
Write-Output "Anchor checks=$checks; failures=$failures; snapshot/interpolation/column checks passed. Synthetic terrain, not live art approval."
