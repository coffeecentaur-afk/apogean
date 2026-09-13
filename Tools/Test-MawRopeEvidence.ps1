param([string[]]$Path,[switch]$RequireAnatomyItems)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Require($ok,[string]$message){if(-not $ok){throw $message}}
function Check-Record($r,[bool]$needBinding=$false){
 Require ($r.schemaVersion -in @(1,2) -and $r.cases.Count -le 15) 'Unknown rope contract'
 Require ($r.before -cmatch '^[0-9A-F]{64}$' -and $r.after -cmatch '^[0-9A-F]{64}$') 'Invalid fingerprint'
 Require ($r.historyKnown -ge 0 -and $r.historyMissing -ge 0 -and $r.historyKnown+$r.historyMissing -eq 18) 'History accounting'
 Require ($r.patch.Width -eq 32 -and $r.patch.Height -eq 32 -and $r.patch.X -ge 52 -and $r.patch.Y -ge 52) 'Invalid scratch bounds'
 Require ([double]::IsFinite($r.elapsedMs) -and $r.elapsedMs -ge 0 -and $r.checks -ge 0 -and $r.negativeControls -in @(0,1,2)) 'Invalid accounting'
 $hosts=@('vanilla-gray-brick','candidate-rib','candidate-fiber-cap');$anchors=@('above','left','right','below')
 for($i=0;$i -lt $r.cases.Count;$i++){
  $c=$r.cases[$i]
  if($i -lt 12){
   Require ($c.kind -eq 'rope-api' -and $c.host -eq $hosts[[Math]::Floor($i/4)] -and $c.anchor -eq $anchors[$i%4]) 'Case order/identity'
   Require ($c.placed -eq 16 -and $c.afterMining -eq 15 -and $c.ropeDrops -eq 1) 'Rope segment accounting'
   Require ($c.drops.Count -eq 1 -and $c.drops[0].type -eq 965 -and $c.drops[0].stack -eq 1) 'Actual rope drop missing'
   Require ($c.frames.Count -eq 16) 'Missing native frames'
   foreach($f in $c.frames){Require ($f.x -ge 0 -and $f.y -ge 0 -and $f.x%18 -eq 0 -and $f.y%18 -eq 0) 'Malformed native rope frame'}
  }elseif($i -eq 12){
   Require ($c.kind -eq 'isolated-tile-api' -and $c.recognized -and -not $c.playerPlacementTested) 'API placement mislabeled as player evidence'
  }else{
   $rib=$i -eq 13
   Require ($c.kind -eq 'candidate-mining' -and $c.host -eq $hosts[($i-12)]) 'Mining case identity'
   Require ($c.power -eq $(if($rib){59}else{35}) -and $c.rejected58Calls -eq $(if($rib){30}else{0}) -and $c.hits -in 1..40) 'Mining power/call accounting'
   Require ($c.registeredDrop -ge 0) 'Invalid registered drop'
   foreach($d in $c.drops){Require ($d.type -gt 0 -and $d.stack -gt 0) 'Invalid actual drop'}
   if($r.schemaVersion -eq 2 -or $needBinding){
    Require ($c.registeredDrop -gt 0 -and $c.drops.Count -eq 1 -and $c.drops[0].type -eq $c.registeredDrop -and $c.drops[0].stack -eq 1) 'Anatomy pickup binding absent'
    $key=if($rib){'rib'}else{'cap'};$b=$c.binding
    Require ($b.itemName -ceq "apogean/AnatomyItem_$key" -and $b.tileName -ceq "apogean/Anatomy_$key") 'Wrong named binding'
    Require ($b.createTile -eq $b.tileType -and $b.tileType -gt 0 -and $b.itemType -eq $c.registeredDrop -and $b.consumable -and $b.width -eq 16 -and $b.height -eq 16 -and $b.maxStack -gt 1) 'Placeable item defaults'
    Require ($c.replacedType -eq $b.tileType -and $c.repickHits -in 1..40 -and $c.secondDrops.Count -eq 1 -and $c.secondDrops[0].type -eq $c.registeredDrop -and $c.secondDrops[0].stack -eq 1) 'Replacement cycle failed'
   }
  }
 }
 $pass=$null -eq $r.error -and $r.restored -and $r.historyUnchanged -and $r.before -eq $r.after -and $r.cases.Count -eq 15 -and $r.negativeControls -eq 2
 Require ($pass -eq $r.pass) 'Optimistic rope verdict'
 if($r.pass){Require ($r.checks -eq $(if($r.schemaVersion -eq 1){671}else{687})) 'Incomplete successful native checks'}
}
# Independently assembled schema exercise, never a game result.
$cases=@(0..11|ForEach-Object {@{kind='rope-api';host=@('vanilla-gray-brick','candidate-rib','candidate-fiber-cap')[[Math]::Floor($_/4)];anchor=@('above','left','right','below')[$_%4];placed=16;afterMining=15;ropeDrops=1;drops=@(@{type=965;stack=1});frames=@(0..15|ForEach-Object {@{x=0;y=0}})}})
$cases+=@{kind='isolated-tile-api';recognized=$true;playerPlacementTested=$false}
$cases+=@{kind='candidate-mining';host='candidate-rib';power=59;rejected58Calls=30;hits=5;registeredDrop=0;drops=@()}
$cases+=@{kind='candidate-mining';host='candidate-fiber-cap';power=35;rejected58Calls=0;hits=4;registeredDrop=0;drops=@()}
$good=@{schemaVersion=1;before='A'*64;after='A'*64;historyKnown=9;historyMissing=9;patch=@{X=100;Y=100;Width=32;Height=32};elapsedMs=1;checks=671;negativeControls=2;cases=$cases;error=$null;restored=$true;historyUnchanged=$true;pass=$true}
function Clone($r){$r|ConvertTo-Json -Depth 10|ConvertFrom-Json}
Check-Record (Clone $good)
$mutations=@(
 {param($r)$r.cases[0].ropeDrops=2}, {param($r)$r.cases[0].drops[0].type=9},
 {param($r)$r.cases[0].afterMining=0}, {param($r)$r.cases[1].anchor='above'},
 {param($r)$r.cases[4].host='vanilla-gray-brick'}, {param($r)$r.cases[0].frames[0].x=1},
 {param($r)$r.cases[12].playerPlacementTested=$true}, {param($r)$r.cases[13].power=58},
 {param($r)$r.cases[13].rejected58Calls=0}, {param($r)$r.cases[14].hits=41},
 {param($r)$r.cases=@($r.cases[0..13])}, {param($r)$r.restored=$false},
 {param($r)$r.historyUnchanged=$false}, {param($r)$r.after='B'*64},
 {param($r)$r.historyMissing=0}, {param($r)$r.checks=1},
 {param($r)$r.negativeControls=1}, {param($r)$r.error='Native failed'}
)
foreach($mutation in $mutations){$r=Clone $good;& $mutation $r;$caught=$false;try{Check-Record $r}catch{$caught=$true};Require $caught 'Rope evidence mutation survived'}
$partial=Clone $good;$partial.cases=@($partial.cases[0]);$partial.error='Next case failed';$partial.checks=60;$partial.negativeControls=1;$partial.pass=$false;Check-Record $partial
Write-Output "PASS $($mutations.Count) rope-report rejection controls and retained partial-failure control. Does not independently prove native state."
$bound=Clone $good;$bound.schemaVersion=2;$bound.checks=687
for($i=13;$i -le 14;$i++){
 $key=if($i -eq 13){'rib'}else{'cap'};$c=$bound.cases[$i];$c.registeredDrop=5000+$i;$c.drops=@([pscustomobject]@{type=5000+$i;stack=1})
 $c|Add-Member binding ([pscustomobject]@{itemName="apogean/AnatomyItem_$key";tileName="apogean/Anatomy_$key";createTile=1000+$i;tileType=1000+$i;itemType=5000+$i;consumable=$true;width=16;height=16;maxStack=9999})
 $c|Add-Member replacedType (1000+$i);$c|Add-Member repickHits 5;$c|Add-Member secondDrops @([pscustomobject]@{type=5000+$i;stack=1})
}
Check-Record $bound
$bindingMutations=@(
 {param($r)$r.cases[13].registeredDrop=0},{param($r)$r.cases[13].drops=@()},
 {param($r)$r.cases[14].drops[0].stack=2},{param($r)$r.cases[13].binding.itemName='apogean/AnatomyItem_cap'},
 {param($r)$r.cases[13].binding.createTile=1},{param($r)$r.cases[13].binding.consumable=$false},
 {param($r)$r.cases[14].replacedType=1},{param($r)$r.cases[14].secondDrops=@()},
 {param($r)$r.cases[13].secondDrops[0].type=1},{param($r)$r.cases[13].repickHits=41}
)
foreach($mutation in $bindingMutations){$r=Clone $bound;& $mutation $r;$caught=$false;try{Check-Record $r}catch{$caught=$true};Require $caught 'Anatomy binding defect survived'}
Write-Output "PASS $($bindingMutations.Count) schema2 binding/replacement defects; schema1 kept without retrospective guarantees."
foreach($file in $Path){$r=Get-Content -Raw -LiteralPath $file|ConvertFrom-Json;Check-Record $r ([bool]$RequireAnatomyItems);Write-Output "REPLAY $file : pass=$($r.pass); cases=$($r.cases.Count); restored=$($r.restored); known=$($r.historyKnown)/18. Native tile/mining APIs only, not player placement/ascent."}
