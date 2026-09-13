param([string[]]$Path)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Need($ok,[string]$why){if(-not $ok){throw $why}}
function Stats($values){
 $v=@($values|Sort-Object);$n=$v.Count
 if(-not $n){return @{Count=0;MeanMs=0;P50Ms=0;P95Ms=0;P99Ms=0;MaxMs=0;Over33Ms=0;Over50Ms=0}}
 return @{Count=$n;MeanMs=($v|Measure-Object -Average).Average;P50Ms=$v[[Math]::Ceiling(.5*$n)-1];P95Ms=$v[[Math]::Ceiling(.95*$n)-1];P99Ms=$v[[Math]::Ceiling(.99*$n)-1];MaxMs=$v[-1];Over33Ms=@($v|Where-Object {$_ -gt 1000/30}).Count;Over50Ms=@($v|Where-Object {$_ -gt 50}).Count}
}
function Summarize-Growth($samples){$s=@($samples);$allocated=if($s.Count){($s|Measure-Object WorkAllocatedBytes -Sum).Sum}else{0};return @{count=$s.Count;work=(Stats @($s|ForEach-Object WorkMs));validation=(Stats @($s|ForEach-Object ValidationMs));allocatedBytes=$allocated}}
function CheckStats($values,$actual){$wanted=Stats $values;foreach($k in $wanted.Keys){Need ([double]::IsFinite($actual.$k) -and [Math]::Abs($wanted[$k]-$actual.$k) -lt .000001) "Wrong summary $k"}}
function Check($r){
 Need ($r.schemaVersion -in @(1,2) -and $r.budget -in @(0,1,8,32) -and $r.player -ceq 'gg') 'Identity/budget'
 $saturated=$false
 if($r.schemaVersion -eq 2){Need ($r.layout -cin @('four-seed-perimeter','sixteen-seed-perimeter')) 'Unknown growth layout';$saturated=$r.layout -ceq 'sixteen-seed-perimeter'}
 $initial=if($saturated){16}else{4}
 Need ($r.updates -eq $r.samples.Count -and $r.updates -ge 0 -and $r.updates -le 240) 'Update count'
 Need ($r.patch.Width -eq 32 -and $r.patch.Height -eq 32 -and $r.patch.X -ge 52 -and $r.patch.Y -ge 52) 'Patch bounds'
 Need ($r.initialGrass -eq $initial -and $r.historyKnown -ge 0 -and $r.historyMissing -ge 0 -and $r.historyKnown+$r.historyMissing -eq 18) 'Seed/history coverage'
 Need ([double]::IsFinite($r.measuredElapsedMs) -and $r.measuredElapsedMs -ge 0) 'Elapsed time'
 foreach($k in @('before','historyBefore')){Need ($r.$k -cmatch '^[A-F0-9]{64}$') 'Initial fingerprint'}
 foreach($k in @('after','historyAfter')){Need ($null -eq $r.$k -or $r.$k -cmatch '^[A-F0-9]{64}$') 'Final fingerprint'}
 Need (-not $r.restored -or $r.before -ceq $r.after) 'Restoration mismatch'
 Need ($r.historyUnchanged -eq ($r.historyBefore -ceq $r.historyAfter)) 'History mismatch'
 $converted=0;$lastUpdate=$null
 for($i=0;$i -lt $r.samples.Count;$i++){
  $s=$r.samples[$i]
  Need ($s.Tick -eq $i -and $s.GameUpdate -ge 0 -and ($null -eq $lastUpdate -or $s.GameUpdate -eq $lastUpdate+1)) 'Missed/duplicate native update'
  Need ($s.Changed -ge 0 -and $s.Changed -le $r.budget -and $s.Changed -eq [int]$s.Changed) 'Per-update cell budget'
  if($saturated -and $r.budget -eq 32){$expectedChange=if($i -lt 4){32}elseif($i -eq 4){16}else{0};Need ($s.Changed -eq $expectedChange) 'Full-budget frontier sequence'}
  foreach($k in @('WorkMs','ValidationMs')){Need ([double]::IsFinite($s.$k) -and $s.$k -ge 0) 'Invalid timing sample'}
  Need ($s.WorkAllocatedBytes -ge 0 -and $s.WorkAllocatedBytes -eq [long]$s.WorkAllocatedBytes) 'Invalid allocation'
  $converted+=$s.Changed;Need ($s.Grass -eq $initial+$converted -and $s.Grass -le 160) 'Cumulative grass mismatch'
  $lastUpdate=$s.GameUpdate
 }
 Need ($r.converted -eq $converted -and $r.finalGrass -eq $initial+$converted) 'False endpoint'
 foreach($key in @('active','idle')){
  $rows=@($r.samples|Where-Object {if($key -eq 'active'){$_.Changed -gt 0}else{$_.Changed -eq 0}})
  $summary=$r.$key;Need ($summary.count -eq $rows.Count) 'Active/idle mix'
  CheckStats @($rows|ForEach-Object WorkMs) $summary.work
  CheckStats @($rows|ForEach-Object ValidationMs) $summary.validation
  $allocated=if($rows.Count){($rows|Measure-Object WorkAllocatedBytes -Sum).Sum}else{0}
  Need ($summary.allocatedBytes -eq $allocated) 'Allocation sum'
 }
 $expectedChanges=if($r.budget -eq 0){0}else{160-$initial}
 $pass=$null -eq $r.error -and $r.reason -ceq 'complete' -and $r.updates -eq 240 -and $r.restored -and $r.historyUnchanged -and $converted -eq $expectedChanges -and $r.finalGrass -eq $initial+$expectedChanges
 Need ($r.pass -eq $pass) 'Optimistic pass verdict'
 if($pass){Need ($null -eq $r.failedStep -and $r.measuredElapsedMs -le 9000) 'Hidden failed step or runaway duration'}
}
function Fixture([int]$budget,[bool]$saturated=$false){
 $initial=if($saturated){16}else{4};$remaining=if($budget -eq 0){0}else{160-$initial};$grass=$initial
 $rows=@(0..239|ForEach-Object {$change=[Math]::Min($budget,$remaining);$remaining-=$change;$grass+=$change;@{Tick=$_;GameUpdate=1000+$_;Changed=$change;Grass=$grass;WorkMs=.2;ValidationMs=.1;WorkAllocatedBytes=32}})
 return @{schemaVersion=$(if($saturated){2}else{1});layout=$(if($saturated){'sixteen-seed-perimeter'}else{'four-seed-perimeter'});budget=$budget;player='gg';updates=240;samples=$rows;patch=@{X=100;Y=100;Width=32;Height=32};initialGrass=$initial;converted=$grass-$initial;finalGrass=$grass;historyKnown=9;historyMissing=9;before='A'*64;after='A'*64;historyBefore='B'*64;historyAfter='B'*64;restored=$true;historyUnchanged=$true;measuredElapsedMs=4000;error=$null;failedStep=$null;reason='complete';pass=$true;active=(Summarize-Growth @($rows|Where-Object {$_.Changed -gt 0}));idle=(Summarize-Growth @($rows|Where-Object {$_.Changed -eq 0}))}
}
function Clone($r){$r|ConvertTo-Json -Depth 12|ConvertFrom-Json}
foreach($budget in @(0,1,8,32)){Check (Clone (Fixture $budget))}
$good=Fixture 1
$mutations=@(
 {param($r)$r.player='Maw QA Plain'}, {param($r)$r.budget=1000}, {param($r)$r.updates=239},
 {param($r)$r.samples[1].GameUpdate=1000}, {param($r)$r.samples[1].GameUpdate=1003}, {param($r)$r.samples[1].Tick=0},
 {param($r)$r.samples[1].Changed=2}, {param($r)$r.samples[1].WorkMs=-1}, {param($r)$r.samples[1].ValidationMs=-1},
 {param($r)$r.samples[1].WorkAllocatedBytes=-1}, {param($r)$r.samples[1].Grass=160},
 {param($r)$r.converted=0}, {param($r)$r.finalGrass=4}, {param($r)$r.after='C'*64}, {param($r)$r.historyAfter='C'*64},
 {param($r)$r.historyMissing=0}, {param($r)$r.active.count=0}, {param($r)$r.active.work.MeanMs=100},
 {param($r)$r.idle.validation.P99Ms=100}, {param($r)$r.active.allocatedBytes=0}, {param($r)$r.reason='stopped'},
 {param($r)$r.error='failure'}, {param($r)$r.failedStep=@{tick=42}}, {param($r)$r.measuredElapsedMs=10000},
 {param($r)$r.pass=$false}, {param($r)$r.initialGrass=5}, {param($r)$r.patch.Width=1000}
)
foreach($mutate in $mutations){$r=Clone $good;& $mutate $r;$rejected=$false;try{Check $r}catch{$rejected=$true};Need $rejected 'Corrupt growth report survived'}
$partial=Clone $good;$partial.samples=@($partial.samples[0..4]);$partial.updates=5;$partial.converted=5;$partial.finalGrass=9;$partial.reason='new-command';$partial.pass=$false;$partial.active=Summarize-Growth $partial.samples;$partial.idle=Summarize-Growth @();Check $partial
Write-Output "PASS four synthetic budget/idle cases, $($mutations.Count) rejected reports and retained partial evidence. Not native performance proof."
foreach($budget in @(0,1,8,32)){Check (Clone (Fixture $budget $true));$r=Clone (Fixture $budget);$r.schemaVersion=2;Check $r}
$wide=Fixture 32 $true
$layoutMutations=@(
 {param($r)$r.layout='unknown'}, {param($r)$r.layout='four-seed-perimeter'},
 {param($r)$r.initialGrass=4}, {param($r)$r.schemaVersion=1},
 {param($r)$r.samples[0].Changed=8}, {param($r)$r.samples[4].Changed=32}
)
foreach($mutate in $layoutMutations){$r=Clone $wide;& $mutate $r;$rejected=$false;try{Check $r}catch{$rejected=$true};Need $rejected 'Saturated growth defect survived'}
$r=Clone $wide;$r.samples=@($r.samples[0..2]);$r.updates=3;$r.converted=96;$r.finalGrass=112;$r.reason='new-command';$r.pass=$false;$r.active=Summarize-Growth $r.samples;$r.idle=Summarize-Growth @();Check $r
Write-Output 'PASS eight schema2 layout/budget cases, six layout/frontier rejection controls and retained saturated partial.'
foreach($file in $Path){$r=Get-Content -Raw -LiteralPath $file|ConvertFrom-Json;Check $r;Write-Output "REPLAY $file : budget=$($r.budget); pass=$($r.pass); updates=$($r.updates); converted=$($r.converted); active max=$($r.active.work.MaxMs)ms; idle max=$($r.idle.work.MaxMs)ms; restored=$($r.restored). Bounded local work only."}
