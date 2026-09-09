param([Parameter(Mandatory)][string]$CandidateDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$spritePath = Join-Path $CandidateDirectory 'tooth.png'
$atlasPath = Join-Path $CandidateDirectory 'upright-art-atlas.png'
if (!(Test-Path -LiteralPath $spritePath) -or !(Test-Path -LiteralPath $atlasPath)) { throw 'MISSING_SLIM_TOOTH_ASSET' }
Add-Type -AssemblyName System.Drawing
$sprite = [Drawing.Bitmap]::new([IO.Path]::GetFullPath($spritePath))
$atlas = [Drawing.Bitmap]::new([IO.Path]::GetFullPath($atlasPath))
try {
    if ($sprite.Width -ne 16 -or $sprite.Height -ne 48) { throw 'SLIM_TOOTH_DIMENSIONS' }
    if ($atlas.Width -ne 18 -or $atlas.Height -ne 54) { throw 'SLIM_ATLAS_DIMENSIONS' }
    $palette = [Collections.Generic.HashSet[int]]::new()
    $last = @(); $opaque = 0; $minX=16; $maxX=-1
    foreach ($y in 0..47) {
        $row = @()
        foreach ($x in 0..15) {
            $c=$sprite.GetPixel($x,$y)
            if ($c.A -notin @(0,255)) { throw 'SOFT_ALPHA' }
            if ($c.A -eq 0) { if ($c.ToArgb() -ne 0) { throw 'HIDDEN_MATTE_RGB' }; continue }
            if ([Math]::Max($c.R,[Math]::Max($c.G,$c.B)) -gt 220) { throw 'BRIGHT_EDGE_OR_SPECKLE' }
            $null=$palette.Add($c.ToArgb()); $row+=,$x; $opaque++
            $minX=[Math]::Min($minX,$x); $maxX=[Math]::Max($maxX,$x)
        }
        if ($row.Count -eq 0) { throw "SEVERED_TOOTH_ROW_$y" }
        if ($row.Count -ne ($row[-1]-$row[0]+1)) { throw 'SILHOUETTE_HOLE' }
        if ($y -eq 0 -and $row.Count -gt 2) { throw 'BLUNT_TIP' }
        if ($y -gt 0 -and ($row[0] -gt $last[-1] -or $row[-1] -lt $last[0])) { throw 'DISCONNECTED_SHAFT' }
        if ($y -eq 36 -and $row.Count -lt 4) { throw 'MISSING_LOWER_SHAFT' }
        $last=$row
    }
    if ($maxX-$minX+1 -gt 16 -or $maxX-$minX+1 -lt 7) { throw 'FOOTPRINT_WIDTH' }
    if ($palette.Count -lt 3 -or $palette.Count -gt 8) { throw 'PALETTE_COUNT' }
    foreach ($y in 0..53) { foreach ($x in 0..17) {
        $c=$atlas.GetPixel($x,$y)
        if ($x -ge 16 -or $y%18 -ge 16) { if ($c.ToArgb() -ne 0) { throw 'ATLAS_PADDING' }; continue }
        $sy=[int][Math]::Floor($y/18)*16+$y%18
        if ($c.ToArgb() -ne $sprite.GetPixel($x,$sy).ToArgb()) { throw 'ATLAS_REASSEMBLY_CHANGED_ART' }
    }}
    Write-Host "PASS: 16x48 canvas, $($maxX-$minX+1)px occupied width, $opaque opaque pixels, $($palette.Count) colors; intact tip/shaft; exact 18x54 art-only atlas. No physics or live-render claim."
} finally { $sprite.Dispose(); $atlas.Dispose() }
