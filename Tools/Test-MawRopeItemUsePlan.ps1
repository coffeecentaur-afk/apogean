Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw (Join-Path $PSScriptRoot '../Content/Diagnostics/MawRopeItemUsePlan.cs')
$source=$source.Replace('internal static','public static').Replace('internal const','public const')
Add-Type -TypeDefinition $source
$type=[apogean.Content.Diagnostics.MawRopeItemUsePlan]
$checks=0
function Need($ok,[string]$why){if(-not $ok){throw $why};$script:checks++}
foreach($w in @('Apogee Native Visual V3','aga','')){foreach($p in @('gg','Maw QA Plain','Maw QA Rope','')){foreach($sp in @($true,$false)){foreach($menu in @($true,$false)){
 Need ($type::Context($w,$p,$sp,$menu) -eq ($w -ceq 'Apogee Native Visual V3' -and $p -ceq 'gg' -and $sp -and -not $menu)) 'Context leak'
}}}}
foreach($v in @('vanilla','rib','cap','wall','diagonal','none','far')){
 Need ($type::Variant($v)) 'Variant refused';Need ($type::ExpectedPlacement($v) -eq ($v -in @('vanilla','rib','cap','wall'))) 'Expected result'
 $target=$type::TargetX($v);Need ($target -eq $(if($v -eq 'far'){28}else{11})) 'Target'
 $floor=0;$support=0;$wall=0
 for($x=0;$x -lt 32;$x++){for($y=0;$y -lt 32;$y++){
  $f=$type::Floor($x,$y);$s=$type::Support($v,$x,$y);$a=$type::Wall($v,$x,$y)
  if($f){$floor++};if($s){$support++};if($a){$wall++}
  Need (-not ($f -and $s)) 'Overlapping support';Need (-not (($f -or $s) -and $x -eq $target -and $y -eq 23)) 'Target prefilled'
  Need (-not $f -or ($x -ge 5 -and $x -le 12 -and $y -eq 26)) 'Floor shape'
  Need (-not $s -or ($x -eq $target-1 -and $y -eq $(if($v -eq 'diagonal'){22}else{23}))) 'Support shape'
 }}
 Need ($floor -eq 8) 'Floor count';Need ($support -eq $(if($v -in @('none','wall')){0}else{1})) 'Support count';Need ($wall -eq $(if($v -eq 'wall'){1}else{0})) 'Wall count'
}
foreach($v in @('','stop','fake')){Need (-not $type::Variant($v)) 'Unknown variant accepted'}
foreach($tick in -1..61){foreach($occupied in @($true,$false)){Need ($type::Use($tick,$occupied) -eq ($tick -eq 1 -and -not $occupied)) 'Use escape or auto-extension risk'}}
Write-Output "PASS $checks scope/geometry/control checks; not native item-use, reach or consumption proof."
