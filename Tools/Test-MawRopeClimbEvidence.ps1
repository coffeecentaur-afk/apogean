param([string[]]$Path)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Need($ok,[string]$why){if(-not $ok){throw $why}}
function Check($r){
 Need ($r.schemaVersion -eq 1 -and $r.variant -cin @('vanilla','rib','none') -and $r.player -ceq 'Maw QA Rope') 'Unknown climb identity'
 Need ($r.samples.Count -le 210 -and $r.controlled -ge $r.samples.Count -and $r.controlled -le 210) 'Sample/control budget'
 Need ($r.patch.Width -eq 32 -and $r.patch.Height -eq 32 -and $r.patch.X -ge 52 -and $r.patch.Y -ge 52) 'Scratch bounds'
 Need ($r.historyKnown -ge 0 -and $r.historyMissing -ge 0 -and $r.historyKnown+$r.historyMissing -eq 18) 'Missing history accounting'
 Need ([double]::IsFinite($r.elapsedMs) -and $r.elapsedMs -ge 0) 'Elapsed time'
 foreach($field in @('geometryBefore','emptyBefore','historyBefore')){Need ($r.$field -cmatch '^[0-9A-F]{64}$') 'Invalid initial hash'}
 foreach($field in @('geometryAfter','emptyAfter','historyAfter')){Need ($null -eq $r.$field -or $r.$field -cmatch '^[0-9A-F]{64}$') 'Invalid final hash'}
 Need ($r.unchanged -eq ($r.geometryBefore -ceq $r.geometryAfter)) 'Geometry verdict disagrees with hash'
 # Exact cell comparison can reject even when a hash matches; true requires both hashes.
 Need (-not $r.restored -or $r.emptyBefore -ceq $r.emptyAfter) 'Restoration verdict disagrees with hash'
 Need ($r.historyUnchanged -eq ($r.historyBefore -ceq $r.historyAfter)) 'History verdict disagrees with hash'
 $pulley=0;$upward=0;$minimum=374.0;$previousX=254.0;$previousY=374.0;$motionValid=$true;$inputsValid=$true;$lifeValid=$true
 for($i=0;$i -lt $r.samples.Count;$i++){
  $s=$r.samples[$i];Need ($s.Tick -eq $i) 'Skipped/duplicated tick'
  foreach($f in @('X','Y','Vx','Vy')){Need ([double]::IsFinite($s.$f)) 'Non-finite movement'}
  $motionValid=$motionValid -and [Math]::Abs($s.X-$previousX) -le 16 -and [Math]::Abs($s.Y-$previousY) -le 12 -and $s.X -ge 176 -and $s.X -le 336 -and $s.Y -ge 32 -and $s.Y -le 432
  $inputsValid=$inputsValid -and $s.Up -eq ($i -lt 180) -and -not $s.Down -and -not $s.Jump
  $lifeValid=$lifeValid -and $s.Life -ge $r.startLife
  if($s.Pulley){$pulley++;if($s.Vy -lt -.1){$upward++}}
  $minimum=[Math]::Min($minimum,$s.Y);$previousX=$s.X;$previousY=$s.Y
 }
 # Start-relative rise agrees with the runtime even for an all-downward failed trace.
 $rise=if($r.samples.Count){374.0-($r.samples.Y|Measure-Object -Minimum).Minimum}else{0.0}
 Need ([Math]::Abs($r.rise-$rise) -lt .01 -and $r.pulleyTicks -eq $pulley -and $r.upwardPulleyTicks -eq $upward) 'Motion totals disagree with raw trace'
 $attached=$pulley -gt 0;$ascended=$pulley -ge 30 -and $upward -ge 30 -and $rise -ge 128
 Need ($r.attached -eq $attached -and $r.ascended -eq $ascended) 'Attachment/ascent verdict'
 $complete=$null -eq $r.error -and $r.reason -ceq 'complete' -and $r.samples.Count -eq 210 -and $r.controlled -eq 210 -and $r.baseline -and $r.endLife -ge $r.startLife -and $r.unchanged -and $r.restored -and $r.historyUnchanged
 Need ($r.complete -eq $complete) 'Optimistic completion'
 $pass=$complete -and $(if($r.variant -ceq 'none'){-not $attached -and -not $ascended -and [Math]::Abs($rise) -lt 1}else{$ascended})
 Need ($r.comparisonPass -eq $pass) 'Optimistic comparison'
 if($complete){Need ($r.elapsedMs -le 9000 -and $motionValid -and $inputsValid -and $lifeValid) 'Completed trace violates bounded native movement/input/life contract'}
}
function Clone($r){$r|ConvertTo-Json -Depth 10|ConvertFrom-Json}
# Synthetic schema fixtures exercise the checker only, never stand in for native traces.
$rows=@(0..209|ForEach-Object {$y=374-[Math]::Min($_+1,160);@{Tick=$_;X=254;Y=$y;Vx=0;Vy=$(if($_ -lt 160){-1}else{0});Up=($_ -lt 180);Down=$false;Jump=$false;Pulley=$true;PulleyDir=1;RopeCount=0;Life=100}})
$good=@{schemaVersion=1;variant='rib';player='Maw QA Rope';samples=$rows;controlled=210;patch=@{X=100;Y=100;Width=32;Height=32};historyKnown=9;historyMissing=9;elapsedMs=3500;geometryBefore='A'*64;geometryAfter='A'*64;emptyBefore='B'*64;emptyAfter='B'*64;historyBefore='C'*64;historyAfter='C'*64;unchanged=$true;restored=$true;historyUnchanged=$true;rise=160;pulleyTicks=210;upwardPulleyTicks=160;attached=$true;ascended=$true;baseline=$true;startLife=100;endLife=100;error=$null;reason='complete';complete=$true;comparisonPass=$true}
Check (Clone $good)
$none=Clone $good;$none.variant='none';$none.rise=0;$none.pulleyTicks=0;$none.upwardPulleyTicks=0;$none.attached=$false;$none.ascended=$false
foreach($s in $none.samples){$s.Y=374;$s.Vy=0;$s.Pulley=$false};Check $none
$mutations=@(
 {param($r)$r.player='gg'}, {param($r)$r.variant='fake'}, {param($r)$r.samples[3].Tick=2},
 {param($r)$r.samples[0].X=999}, {param($r)$r.samples[100].Y=300}, {param($r)$r.samples[0].Jump=$true},
 {param($r)$r.samples[0].Down=$true}, {param($r)$r.samples[180].Up=$true}, {param($r)$r.samples[30].Life=99},
 {param($r)$r.rise=200}, {param($r)$r.pulleyTicks=0}, {param($r)$r.upwardPulleyTicks=0},
 {param($r)$r.attached=$false}, {param($r)$r.ascended=$false}, {param($r)$r.controlled=209},
 {param($r)$r.geometryAfter='D'*64}, {param($r)$r.emptyAfter='D'*64}, {param($r)$r.historyAfter='D'*64},
 {param($r)$r.historyMissing=0}, {param($r)$r.baseline=$false}, {param($r)$r.reason='cancelled'},
 {param($r)$r.samples=@($r.samples[0..199])}, {param($r)$r.elapsedMs=10000}, {param($r)$r.comparisonPass=$false}
)
foreach($mutate in $mutations){$r=Clone $good;& $mutate $r;$rejected=$false;try{Check $r}catch{$rejected=$true};Need $rejected 'Climb-report mutation survived'}
$partial=Clone $good;$partial.samples=@($partial.samples[0..9]);$partial.controlled=10;$partial.rise=10;$partial.pulleyTicks=10;$partial.upwardPulleyTicks=10;$partial.ascended=$false;$partial.reason='ui-context-pause-timeout';$partial.complete=$false;$partial.comparisonPass=$false;Check $partial
Write-Output "PASS $($mutations.Count) corrupt-climb-report rejection controls, matched no-rope and retained partial controls. Synthetic checker tests only."
foreach($file in $Path){$r=Get-Content -Raw -LiteralPath $file|ConvertFrom-Json;Check $r;Write-Output "REPLAY $file : variant=$($r.variant); complete=$($r.complete); comparisonPass=$($r.comparisonPass); rise=$($r.rise); pulley=$($r.pulleyTicks); restored=$($r.restored). Preplaced-rope ascent only."}
