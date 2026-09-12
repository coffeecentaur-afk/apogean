param([string]$CandidateDirectory=(Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawRibSurface-v1/Native-v1'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanAnatomyControls-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
Add-Type -AssemblyName System.Drawing
$count=0
foreach($case in @('positive','rib-alpha','cap-alpha','guard','index')){
 $folder=Join-Path $scratch $case
 New-Item -ItemType Directory -Path $folder | Out-Null
 Copy-Item -LiteralPath (Join-Path $CandidateDirectory 'recipe.json') -Destination $folder
 foreach($key in @('rib','cap')){Copy-Item -LiteralPath (Join-Path $CandidateDirectory $key) -Destination $folder -Recurse}
 $key=if($case -eq 'cap-alpha'){'cap'}else{'rib'}
 if($case -eq 'index'){
  $stream=[IO.File]::OpenWrite((Join-Path $folder "$key/map.bin"))
  try{$stream.Position=32;$bad=[BitConverter]::GetBytes([int]2147483647);$stream.Write($bad,0,4)}finally{$stream.Dispose()}
 }elseif($case -ne 'positive'){
  $path=Join-Path $folder "$key/atlas.png"
  $original=[Drawing.Bitmap]::new($path);$changed=[Drawing.Bitmap]::new($original)
  try{
   if($case -eq 'guard'){$changed.SetPixel(16,0,[Drawing.Color]::White)}else{
    $found=$false
    for($y=0;$y -lt $changed.Height -and -not $found;$y++){for($x=0;$x -lt $changed.Width -and -not $found;$x++){
     if($changed.GetPixel($x,$y).A -eq 255){$changed.SetPixel($x,$y,[Drawing.Color]::FromArgb(0,0,0,0));$found=$true}
    }}
    if(-not $found){throw 'NO_OPAQUE_PIXEL'}
   }
   $original.Dispose();$changed.Save($path,[Drawing.Imaging.ImageFormat]::Png)
  }finally{$original.Dispose();$changed.Dispose()}
 }
 $result=& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawAnatomyCandidate.ps1') -CandidateDirectory $folder 2>&1
 $exit=$LASTEXITCODE
 if($case -eq 'positive'){if($exit -ne 0){throw "POSITIVE_FAILED $result"}}
 elseif($exit -eq 0 -or ($result -join "`n") -notmatch 'PIXEL_CONTRACT|INDEX'){throw "MUTATION_SURVIVED $case $result"}
 Write-Output "PASS actual CLI control: $case";$count++
}
Write-Output "PASS $count controls; malformed candidates retained only in $scratch"
