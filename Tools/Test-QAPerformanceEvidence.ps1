param([string]$Directory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Require($Condition,[string]$Message) { if(-not $Condition){throw $Message} }
function Check-Stats($Samples,$Summary) {
 Require ($Samples.Count -eq $Summary.Count) 'Sample count mismatch'
 if(-not $Samples.Count){return}
 foreach($value in $Samples){Require ([double]::IsFinite($value) -and $value -ge 0) 'Invalid timing sample'}
 $ordered=@($Samples | Sort-Object)
 $expected=@{MeanMs=($Samples|Measure-Object -Average).Average;MaxMs=$ordered[-1];P50Ms=$ordered[[Math]::Ceiling($ordered.Count*.50)-1];P95Ms=$ordered[[Math]::Ceiling($ordered.Count*.95)-1];P99Ms=$ordered[[Math]::Ceiling($ordered.Count*.99)-1];Over33Ms=@($Samples|Where-Object {$_ -gt (1000.0/30)}).Count;Over50Ms=@($Samples|Where-Object {$_ -gt 50}).Count}
 foreach($key in $expected.Keys){Require ([Math]::Abs($Summary.$key-$expected[$key]) -lt .000001) "Incorrect statistic $key"}
}
function Check-Record($Report) {
 Require ($Report.schemaVersion -eq 2) 'Expected schema2'
 Check-Stats $Report.updateSamplesMs $Report.worldUpdateSlice
 Check-Stats $Report.drawIntervalsMs $Report.worldDrawCallbackIntervals
 $shallow=$Report.scenario -in @('shallow-static','shallow-sweep')
 $geometry= -not $shallow -or ($null -ne $Report.fixtureBefore -and $Report.fixtureBefore -eq $Report.fixtureAfter)
 $coverage=$true
 if($shallow) {
  $coverage=$Report.cameraCallbacks -gt 0 -and $null -ne $Report.cameraRange
  if($coverage) {
   $dx=$Report.cameraRange.maxX-$Report.cameraRange.minX; $dy=$Report.cameraRange.maxY-$Report.cameraRange.minY
   $coverage=if($Report.scenario -eq 'shallow-static'){[Math]::Sqrt($dx*$dx+$dy*$dy) -lt 1}else{$dx -ge 304 -and $dy -ge 608}
  }
  Require ($Report.synthetic.neutralLights -eq 64) 'Unexpected workload lighting'
 }
 Require ($geometry -eq $Report.geometryUnchanged) 'Geometry verdict does not match hashes'
 Require ($coverage -eq $Report.cameraCoverage) 'Camera coverage verdict mismatch'
 $usable=$Report.reason -eq 'duration-complete' -and $Report.worldUpdateSlice.Count -ge 300 -and $Report.worldDrawCallbackIntervals.Count -ge 300 -and $Report.overflow -eq 0 -and $Report.pausedCallbacks -eq 0 -and $Report.inactiveCallbacks -eq 0 -and $Report.captureCallbacks -eq 0 -and $geometry -and $coverage
 Require ($usable -eq $Report.usable) 'Usable verdict mismatch'
}
# Independently computed fixtures; parser must reject optimistic summaries/verdicts.
$samples=@(1..300 | ForEach-Object {16.0})
$stats=@{Count=300;MeanMs=16;P50Ms=16;P95Ms=16;P99Ms=16;MaxMs=16;Over33Ms=0;Over50Ms=0}
$good=@{schemaVersion=2;scenario='shallow-sweep';reason='duration-complete';usable=$true;updateSamplesMs=$samples;drawIntervalsMs=$samples;worldUpdateSlice=$stats;worldDrawCallbackIntervals=$stats;fixtureBefore='a';fixtureAfter='a';geometryUnchanged=$true;cameraCoverage=$true;cameraCallbacks=301;cameraRange=@{minX=0;minY=0;maxX=320;maxY=640};synthetic=@{neutralLights=64};overflow=0;pausedCallbacks=0;inactiveCallbacks=0;captureCallbacks=0}
Check-Record ($good|ConvertTo-Json -Depth 6|ConvertFrom-Json)
$defects=@(
 {param($r) $r.drawIntervalsMs[0]=200},
 {param($r) $r.worldUpdateSlice.P99Ms=0},
 {param($r) $r.fixtureAfter='changed'},
 {param($r) $r.cameraRange.maxY=20},
 {param($r) $r.reason='manual-stop'},
 {param($r) $r.inactiveCallbacks=1},
 {param($r) $r.captureCallbacks=1},
 {param($r) $r.pausedCallbacks=1},
 {param($r) $r.overflow=1},
 {param($r) $r.scenario='shallow-static'},
 {param($r) $r.synthetic.neutralLights=0}
)
foreach($mutate in $defects) {
 $bad=$good|ConvertTo-Json -Depth 6|ConvertFrom-Json; & $mutate $bad
 $caught=$false;try{Check-Record $bad}catch{$caught=$true}
 Require $caught 'Mutation wrongly accepted'
}
Write-Output "PASS independent timing replay and $($defects.Count) optimistic-report rejection controls. Not a performance acceptance threshold."
if($Directory) {
 $files=@(Get-ChildItem -LiteralPath $Directory -Filter 'timing-*.json'|Sort-Object Name)
 Require ($files.Count -gt 0) 'No timing evidence'
 foreach($file in $files) {
  $report=Get-Content -Raw -LiteralPath $file.FullName|ConvertFrom-Json
  Check-Record $report
  Write-Output "REPLAY $($file.Name): $($report.scenario); usable=$($report.usable); reason=$($report.reason); NPCs=$($report.before.scope.actors.npcs)->$($report.after.scope.actors.npcs). Raw statistics consistent; not gameplay/low-end approval."
 }
}
