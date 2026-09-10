param([string]$CandidateDirectory = (Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v2'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
if (-not(Test-Path (Join-Path $CandidateDirectory 'cluster.png'))) { throw 'MISSING_CLUSTER' }
$sprite=[Drawing.Bitmap]::new((Join-Path $CandidateDirectory 'cluster.png'))
$atlas=[Drawing.Bitmap]::new((Join-Path $CandidateDirectory 'cluster-atlas.png'))
$mask=[IO.File]::ReadAllBytes((Join-Path $CandidateDirectory 'contact-mask.bin'))
try {
    if ($sprite.Width-ne64 -or $sprite.Height-ne64 -or $atlas.Width-ne288 -or $atlas.Height-ne72 -or $mask.Length-ne16384) { throw 'DIMENSIONS' }
    $palette=[Collections.Generic.HashSet[int]]::new(); $opaque=0
    for($y=0;$y-lt72;$y++){for($x=0;$x-lt288;$x++){
        $a=$atlas.GetPixel($x,$y)
        if($x%18-ge16 -or $y%18-ge16){if($a.ToArgb()-ne0){throw 'PADDING'};continue}
        $orientation=[int][Math]::Floor($x/72)
        $rx=[int][Math]::Floor(($x%72)/18)*16+$x%18;$ry=[int][Math]::Floor($y/18)*16+$y%18
        # Independent inverse rotation, not the compiler's forward iteration.
        switch($orientation){0{$sx=$rx;$sy=$ry}1{$sx=$ry;$sy=63-$rx}2{$sx=63-$rx;$sy=63-$ry}3{$sx=63-$ry;$sy=$rx}}
        $c=$sprite.GetPixel($sx,$sy)
        if($a.ToArgb()-ne$c.ToArgb()){throw 'ATLAS_REASSEMBLY'}
        if($c.A-eq255){$opaque++;[void]$palette.Add($c.ToArgb());if($c.R-eq255 -and $c.G-eq255 -and $c.B-eq255){throw 'WHITE_KEY'}}
        elseif($c.ToArgb()-ne0){throw 'ALPHA'}
        if($mask[$orientation*4096+$ry*64+$rx]-ne[byte]($c.A-eq255)){throw 'CONTACT_MASK'}
    }}
    if($palette.Count-ne8 -or $opaque-ne5912){throw 'PALETTE_OR_DENSITY'}
    $approved=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png'
    if((Get-FileHash $approved).Hash-ne(Get-FileHash (Join-Path $CandidateDirectory 'cluster.png')).Hash){throw 'APPROVED_SPRITE_CHANGED'}
    $recipe=Get-Content -Raw (Join-Path $CandidateDirectory 'recipe.json')|ConvertFrom-Json
    foreach($entry in @(@('cluster.png','spriteSHA256'),@('cluster-atlas.png','atlasSHA256'),@('contact-mask.bin','maskSHA256'))){
        if((Get-FileHash (Join-Path $CandidateDirectory $entry[0])).Hash-ne$recipe.($entry[1])){throw 'PROVENANCE'}
    }
    Write-Host "PASS: unchanged cluster 64x64, atlas288x72, four exact rotations, $opaque opaque pixels, $($palette.Count) colors; 16384 mask comparisons. Native behavior remains separate."
} finally {$sprite.Dispose();$atlas.Dispose()}
