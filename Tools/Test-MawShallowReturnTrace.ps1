param([string]$Path)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Verify($r){
 function Need($ok,[string]$why){if(-not $ok){throw "RETURN_TRACE: $why"}}
 Need ($r.schemaVersion -eq 4 -and $r.route -eq 'connector-return-jump-only' -and $r.world -eq 'Apogee Native Visual V3' -and $r.player -eq 'Maw QA Plain') 'scope'
 foreach($key in @('baselineStart','baselineEnd','geometryUnchanged','pass')){Need ($r.$key -is [bool]) "typed $key"}
 Need ($r.baselineStart -and $r.baselineEnd -and $r.equipment -cmatch '^(0:0,){20}$') 'plain loadout'
 Need ($r.geometryBefore -cmatch '^[A-F0-9]{64}$' -and $r.geometryBefore -ceq $r.geometryAfter -and $r.geometryUnchanged) 'changed or missing geometry'
 Need ($r.scene.Width -eq 112 -and $r.scene.Height -eq 104) 'scene size'
 Need ([Math]::Abs($r.startLocalX-944) -lt .01 -and [Math]::Abs($r.startLocalY-1062) -lt .01) 'setup position'
 $rows=@($r.samples)
 Need ($rows.Count -ge 30 -and $rows.Count -le 360 -and [Math]::Abs($r.controlled-$rows.Count) -le 1) 'missing controls'
 $right=0;$rising=0;$falling=0;$jump=0;$teeth=0;$minY=1062.0
 for($i=0;$i -lt $rows.Count;$i++){
  $s=$rows[$i];Need ($s.Tick -eq $i) 'tick sequence'
  foreach($key in @('X','Y','Vx','Vy')){Need ([double]::IsFinite($s.$key)) 'finite position/velocity'}
  foreach($key in @('Right','Left','Jump','Grounded','Teeth')){Need ($s.$key -is [bool]) "typed $key"}
  Need (-not ($s.Right -and $s.Left)) 'contradictory inputs'
  Need ($s.X -ge 864 -and $s.X -le 1440 -and $s.Y -ge 576 -and $s.Y -le 1152) 'envelope'
  Need (($s.Life -is [long] -or $s.Life -is [int]) -and $s.Life -gt 0 -and $s.Life -le 100) 'sample health'
  if($i -gt 0){Need ([Math]::Abs($s.X-$rows[$i-1].X) -le 32 -and [Math]::Abs($s.Y-$rows[$i-1].Y) -le 32) 'discontinuity'}
  if($s.Right){$right++};if($s.Vy -lt -.25){$rising++};if($s.Vy -gt .25){$falling++};if($s.Jump){$jump++};if($s.Teeth){$teeth++}
  $minY=[Math]::Min($minY,$s.Y)
 }
 Need ([Math]::Abs($rows[0].X-944) -le 4 -and [Math]::Abs($rows[0].Y-1062) -le 3) 'trace start'
 Need ($right -ge 10 -and $jump -ge 10 -and $rising -ge 5 -and $falling -eq $r.airborne -and 1062-$minY -ge 48) 'attempt not exercised'
 Need ($r.startLife -gt 0 -and $r.startLife -le 100 -and $rows[-1].Life -eq $r.endLife) 'health summary'
 if($r.pass){
  Need ($r.reason -eq 'landed' -and $r.settled -ge 12 -and [Math]::Abs($rows[-1].X+10-1312) -le 6 -and [Math]::Abs($rows[-1].Y-646) -le 2) 'wrong claimed destination'
  foreach($s in $rows[($rows.Count-12)..($rows.Count-1)]){Need ($s.Grounded -and [Math]::Abs($s.Vy) -lt .001 -and [Math]::Abs($s.Y-646) -le 2) 'unstable claimed destination'}
 }else{Need ($r.reason -eq 'tick-budget' -and $rows.Count -eq 360 -and $r.settled -lt 12) 'interrupted attempt'}
 [ordered]@{evidenceValid=$true;routeReached=$r.pass;ticks=$rows.Count;rightTicks=$right;jumpHeldTicks=$jump;risingTicks=$rising;fallingTicks=$falling;highestRisePixels=1062-$minY;toothContactTicks=$teeth;lifeBefore=$r.startLife;lifeAfter=$r.endLife;geometryUnchanged=$true;
  scope='Recorded bounded attempt only. Incomplete attempt is NOT proof of impossible traversal, forced gear, manual feel or difficulty approval.'}
}
# Synthetic parser controls only; these are not engine movement samples.
$samples=@(for($i=0;$i -lt 360;$i++){
 $phase=$i%60;$dy=if($phase -lt 20){$phase*4}elseif($phase -lt 40){(40-$phase)*4}else{0}
 [ordered]@{Tick=$i;X=944+[Math]::Min(166,$i*3);Y=1062-$dy;Vx=0;Vy=$(if($phase -lt 20){-4}elseif($phase -lt 40){4}else{0});Right=($i -lt 56);Left=$false;Jump=($phase -lt 24);Grounded=($phase -ge 40);Teeth=$false;Life=100}
})
$base=[ordered]@{schemaVersion=4;route='connector-return-jump-only';world='Apogee Native Visual V3';player='Maw QA Plain';baselineStart=$true;baselineEnd=$true;equipment=('0:0,'*20);geometryBefore=('A'*64);geometryAfter=('A'*64);geometryUnchanged=$true;pass=$false;reason='tick-budget';scene=@{Width=112;Height=104};startLocalX=944;startLocalY=1062;controlled=360;airborne=120;settled=0;startLife=100;endLife=100;samples=$samples}|ConvertTo-Json -Depth 8|ConvertFrom-Json
$null=Verify $base
$mutations=@(
 {param($r)$r.pass=$true}, {param($r)$r.geometryAfter='B'*64}, {param($r)$r.geometryUnchanged='true'},
 {param($r)$r.baselineEnd=$false}, {param($r)$r.world='aga'}, {param($r)$r.reason='context-or-tick-budget'},
 {param($r)$r.samples[20].X=9000}, {param($r)$r.samples[20].Tick=0}, {param($r)$r.startLocalY=1000},
 {param($r)foreach($s in $r.samples){$s.Jump=$false}}, {param($r)$r.samples[20].Life=500}, {param($r)$r.airborne=0},
 {param($r)$r.controlled=0}, {param($r)$r.geometryBefore=''}, {param($r)$r.samples[20].Jump='true'}
)
foreach($mutate in $mutations){$bad=$base|ConvertTo-Json -Depth 8|ConvertFrom-Json;&$mutate $bad;$caught=$false;try{$null=Verify $bad}catch{if($_.Exception.Message -notlike 'RETURN_TRACE:*'){throw};$caught=$true};if(-not $caught){throw 'Optimistic return trace accepted.'}}
Write-Output "PASS synthetic incomplete-attempt parser and $($mutations.Count) rejected controls. Not native traversal."
if($Path){Verify (Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json)|ConvertTo-Json -Depth 4}
