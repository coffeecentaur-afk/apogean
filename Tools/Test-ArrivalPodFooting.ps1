Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$old=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v2'
$new=Join-Path $repo 'Art/Candidates/ArrivalPod-v1/Native-v3'
$validator=Join-Path $PSScriptRoot 'Test-ArrivalPodNative.ps1'
$generator=Join-Path $PSScriptRoot 'New-ArrivalPodFooting.ps1'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ArrivalPodFooting-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$pins=@{}
foreach($dir in @($old,$new)){foreach($name in @('ArrivalPod.png','ArrivalPod_Tile.png')){$p=Join-Path $dir $name;$pins[$p]=(Get-FileHash $p).Hash}}
$red=& pwsh -NoProfile -File $validator -Directory $old -PixelClusterSize 2 -RequireFlatFooting 2>&1
if($LASTEXITCODE -eq 0 -or ($red -join ' ') -notmatch 'FLAT_FOOTING_GAP'){throw 'CURVED_BASE_NOT_REJECTED'}
& pwsh -NoProfile -File $validator -Directory $new -PixelClusterSize 2 -RequireFlatFooting
if($LASTEXITCODE -ne 0){throw 'NEW_FOOTING_FAILED'}
$a=[Drawing.Bitmap]::new((Join-Path $old 'ArrivalPod.png'))
$b=[Drawing.Bitmap]::new((Join-Path $new 'ArrivalPod.png'))
try{for($y=0;$y -lt 88;$y++){for($x=0;$x -lt 80;$x++){if($a.GetPixel($x,$y).ToArgb() -ne $b.GetPixel($x,$y).ToArgb()){throw 'APPROVED_UPPER_ART_CHANGED'}}}}finally{$a.Dispose();$b.Dispose()}
& pwsh -NoProfile -File $generator -OutputDirectory $scratch
if($LASTEXITCODE -ne 0){throw 'REPLAY_FAILED'}
foreach($name in @('ArrivalPod.png','ArrivalPod_Tile.png')){if((Get-FileHash (Join-Path $scratch $name)).Hash -ne $pins[(Join-Path $new $name)]){throw 'NONDETERMINISTIC_OUTPUT'}}
$refused=& pwsh -NoProfile -File $generator -OutputDirectory $scratch 2>&1
if($LASTEXITCODE -eq 0 -or ($refused -join ' ') -notmatch 'OUTPUT_EXISTS'){throw 'OVERWRITE_NOT_REFUSED'}
$refused=& pwsh -NoProfile -File $generator -OutputDirectory (Join-Path $repo 'Content') 2>&1
if($LASTEXITCODE -eq 0 -or ($refused -join ' ') -notmatch 'PRODUCTION_OUTPUT_FORBIDDEN'){throw 'CONTENT_NOT_REFUSED'}
# Defective fixture data: preserve the grid and atlas roundtrip but punch a
# two-pixel contact hole. The new gate, not unrelated alpha/grid gates, must fail.
$mutant=Join-Path $scratch 'contact-hole';New-Item -ItemType Directory -Path $mutant | Out-Null
$a=[Drawing.Bitmap]::new((Join-Path $new 'ArrivalPod.png'))
$b=[Drawing.Bitmap]::new((Join-Path $new 'ArrivalPod_Tile.png'))
try {
 for($y=94;$y -lt 96;$y++){for($x=20;$x -lt 22;$x++){
  $a.SetPixel($x,$y,[Drawing.Color]::FromArgb(0,0,0,0))
  $b.SetPixel(([int][Math]::Floor($x/16)*18+$x%16),([int][Math]::Floor($y/16)*18+$y%16),[Drawing.Color]::FromArgb(0,0,0,0))
 }}
 $a.Save((Join-Path $mutant 'ArrivalPod.png'),[Drawing.Imaging.ImageFormat]::Png)
 $b.Save((Join-Path $mutant 'ArrivalPod_Tile.png'),[Drawing.Imaging.ImageFormat]::Png)
}finally{$a.Dispose();$b.Dispose()}
$red=& pwsh -NoProfile -File $validator -Directory $mutant -PixelClusterSize 2 -RequireFlatFooting 2>&1
if($LASTEXITCODE -eq 0 -or ($red -join ' ') -notmatch 'FLAT_FOOTING_GAP'){throw 'CONTACT_HOLE_NOT_REJECTED'}
foreach($p in $pins.Keys){if((Get-FileHash $p).Hash -ne $pins[$p]){throw 'PINNED_ART_CHANGED'}}
Write-Output "PASS: old curved base and contact-hole CLI controls rejected; new flat base passes; 7040 upper pixels preserved; deterministic atlas replay; overwrite/Content refused; original pins unchanged. Scratch=$scratch"
