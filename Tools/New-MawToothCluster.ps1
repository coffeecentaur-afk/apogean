param([string]$OutputDirectory = (Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v2'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'Art/Candidates/MawToothFamily-v1/Native-v1'
$recipe = Get-Content -Raw (Join-Path $source 'recipe.json') | ConvertFrom-Json
# Exact integer placement only: approved pixels, no filtering, redraw or enlargement.
$parts = @(@('long', 1), @('short', 10), @('wide', 17), @('long', 38), @('short', 47))
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$sprite = [Drawing.Bitmap]::new(64,64)
$atlas = [Drawing.Bitmap]::new(288,72)
$mask = [byte[]]::new(16384)
try {
    foreach ($part in $parts) {
        $path = Join-Path $source "$($part[0])/tooth.png"
        $record = $recipe.records | Where-Object name -eq $part[0]
        if ((Get-FileHash -LiteralPath $path).Hash -ne $record.spriteSHA256) { throw 'APPROVED_INPUT_CHANGED' }
        $inputSprite = [Drawing.Bitmap]::new($path)
        try {
            for ($y=0; $y -lt $inputSprite.Height; $y++) {
                for ($x=0; $x -lt $inputSprite.Width; $x++) {
                    $c=$inputSprite.GetPixel($x,$y)
                    if ($c.A -eq 255) { $sprite.SetPixel($x+$part[1],$y+64-$inputSprite.Height,$c) }
                    elseif ($c.A -ne 0) { throw 'SOFT_INPUT' }
                }
            }
        } finally { $inputSprite.Dispose() }
    }
    for ($orientation=0;$orientation -lt 4;$orientation++) {
    for ($y=0;$y -lt 64;$y++) {
        for ($x=0;$x -lt 64;$x++) {
            $c=$sprite.GetPixel($x,$y)
            $rx=$x;$ry=$y
            for($turn=0;$turn-lt$orientation;$turn++){$nextX=63-$ry;$ry=$rx;$rx=$nextX}
            $atlas.SetPixel($orientation*72+[int][Math]::Floor($rx/16)*18+$rx%16,[int][Math]::Floor($ry/16)*18+$ry%16,$c)
            if ($c.A -eq 255) { $mask[$orientation*4096+$ry*64+$rx]=1 }
        }
    }
    }
    $sprite.Save((Join-Path $OutputDirectory 'cluster.png'),[Drawing.Imaging.ImageFormat]::Png)
    $atlas.Save((Join-Path $OutputDirectory 'cluster-atlas.png'),[Drawing.Imaging.ImageFormat]::Png)
    [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'contact-mask.bin'),$mask)
    [ordered]@{
        schema=2; size=@(64,64); cells=@(4,4); atlasSize=@(288,72)
        orientations=@('floor','left-wall','ceiling','right-wall')
        drawOffsets=@(@(0,4),@(0,0),@(0,-4),@(0,0))
        assembly='integer translated approved pixels; source-over ordering; no new art or filtering'
        parts=@($parts | ForEach-Object { [ordered]@{variant=$_[0];x=$_[1];bottom=64} })
        opaque=($mask | Where-Object {$_ -eq 1}).Count
        spriteSHA256=(Get-FileHash (Join-Path $OutputDirectory 'cluster.png')).Hash
        atlasSHA256=(Get-FileHash (Join-Path $OutputDirectory 'cluster-atlas.png')).Hash
        maskSHA256=(Get-FileHash (Join-Path $OutputDirectory 'contact-mask.bin')).Hash
    } | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $OutputDirectory 'recipe.json') -Encoding utf8
} finally { $sprite.Dispose(); $atlas.Dispose() }
Write-Host 'Assembled five unchanged teeth, four exact integer rotations; original upright pixels preserved.'
