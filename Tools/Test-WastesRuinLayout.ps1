param([ValidateSet('None','MissingGroup','PhaseJump','WrongAsset','OldDensity')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$code=@('WastesParallaxContract','WastesRuinLayout')|ForEach-Object{(Get-Content -Raw (Join-Path $root "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;',''}
if($Mutation -eq 'OldDensity') {
 $code=$code | ForEach-Object { $_.Replace('Period = 10000','Period = 6800').Replace('* 2000','* 1360').Replace(': 1150',': 760') }
}
Add-Type -TypeDefinition ("using System;`n"+($code -join "`n"))
$checks=0
# The requested trial is five landmarks per10,000 native display pixels,
# instead of per6,800:32% lower density, without resizing any artwork.
if([apogean.Common.Backgrounds.WastesRuinLayout]::Period -ne 10000) {throw 'LANDMARK_DENSITY_NOT_REDUCED'}
foreach($viewport in @(1920,2560)) {
 foreach($worldX in @(-150000,-1,-.1,0,.1,1,150000)+(0..80|ForEach-Object{$_*10000/.14/80})) {
  $phase=[apogean.Common.Backgrounds.WastesRuinLayout]::Phase($worldX)
  if($Mutation -eq 'PhaseJump'){$phase+=20}
  $expectedPhase=(($worldX*.14)%10000+10000)%10000
  if([Math]::Abs($phase-$expectedPhase) -gt .001){throw 'PHASE_JUMP'}
  $actual=[Collections.Generic.List[string]]::new()
  $start=-$phase-10000
  for(;$start -lt $viewport;$start+=10000){
   for($g=0;$g -lt [apogean.Common.Backgrounds.WastesRuinLayout]::Count;$g++){
    if($Mutation -eq 'MissingGroup' -and $g -eq 4){continue}
    $x=$start+[apogean.Common.Backgrounds.WastesRuinLayout]::Offset($g)
    $width=[apogean.Common.Backgrounds.WastesRuinLayout]::Width($g)
    $asset=[apogean.Common.Backgrounds.WastesRuinLayout]::Asset($g)
    if($Mutation -eq 'WrongAsset'){$asset=2}
    if($x -lt $viewport -and $x+$width -gt 0){$actual.Add(('{0:F1}/{1}/{2}' -f ([double]$x),$width,$asset))}
   }
  }
  $expected=[Collections.Generic.List[string]]::new()
  $origin=$worldX*.14
  for($c=[Math]::Floor($origin/10000)-1;$c -le [Math]::Floor(($origin+$viewport)/10000)+1;$c++){
   for($g=0;$g -lt 10;$g++){
    $x=$c*10000-$origin+[Math]::Floor($g/2)*2000+($(if($g%2){1150}else{0}))
    $width=if($g%2){391}elseif($g -eq 0){576}else{512}
    $asset=if($g%2){1}elseif($g -eq 0){0}else{[int]($g/2)+1}
    if($x -lt $viewport -and $x+$width -gt 0){$expected.Add(('{0:F1}/{1}/{2}' -f ([double]$x),$width,$asset))}
   }
  }
  # Normalize signed zero only; double-vs-float phase error is separately bounded.
  if(($actual -join '|').Replace('-0.0/','0.0/') -ne ($expected -join '|').Replace('-0.0/','0.0/')){throw "VISIBLE_BANK_MISMATCH: worldX=$worldX; actual=$($actual -join '|'); expected=$($expected -join '|')"}
  $checks++
 }
}
foreach($g in 0..9){
 $right=[apogean.Common.Backgrounds.WastesRuinLayout]::Offset($g)+[apogean.Common.Backgrounds.WastesRuinLayout]::Width($g)
 $next=if($g -eq 9){10000}else{[apogean.Common.Backgrounds.WastesRuinLayout]::Offset($g+1)}
 if($next-$right -lt 459){throw 'QUIET_INTERVAL_MISSING'}
}
Write-Output "PASS: $checks independent bank enumeration cases; 32% fewer landmarks, >=459px open intervals, unchanged asset sizes/order and no movement randomization. Not a render proof."
