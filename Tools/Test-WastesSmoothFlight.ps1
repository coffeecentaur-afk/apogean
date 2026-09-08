param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$checks=0
function Assert-Flight([bool]$ok,[string]$why) { if(-not $ok){throw "FAIL: $why"}; $script:checks++ }
function Top([double]$surface,[double]$center,[int]$height,[int]$layer,[single]$zoom) {
    [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,($center-$height*.5),$height,2048,$layer,$zoom)
}
# The actual reported defect: continuous position but a discontinuous slope.
# Keep the rejected old handover locations as regression probes, not new policy.
foreach($layer in 0..2) {
    $ground=9584; $span=$ground-3648
    $old=$ground-$span*@(.5,.25,.15)[$layer]
    $at=Top 649 $old 1369 $layer (4.0/3)
    $before=$at-(Top 649 ($old+1) 1369 $layer (4.0/3))
    $after=(Top 649 ($old-1) 1369 $layer (4.0/3))-$at
    Assert-Flight ([math]::Abs($after-$before) -lt .008) "ALTITUDE_SPEED_KINK layer=$layer before=$before after=$after"
}
foreach($surface in 250,500,649,700) {
 $ground=($surface-50)*16
 $space=([math]::Floor($surface*[double][single]0.35)+1)*16
 $span=$ground-$space
 foreach($height in 720,1080,1369,1440) { foreach($zoom in 1.0,(4.0/3),2.0) {
  $lastRates=$null
  for($step=0;$step -le 280;$step++) {
   $a=$step/256.0; $center=$ground-$span*$a
   $rates=@()
   foreach($layer in 0..2) {
    $top=Top $surface $center $height $layer $zoom
    $rate=((Top $surface ($center-2) $height $layer $zoom)-(Top $surface ($center+2) $height $layer $zoom))/4
    $rates+=$rate
    Assert-Flight ([single]::IsFinite($top) -and $rate -ge 0) 'FINITE_MONOTONE_FLIGHT'
    Assert-Flight ($top -eq (Top $surface $center $height $layer $zoom)) 'STOP_OR_REVISIT_DRIFT'
    if($null -ne $lastRates){Assert-Flight ([math]::Abs($rate-$lastRates[$layer]) -lt .06) 'ABRUPT_RESPONSE_CHANGE'}
    Assert-Flight ([apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$center,$center,$layer) -eq 1) 'HEIGHT_FADE'
    if($a -ge 1){Assert-Flight ($top -ge $height) 'SURFACE_SCENERY_REMAINS_IN_SPACE'}
   }
   Assert-Flight ($rates[2] -gt $rates[1] -and $rates[1] -gt $rates[0]) 'DEPTH_ORDER_COLLAPSED'
   $lastRates=$rates
  }
  foreach($offset in -400,0,($span*.08)) { foreach($layer in 0..2) {
   $center=$ground-$offset
   $expected=if($layer -eq 2){$height*.5-48*$zoom+$offset*.06-488}else{$height*(.57+$layer*.025)-740+($offset-$height*.05)*@(.012,.03)[$layer]}
   Assert-Flight ([math]::Abs((Top $surface $center $height $layer $zoom)-$expected) -lt .015) 'GROUND_COMPOSITION_CHANGED'
  }}
  # Curve boundary derivative and revisit checks, including the return trip.
  foreach($a in .1,1.0) {foreach($layer in 0..2){
   $center=$ground-$span*$a
   $p=Top $surface $center $height $layer $zoom
   $left=$p-(Top $surface ($center+1) $height $layer $zoom)
   $right=(Top $surface ($center-1) $height $layer $zoom)-$p
   Assert-Flight ([math]::Abs($left-$right) -lt .008) 'EASE_ENDPOINT_KINK'
   [void](Top $surface $space $height $layer $zoom)
   Assert-Flight ($p -eq (Top $surface $center $height $layer $zoom)) 'DESCENT_NOT_REVERSIBLE'
  }}
 }}
}
Write-Host "PASS: $checks actual-projection speed, depth, ground, no-fade, exit and statelessness checks. Not native comfort approval."
