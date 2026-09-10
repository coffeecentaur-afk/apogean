param([string]$CandidateDirectory=(Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/MawTerrain-Harsh-v1/Native-v2/Stone'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=(Resolve-Path -LiteralPath $CandidateDirectory).Path
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanWorldMaterialTests-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$runner=(Get-Process -Id $PID).Path
$entry=Join-Path $PSScriptRoot 'Test-MawWorldMaterial.ps1'
Add-Type -AssemblyName System.Drawing
$count=0
foreach($case in @('positive','alpha','palette','phase','dimensions','period','sourceHash','claims')) {
 $folder=Join-Path $scratch $case
 New-Item -ItemType Directory -Path $folder | Out-Null
 foreach($file in @('atlas.png','material-native.png','recipe.json')){Copy-Item -LiteralPath (Join-Path $source $file) -Destination $folder}
 $report=Get-Content -Raw -LiteralPath (Join-Path $folder 'recipe.json') | ConvertFrom-Json
 $imagePath=Join-Path $folder 'atlas.png'
 if($case -in @('alpha','palette','phase','dimensions')) {
  $b=[Drawing.Bitmap]::new($imagePath)
  $changed=if($case-eq'dimensions'){[Drawing.Bitmap]::new(16,16)}else{[Drawing.Bitmap]::new($b)}
  try {
   $found=$false
   for($y=0;$y-lt270-and-not$found;$y++){for($x=0;$x-lt288-and-not$found;$x++){
    if($b.GetPixel($x,$y).A-eq255) {
     switch($case){
      alpha {$changed.SetPixel($x,$y,[Drawing.Color]::Transparent)}
      palette {$changed.SetPixel($x,$y,[Drawing.Color]::White)}
      phase {
       $old=$b.GetPixel($x,$y);$other=[Drawing.Color]::FromArgb(17,18,20)
       if($old.ToArgb()-eq$other.ToArgb()){$other=[Drawing.Color]::FromArgb(201,144,60)}
       $changed.SetPixel($x,$y,$other)
      }
     }
     $found=$true
    }
   }}
   $b.Dispose()
   $changed.Save($imagePath,[Drawing.Imaging.ImageFormat]::Png)
  }finally{$b.Dispose();$changed.Dispose()}
  # Update provenance deliberately: force the independent pixel gate to catch it.
  $report.atlasSHA256=(Get-FileHash -LiteralPath $imagePath).Hash
 }
 if($case-eq'period'){
  $path=Join-Path $folder 'material-native.png';$b=[Drawing.Bitmap]::new($path)
  $copy=[Drawing.Bitmap]::new($b);$b.Dispose()
  try{$copy.SetPixel(0,0,[Drawing.Color]::White);$copy.Save($path,[Drawing.Imaging.ImageFormat]::Png)}
  finally{$copy.Dispose()}
 }
 if($case-eq'sourceHash'){$report.sourceSHA256='0'*64}
 if($case-eq'claims'){$report.nativeRendered=$true}
 $report|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $folder 'recipe.json') -Encoding utf8
 $log=& $runner -NoProfile -File $entry -CandidateDirectory $folder 2>&1
 $expected=switch($case){alpha{'ALPHA_TOPOLOGY'}palette{'PALETTE'}phase{'WORLD_COLOR'}dimensions{'DIMENSIONS'}period{'WORLD_COLOR|PERIOD_BOUNDARY'}sourceHash{'SOURCE_HASH'}claims{'FALSE_EVIDENCE'}default{''}}
 if($case-eq'positive'){if($LASTEXITCODE-ne0){throw "POSITIVE_FAILED $log"}}
 elseif($LASTEXITCODE-eq0-or($log|Out-String)-notmatch$expected){throw "MUTATION_SURVIVED $case $log"}
 Write-Output "PASS real CLI $case";$count++
}
Write-Output "PASS $count CLI controls; retained evidence $scratch"
