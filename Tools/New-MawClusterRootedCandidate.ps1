param([string]$OutputDirectory=(Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v3'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$approved=Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v1/cluster.png'
$inputSprite=[Drawing.Bitmap]::new($approved)
$atlas=[Drawing.Bitmap]::new(416,144)
$faces=[Drawing.Bitmap]::new(256,64)
$mask=[byte[]]::new(16384)
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
try {
    for($face=0;$face-lt4;$face++){for($y=0;$y-lt64;$y++){for($x=0;$x-lt64;$x++){
        # Floor and ceiling unchanged. Right wall mirrors left wall, keeping
        # rounded backs below and tips curving upward in WORLD coordinates.
        switch($face){0{$sx=$x;$sy=$y}1{$sx=$y;$sy=63-$x}2{$sx=63-$x;$sy=63-$y}3{$sx=$y;$sy=$x}}
        $c=$inputSprite.GetPixel($sx,$sy)
        $faces.SetPixel($face*64+$x,$y,$c)
        $atlas.SetPixel($face*72+[int][Math]::Floor($x/16)*18+$x%16,[int][Math]::Floor($y/16)*18+$y%16,$c)
        $mask[$face*4096+$y*64+$x]=[byte]($c.A-eq255)
        if($face-eq1 -or $face-eq3){
            # Placed draw region: centered24px native cells, with8 transparent
            # pixels on the outer side. Visible16px cell shifts inward4px.
            $padLeft=if($face-eq3){8}else{0}
            $atlas.SetPixel($face*104+[int][Math]::Floor($x/16)*26+$x%16+$padLeft,72+[int][Math]::Floor($y/16)*18+$y%16,$c)
        }
    }}}
    $atlas.Save((Join-Path $OutputDirectory 'cluster-atlas.png'),[Drawing.Imaging.ImageFormat]::Png)
    $faces.Save((Join-Path $OutputDirectory 'faces.png'),[Drawing.Imaging.ImageFormat]::Png)
    Copy-Item -LiteralPath $approved -Destination (Join-Path $OutputDirectory 'cluster.png')
    [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'contact-mask.bin'),$mask)
    [ordered]@{schema=3;atlasSize=@(416,144);savedCellStride=18;previewSize=@(288,72);wallDrawRegionY=72;wallDrawCellStride=26;wallDrawWidth=24;drawOffsets=@(@(0,4),@(-4,0),@(0,-4),@(4,0));rightWall='horizontal mirror of left; backs underneath, tips curl upward';sourceSHA256=(Get-FileHash $approved).Hash;atlasSHA256=(Get-FileHash (Join-Path $OutputDirectory 'cluster-atlas.png')).Hash;maskSHA256=(Get-FileHash (Join-Path $OutputDirectory 'contact-mask.bin')).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $OutputDirectory 'recipe.json') -Encoding utf8
} finally {$inputSprite.Dispose();$atlas.Dispose();$faces.Dispose()}
Write-Host 'Compiled unchanged tooth pixels into rooted wall drawing cells; right wall mirrors left.'
