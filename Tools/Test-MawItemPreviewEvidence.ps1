param([string[]]$EvidencePath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Check($r){
 if($r.Schema -ne 1 -or $r.Reason -ne 'new-command' -or $r.Draws -lt 1){throw 'ITEM_PREVIEW_EVIDENCE: incomplete or failed preview'}
 $f=$r.FirstFrame
 if($null -eq $f -or $f.GameUpdate -lt 1 -or $f.UiScale -le 0 -or $f.UiScale -gt 4 -or
    $f.OriginalInventoryScale -le 0 -or $f.OriginalInventoryScale -gt 4 -or $f.SlotScale -ne 1 -or $f.Context -ne 31 -or
    $f.SlotTextureWidth -ne 52 -or $f.SlotTextureHeight -ne 52 -or $f.SlotLeft -ne 100 -or $f.SlotTop -ne 230 -or $f.SlotSpacing -ne 192){throw 'ITEM_PREVIEW_EVIDENCE: wrong native comparison geometry'}
 if($f.Hooks.Count -ne 2){throw 'ITEM_PREVIEW_EVIDENCE: missing/extra hooks'}
 for($i=0;$i -lt 2;$i++){
  $h=$f.Hooks[$i];$key=if($i -eq 0){'rib'}else{'cap'};$sourceY=if($i -eq 0){180}else{270}
  if($h.Index -ne $i -or $h.Name -cne ('apogean/AnatomyItem_'+$key) -or $h.ItemType -le 0 -or
     $h.CenterX -ne ($f.SlotLeft+$i*$f.SlotSpacing+$f.SlotTextureWidth/2) -or $h.CenterY -ne ($f.SlotTop+$f.SlotTextureHeight/2) -or
     $h.Scale -ne 1 -or $h.SourceX -ne 0 -or $h.SourceY -ne $sourceY -or $h.SourceWidth -ne 16 -or $h.SourceHeight -ne 16 -or
     $h.FallbackWidth -ne 16 -or $h.FallbackHeight -ne 16 -or $h.FallbackOriginX -ne 8 -or $h.FallbackOriginY -ne 8){throw 'ITEM_PREVIEW_EVIDENCE: wrong candidate identity/frame/center/scale'}
 }
 if($f.Hooks[0].ItemType -eq $f.Hooks[1].ItemType){throw 'ITEM_PREVIEW_EVIDENCE: duplicated candidate type'}
}
$baseline=@{
 Schema=1;Reason='new-command';Draws=100;FirstFrame=@{
  GameUpdate=123;UiScale=1.15;OriginalInventoryScale=.75;SlotScale=1;Context=31;
  SlotTextureWidth=52;SlotTextureHeight=52;SlotLeft=100;SlotTop=230;SlotSpacing=192;
  Hooks=@(
   @{Index=0;ItemType=5546;Name='apogean/AnatomyItem_rib';CenterX=126;CenterY=256;Scale=1;SourceX=0;SourceY=180;SourceWidth=16;SourceHeight=16;FallbackWidth=16;FallbackHeight=16;FallbackOriginX=8;FallbackOriginY=8},
   @{Index=1;ItemType=5547;Name='apogean/AnatomyItem_cap';CenterX=318;CenterY=256;Scale=1;SourceX=0;SourceY=270;SourceWidth=16;SourceHeight=16;FallbackWidth=16;FallbackHeight=16;FallbackOriginX=8;FallbackOriginY=8})
 }
}
Check $baseline
$controls=@(
 {param($r)$r.Schema=2}, {param($r)$r.Reason='draw-failure'}, {param($r)$r.Draws=0},
 {param($r)$r.FirstFrame=$null}, {param($r)$r.FirstFrame.Context=0}, {param($r)$r.FirstFrame.UiScale=0},
 {param($r)$r.FirstFrame.SlotScale=2}, {param($r)$r.FirstFrame.Hooks=@($r.FirstFrame.Hooks[0])},
 {param($r)$r.FirstFrame.Hooks[0].CenterX=100}, {param($r)$r.FirstFrame.Hooks[1].CenterY=230},
 {param($r)$r.FirstFrame.Hooks[1].SourceWidth=288}, {param($r)$r.FirstFrame.Hooks[1].SourceY=180},
 {param($r)$r.FirstFrame.Hooks[0].Name='apogean/AnatomyItem_cap'},
 {param($r)$r.FirstFrame.Hooks[1].ItemType=$r.FirstFrame.Hooks[0].ItemType},
 {param($r)$r.FirstFrame.Hooks[0].Scale=.5}, {param($r)$r.FirstFrame.Hooks[0].FallbackOriginX=0})
foreach($mutation in $controls){
 $r=$baseline|ConvertTo-Json -Depth 10|ConvertFrom-Json -AsHashtable
 & $mutation $r
 $caught=$false
 try{Check $r}catch{$caught=$true}
 if(-not $caught){throw 'Bad preview report survived.'}
}
Write-Output 'PASS one synthetic baseline and16 malformed-report controls. Geometry validation only, not screenshot/scale restoration proof.'
foreach($path in $EvidencePath){Check (Get-Content -LiteralPath $path -Raw|ConvertFrom-Json);Write-Output "PASS native first-frame replay: $path"}
