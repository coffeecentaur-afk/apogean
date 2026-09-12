param([string]$Directory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Require($Condition,[string]$Message) {if(-not $Condition){throw $Message}}
function Check-Stats($Samples,$Summary) {
 Require ($Samples.Count -eq $Summary.Count) 'Sample count mismatch'
 foreach($v in $Samples){Require ([double]::IsFinite($v) -and $v -ge 0) 'Invalid timing'}
 $expected=@{MeanMs=0;MaxMs=0;P50Ms=0;P95Ms=0;P99Ms=0;Over33Ms=0;Over50Ms=0}
 if($Samples.Count) {
  $sorted=@($Samples|Sort-Object)
  $expected=@{MeanMs=($Samples|Measure-Object -Average).Average;MaxMs=$sorted[-1];P50Ms=$sorted[[Math]::Ceiling($sorted.Count*.5)-1];P95Ms=$sorted[[Math]::Ceiling($sorted.Count*.95)-1];P99Ms=$sorted[[Math]::Ceiling($sorted.Count*.99)-1];Over33Ms=@($Samples|Where-Object {$_ -gt (1000.0/30)}).Count;Over50Ms=@($Samples|Where-Object {$_ -gt 50}).Count}
 }
 foreach($key in $expected.Keys){Require ([double]::IsFinite($Summary.$key) -and [Math]::Abs($Summary.$key-$expected[$key]) -lt .000001) "Incorrect statistic $key"}
}
function Check-Record($r) {
 Require ($r.schemaVersion -eq 1 -and $r.side -in @(16,32) -and $r.cycles -eq 3) 'Contract mismatch'
 Require ($r.before -match '^[0-9A-F]{64}$' -and $r.after -match '^[0-9A-F]{64}$') 'Invalid fingerprints'
 Require ([double]::IsFinite($r.totalMs) -and $r.totalMs -ge 0) 'Invalid duration'
 Require ($r.phases.Count -le 9 -and $r.negativeControls -in @(0,1)) 'Invalid phase/control count'
 $cells=0; $timedMs=0.0; $allValidated=$r.phases.Count -eq 9
 for($i=0;$i -lt $r.phases.Count;$i++) {
  $p=$r.phases[$i]
  Require ($p.cycle -eq [Math]::Floor($i/3) -and $p.phase -eq ($i%3+1)) 'Phase order mismatch'
  Require ($p.budgetCells -eq 32 -and $p.cells -eq $p.conversionBatchMs.Count*32 -and $p.cells -le $r.side*$r.side) 'Invalid bounded batch accounting'
  Require ($p.threadAllocatedBytes -ge 0) 'Negative allocation'
  Check-Stats $p.conversionBatchMs $p.statistics
  if($p.validated){Require ($p.cells -eq $r.side*$r.side) 'Incomplete phase marked validated'}
  else {Require ($i -eq $r.phases.Count-1) 'Work continued after failed phase';$allValidated=$false}
  $cells+=$p.cells
  $timedMs+=($p.conversionBatchMs|Measure-Object -Sum).Sum
 }
 Require ($cells -eq $r.completedCells) 'Completed cells mismatch'
 Require ($timedMs -le $r.totalMs+.001) 'Timed batches exceed total duration'
 $pass=$null -eq $r.error -and $r.restored -and $r.historyUnchanged -and $r.before -eq $r.after -and $cells -eq $r.side*$r.side*9
 Require ($pass -eq $r.pass) 'Pass verdict mismatch'
 if($r.pass){Require ($allValidated -and $r.negativeControls -eq 1 -and $r.checks -eq 20*$r.side*$r.side+3) 'Successful report lacks complete validation'}
}
# An independent synthetic report, not native evidence. Invalid reports must fail;
# accurately reported partial failures must remain readable and must not be erased.
$phases=@(0..8|ForEach-Object { @{cycle=[Math]::Floor($_/3);phase=($_%3+1);cells=256;budgetCells=32;validated=$true;conversionBatchMs=@(1.0)*8;statistics=@{Count=8;MeanMs=1;MaxMs=1;P50Ms=1;P95Ms=1;P99Ms=1;Over33Ms=0;Over50Ms=0};threadAllocatedBytes=123} })
$good=@{schemaVersion=1;side=16;cycles=3;before='A'*64;after='A'*64;totalMs=100;phases=$phases;negativeControls=1;completedCells=2304;error=$null;restored=$true;historyUnchanged=$true;pass=$true;checks=5123}
Check-Record ($good|ConvertTo-Json -Depth 8|ConvertFrom-Json)
$defects=@(
 {param($r)$r.phases[0].conversionBatchMs[0]=50},
 {param($r)$r.phases[0].statistics.P99Ms=0},
 {param($r)$r.phases[0].cells=224},
 {param($r)$r.phases[0].budgetCells=1024},
 {param($r)$r.phases[0].validated=$false},
 {param($r)$r.phases[1].cycle=2},
 {param($r)$r.phases[8].validated=$false},
 {param($r)$r.completedCells=0},
 {param($r)$r.negativeControls=0},
 {param($r)$r.restored=$false},
 {param($r)$r.historyUnchanged=$false},
 {param($r)$r.after='B'*64},
 {param($r)$r.error='Failure'},
 {param($r)$r.checks=1},
 {param($r)$r.totalMs=0},
 {param($r)$r.side=64}
)
foreach($mutate in $defects){$bad=$good|ConvertTo-Json -Depth 8|ConvertFrom-Json;& $mutate $bad;$caught=$false;try{Check-Record $bad}catch{$caught=$true};Require $caught 'Optimistic mutation accepted'}
$partial=$good|ConvertTo-Json -Depth 8|ConvertFrom-Json
$partial.phases=@($partial.phases[0]);$partial.phases[0].validated=$false;$partial.completedCells=256;$partial.error='Property mismatch';$partial.pass=$false;$partial.checks=513
Check-Record $partial
Write-Output "PASS conversion report replay: $($defects.Count) rejection controls and retained partial-failure control. Does not independently prove native world state."
if($Directory) {
 $files=@(Get-ChildItem -LiteralPath $Directory -Filter 'conversion-*.json'|Sort-Object Name)
 Require ($files.Count -gt 0) 'No conversion reports'
 foreach($file in $files){$r=Get-Content -Raw -LiteralPath $file.FullName|ConvertFrom-Json;Check-Record $r;Write-Output "REPLAY $($file.Name): side=$($r.side); pass=$($r.pass); cells=$($r.completedCells); totalMs=$($r.totalMs). Native throughput only, not per-frame performance."}
}
