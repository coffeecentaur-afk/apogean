param([string]$CandidateDirectory=(Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v4'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$a=[Drawing.Bitmap]::new((Join-Path $CandidateDirectory 'cluster-atlas.png'))
$sprite=[Drawing.Bitmap]::new((Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png'))
$mask=[IO.File]::ReadAllBytes((Join-Path $CandidateDirectory 'contact-mask.bin'))
try{
    if($a.Width-ne832 -or $a.Height-ne144 -or $mask.Length-ne32768){throw 'CURVES_DIMENSIONS'}
    $expected=[int[]]::new(832*144)
    # Independent forward mapping from original sprite, not the v3 faces/compiler.
    for($v=0;$v-lt8;$v++){for($sy=0;$sy-lt64;$sy++){for($sx=0;$sx-lt64;$sx++){
        $f=$v%4;switch($f){0{$x=$sx;$y=$sy}1{$x=63-$sy;$y=$sx}2{$x=63-$sx;$y=63-$sy}3{$x=$sy;$y=$sx}}
        $wall=$f-eq1 -or $f-eq3
        if($v-ge4){if($wall){$y=63-$y}else{$x=63-$x}}
        $c=$sprite.GetPixel($sx,$sy);$argb=$c.ToArgb()
        $cx=[int][Math]::Floor($x/16);$cy=[int][Math]::Floor($y/16)
        $expected[($cy*18+$y%16)*832+$v*72+$cx*18+$x%16]=$argb
        if($mask[$v*4096+$y*64+$x]-ne[byte]($c.A-eq255)){throw 'CURVES_CONTACT'}
        if($wall){
            $pad=if($f-eq3){8}else{0}
            $expected[(72+$cy*18+$y%16)*832+$v*104+$cx*26+$x%16+$pad]=$argb
            $actualX=$cx*16-4+$x%16+$pad;$inset=if($f-eq1){-4}else{4}
            if($actualX-ne($x+$inset)){throw 'CURVES_WALL_PROJECTION'}
        }
    }}}
    for($y=0;$y-lt144;$y++){for($x=0;$x-lt832;$x++){if($a.GetPixel($x,$y).ToArgb()-ne$expected[$y*832+$x]){throw "CURVES_PIXELS_OR_PADDING at$x,$y"}}}
    $recipe=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'recipe.json')|ConvertFrom-Json
    if($recipe.schema-ne4 -or $recipe.variants-ne8 -or $recipe.styleMultiplier-ne4 -or $recipe.styleWrapLimit-ne8){throw 'CURVES_METADATA'}
    $sourceHash=(Get-FileHash (Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png')).Hash
    if($sourceHash-ne$recipe.sourceSHA256 -or (Get-FileHash (Join-Path $CandidateDirectory 'cluster.png')).Hash-ne$sourceHash){throw 'CURVES_SOURCE'}
    if((Get-FileHash (Join-Path $CandidateDirectory 'cluster-atlas.png')).Hash-ne$recipe.atlasSHA256 -or (Get-FileHash (Join-Path $CandidateDirectory 'contact-mask.bin')).Hash-ne$recipe.maskSHA256){throw 'CURVES_PROVENANCE'}
    Write-Host 'PASS: 119808 exact atlas/padding pixels,32768 contact samples,16384 wall projections; all eight curves; original saved frames unchanged.'
}finally{$a.Dispose();$sprite.Dispose()}
