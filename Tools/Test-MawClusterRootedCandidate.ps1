param([string]$CandidateDirectory=(Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v3'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$atlas=[Drawing.Bitmap]::new((Join-Path $CandidateDirectory 'cluster-atlas.png'))
$original=[Drawing.Bitmap]::new((Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png'))
$mask=[IO.File]::ReadAllBytes((Join-Path $CandidateDirectory 'contact-mask.bin'))
try {
    if($atlas.Width-ne416 -or $atlas.Height-ne144 -or $mask.Length-ne16384){throw 'ROOTED_DIMENSIONS'}
    $expected=[int[]]::new(416*144);$opaque=0
    # Forward mapping differs from compiler inverse mapping.
    for($f=0;$f-lt4;$f++){for($sy=0;$sy-lt64;$sy++){for($sx=0;$sx-lt64;$sx++){
        switch($f){0{$x=$sx;$y=$sy}1{$x=63-$sy;$y=$sx}2{$x=63-$sx;$y=63-$sy}3{$x=$sy;$y=$sx}}
        $c=$original.GetPixel($sx,$sy);$v=if($c.A-eq0){0}else{$c.ToArgb()}
        $cx=[int][Math]::Floor($x/16);$cy=[int][Math]::Floor($y/16)
        $expected[($cy*18+$y%16)*416+$f*72+$cx*18+$x%16]=$v
        if($mask[$f*4096+$y*64+$x]-ne[byte]($c.A-eq255)){throw 'ROOTED_MASK'}
        if($f-eq1 -or $f-eq3){
            $inCell=if($f-eq3){$x%16+8}else{$x%16}
            $expected[(72+$cy*18+$y%16)*416+$f*104+$cx*26+$inCell]=$v
            # Installed TileDrawing centers width24 around16px world cells.
            $actualX=$cx*16-(24-16)/2+$inCell
            $wantedX=$x+$(if($f-eq1){-4}else{4})
            if($actualX-ne$wantedX){throw 'ROOTED_DRAW_PROJECTION'}
        }
        if($c.A-eq255){$opaque++}
    }}}
    for($y=0;$y-lt144;$y++){for($x=0;$x-lt416;$x++){
        if($atlas.GetPixel($x,$y).ToArgb()-ne$expected[$y*416+$x]){throw "ROOTED_PIXELS_OR_PADDING at$x,$y"}
    }}
    if($opaque-ne5912){throw 'ROOTED_DENSITY'}
    $recipe=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'recipe.json')|ConvertFrom-Json
    $approvedHash=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png')).Hash
    if($recipe.sourceSHA256-ne$approvedHash -or (Get-FileHash -LiteralPath (Join-Path $CandidateDirectory 'cluster.png')).Hash-ne$approvedHash){throw 'ROOTED_SOURCE_OR_ITEM'}
    if((Get-FileHash (Join-Path $CandidateDirectory 'cluster-atlas.png')).Hash-ne$recipe.atlasSHA256 -or (Get-FileHash (Join-Path $CandidateDirectory 'contact-mask.bin')).Hash-ne$recipe.maskSHA256){throw 'ROOTED_PROVENANCE'}
    Write-Host 'PASS: original art preserved; mirrored wall curves; 16384 masks,59904 atlas pixels and8192 native centered-cell projections.'
} finally {$atlas.Dispose();$original.Dispose()}
