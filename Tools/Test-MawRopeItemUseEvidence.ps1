param([string[]]$Path)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Need($ok,[string]$why){if(-not $ok){throw $why}}
function Check($r){
 Need ($r.schemaVersion -eq 1 -and $r.player -ceq 'gg' -and $r.variant -in @('vanilla','rib','cap','wall','none','diagonal','far')) 'Identity'
 Need ($r.count -eq $r.samples.Count -and $r.count -ge 0 -and $r.count -le 60 -and $r.controlled -ge $r.count -and $r.controlled -le 60) 'Count'
 Need ($r.slot -ge 0 -and $r.slot -le 9 -and $r.startStack -ge 2 -and $r.endStack -ge 0) 'Inventory bounds'
 Need ($r.patch.Width -eq 32 -and $r.patch.Height -eq 32 -and $r.patch.X -ge 52 -and $r.patch.Y -ge 52) 'Ownership bounds'
 Need ($r.historyKnown -ge 0 -and $r.historyMissing -ge 0 -and $r.historyKnown+$r.historyMissing -eq 18) 'History coverage'
 foreach($k in @('before','historyBefore')){Need ($r.$k -cmatch '^[A-F0-9]{64}$') 'Initial hash'}
 foreach($k in @('after','historyAfter')){Need ($null -eq $r.$k -or $r.$k -cmatch '^[A-F0-9]{64}$') 'Final hash'}
 Need (-not $r.restored -or $r.before -ceq $r.after) 'Restoration mismatch'
 Need ([double]::IsFinite($r.elapsedMs) -and $r.elapsedMs -ge 0) 'Elapsed'
 $expected=$r.variant -in @('vanilla','rib','cap','wall');Need ($r.expected -eq $expected) 'Expected result changed'
 $targetX=if($r.variant -eq 'far'){28}else{11};$traceOk=$r.count -eq 60
 for($i=0;$i -lt $r.count;$i++){
  $s=$r.samples[$i]
  foreach($key in @('X','Y')){Need ([double]::IsFinite($s.$key)) 'Nonfinite body'}
  Need ($s.Stack -ge 0 -and $s.Animation -ge 0 -and $s.ItemTime -ge 0 -and $s.Life -ge 0) 'Invalid native value'
  $aimX=if($i -eq 1){$targetX}else{2};$aimY=if($i -eq 1){23}else{2}
  $rowOk=$s.Tick -eq $i -and ($i -eq 0 -or $s.Update -eq $r.samples[$i-1].Update+1) -and $s.Use -eq ($i -eq 1) -and
    $s.PreCalls -eq $i+1 -and $s.PostCalls -eq $i+1 -and $s.AimX -eq $aimX -and $s.AimY -eq $aimY -and
    $s.NativeX -eq $s.AimX -and $s.NativeY -eq $s.AimY -and $s.NativeItem -eq 965
  $traceOk=$traceOk -and $rowOk
 }
 Need ($r.traceOk -eq $traceOk) 'Optimistic trace verdict'
 $pass=$null -eq $r.error -and $r.reason -ceq 'complete' -and $r.count -eq 60 -and $r.controlled -eq 60 -and
   $traceOk -and $r.samples[1].Animation -gt 0 -and $r.preCalls -eq 60 -and $r.postCalls -eq 60 -and
   $r.endAnimation -eq 0 -and $r.endItemTime -eq 0 -and $r.targetRestored -and $r.inventoryOk -and
   $r.endLife -ge $r.startLife -and $r.collateral -eq 0 -and $r.restored -and $r.before -ceq $r.after -and
   $r.historyBefore -ceq $r.historyAfter -and $r.targetRope -eq $expected -and $r.ropeCells -eq [int]$expected -and
   $r.startStack-$r.endStack -eq [int]$expected
 Need ($r.pass -eq $pass) 'Optimistic completion verdict'
 if($pass){
  Need ($r.selectedRestored -and $r.elapsedMs -le 6500) 'Selection or duration'
  for($i=0;$i -lt $r.count;$i++){
   $s=$r.samples[$i];$placed=$expected -and $i -ge 1
   Need ($s.Stack -eq $r.startStack-[int]$placed -and $s.TargetPresent -eq $placed -and (-not $placed -or $s.TargetType -eq 213)) 'Native placement/consumption sequence'
   Need ($s.Life -ge $r.startLife -and [Math]::Abs($s.X-126) -le 4 -and [Math]::Abs($s.Y-374) -le 4) 'Damage or motion'
  }
  Need ($r.samples[-1].Stack -eq $r.endStack -and $r.samples[-1].Animation -eq $r.endAnimation -and $r.samples[-1].ItemTime -eq $r.endItemTime) 'Endpoint contradicts trace'
  $p=$r.samples[1];$rx=$p.RangeX+$p.TileBoost+$p.BlockRange;$ry=$p.RangeY+$p.TileBoost+$p.BlockRange
  $within=$targetX -ge $p.X/16-$rx -and $targetX -le ($p.X+20)/16+$rx-1 -and 23 -ge $p.Y/16-$ry -and 23 -le ($p.Y+42)/16+$ry-2
  Need ($within -eq ($r.variant -ne 'far')) 'Wrong native reach control'
 }
}
function Fixture([string]$variant){
 $ok=$variant -in @('vanilla','rib','cap','wall');$x=if($variant -eq 'far'){28}else{11}
 $rows=@(0..59|ForEach-Object {$placed=$ok -and $_ -ge 1;$aimX=if($_ -eq 1){$x}else{2};$aimY=if($_ -eq 1){23}else{2};
  @{Tick=$_;Update=1000+$_;Use=$_ -eq 1;PreCalls=$_+1;PostCalls=$_+1;AimX=$aimX;AimY=$aimY;NativeX=$aimX;NativeY=$aimY;NativeItem=965;
    TargetPresent=$placed;TargetType=$(if($placed){213}else{0});Stack=20-[int]$placed;Animation=$(if($_ -ge 1){[Math]::Max(0,15-$_)}else{0});
    ItemTime=$(if($placed){[Math]::Max(0,9-$_)}else{0});X=126;Y=374;Life=500;RangeX=5;RangeY=4;TileBoost=3;BlockRange=0}})
 return @{schemaVersion=1;player='gg';variant=$variant;expected=$ok;count=60;controlled=60;preCalls=60;postCalls=60;samples=$rows;traceOk=$true;
  slot=6;startStack=20;endStack=20-[int]$ok;patch=@{X=100;Y=100;Width=32;Height=32};historyKnown=9;historyMissing=9;
  before='A'*64;after='A'*64;historyBefore='B'*64;historyAfter='B'*64;restored=$true;elapsedMs=1000;error=$null;reason='complete';
  pass=$true;targetRope=$ok;ropeCells=[int]$ok;collateral=0;inventoryOk=$true;startLife=500;endLife=500;endAnimation=0;endItemTime=0;targetRestored=$true;selectedRestored=$true}
}
function Clone($r){$r|ConvertTo-Json -Depth 12|ConvertFrom-Json}
foreach($v in @('vanilla','rib','cap','wall','none','diagonal','far')){Check (Clone (Fixture $v))}
$good=Fixture 'rib'
$mutations=@(
 {param($r)$r.player='GOLB'}, {param($r)$r.variant='fake'}, {param($r)$r.count=59}, {param($r)$r.slot=40}, {param($r)$r.patch.Width=100},
 {param($r)$r.historyMissing=0}, {param($r)$r.after='C'*64}, {param($r)$r.expected=$false},
 {param($r)$r.samples[1].Use=$false}, {param($r)$r.samples[2].Use=$true}, {param($r)$r.samples[1].NativeItem=1},
 {param($r)$r.samples[1].PostCalls=0}, {param($r)$r.samples[1].NativeX=100}, {param($r)$r.samples[2].Update=1001},
 {param($r)$r.samples[1].Stack=20}, {param($r)$r.samples[1].TargetPresent=$false}, {param($r)$r.samples[1].TargetType=1},
 {param($r)$r.samples[1].Animation=0}, {param($r)$r.samples[1].Life=1}, {param($r)$r.samples[1].X=1000},
 {param($r)$r.endStack=20}, {param($r)$r.endItemTime=1}, {param($r)$r.targetRestored=$false}, {param($r)$r.selectedRestored=$false},
 {param($r)$r.error='failure'}, {param($r)$r.reason='stopped'}, {param($r)$r.collateral=1}, {param($r)$r.historyAfter='C'*64},
 {param($r)$r.inventoryOk=$false}, {param($r)$r.traceOk=$false}, {param($r)$r.pass=$false}, {param($r)$r.samples[-1].Animation=1}
)
foreach($m in $mutations){$r=Clone $good;& $m $r;$caught=$false;try{Check $r}catch{$caught=$true};Need $caught 'Corrupt report survived'}
$partial=Clone $good;$partial.samples=@($partial.samples[0..4]);$partial.count=$partial.controlled=$partial.preCalls=$partial.postCalls=5;$partial.reason='new-command';$partial.pass=$partial.traceOk=$false;$partial.endAnimation=10;Check $partial
Write-Output "PASS seven synthetic variants/$($mutations.Count) corrupted reports/retained partial. Native item-use still separate."
foreach($file in $Path){$r=Get-Content -Raw -LiteralPath $file|ConvertFrom-Json;Check $r;Write-Output "REPLAY $file : $($r.variant), pass=$($r.pass), updates=$($r.count), consumed=$($r.startStack-$r.endStack), restored=$($r.restored). Scripted item-use only."}
