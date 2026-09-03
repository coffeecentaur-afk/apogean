param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$CaptureRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
$p = @((C '#211916'), (C '#35261f'), (C '#523827'), (C '#735033'), (C '#967041'), (C '#ccb17d'), (C '#efdbae'), (C '#9f5d13'), (C '#d18a20'))

function New-Canvas([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Save-Canvas($canvas, [string]$path) {
    $canvas.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $canvas.Graphics.Dispose(); $canvas.Bitmap.Dispose()
}

function Draw-Path([System.Drawing.Graphics]$g, [System.Drawing.Point[]]$points, [int]$width = 5, [switch]$Bone) {
    $bodyColor = if ($Bone) { $p[5] } else { $p[3] }
    $highlightColor = if ($Bone) { $p[6] } else { $p[4] }
    $outline = [System.Drawing.Pen]::new($p[0], $width + 3)
    $body = [System.Drawing.Pen]::new($bodyColor, $width)
    $highlight = [System.Drawing.Pen]::new($highlightColor, 1)
    try {
        foreach ($pen in @($outline, $body, $highlight)) {
            $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Miter
            $g.DrawLines($pen, $points)
        }
    } finally { $outline.Dispose(); $body.Dispose(); $highlight.Dispose() }
}

function Draw-TaperedLimb(
    [System.Drawing.Graphics]$g,
    [System.Drawing.Point[]]$points,
    [int]$baseWidth,
    [int]$tipWidth = 1,
    [switch]$Bone
) {
    if ($points.Count -lt 2) { return }
    $outlineBrush = [System.Drawing.SolidBrush]::new($p[0])
    $bodyBrush = [System.Drawing.SolidBrush]::new($(if ($Bone) { $p[5] } else { $p[3] }))
    $shadowBrush = [System.Drawing.SolidBrush]::new($(if ($Bone) { $p[4] } else { $p[2] }))
    $lightBrush = [System.Drawing.SolidBrush]::new($(if ($Bone) { $p[6] } else { $p[4] }))
    $grainPen = [System.Drawing.Pen]::new($(if ($Bone) { $p[6] } else { $p[4] }), 1)
    try {
        foreach ($pass in @(
            @{ Brush = $outlineBrush; Extra = 4 },
            @{ Brush = $bodyBrush; Extra = 0 }
        )) {
            for ($i = 0; $i -lt $points.Count - 1; $i++) {
                $a = $points[$i]; $b = $points[$i + 1]
                $dx = [double]($b.X - $a.X); $dy = [double]($b.Y - $a.Y)
                $length = [Math]::Max(1.0, [Math]::Sqrt($dx * $dx + $dy * $dy))
                $progressA = $i / [double]($points.Count - 1)
                $progressB = ($i + 1) / [double]($points.Count - 1)
                $widthA = [Math]::Max(1, [int][Math]::Round($baseWidth + ($tipWidth - $baseWidth) * $progressA) + $pass.Extra)
                $widthB = [Math]::Max(1, [int][Math]::Round($baseWidth + ($tipWidth - $baseWidth) * $progressB) + $pass.Extra)
                $px = -$dy / $length; $py = $dx / $length
                $quad = [System.Drawing.Point[]]@(
                    [System.Drawing.Point]::new([int][Math]::Round($a.X + $px * $widthA / 2), [int][Math]::Round($a.Y + $py * $widthA / 2)),
                    [System.Drawing.Point]::new([int][Math]::Round($a.X - $px * $widthA / 2), [int][Math]::Round($a.Y - $py * $widthA / 2)),
                    [System.Drawing.Point]::new([int][Math]::Round($b.X - $px * $widthB / 2), [int][Math]::Round($b.Y - $py * $widthB / 2)),
                    [System.Drawing.Point]::new([int][Math]::Round($b.X + $px * $widthB / 2), [int][Math]::Round($b.Y + $py * $widthB / 2))
                )
                $g.FillPolygon($pass.Brush, $quad)
                $joint = [Math]::Max(1, $widthB)
                $g.FillEllipse($pass.Brush, $b.X - [int]($joint / 2), $b.Y - [int]($joint / 2), $joint, $joint)
            }
        }

        # A broken, off-centre grain line gives the large limbs the pale split-bark
        # character of the reference without turning them into smooth vector tubes.
        for ($i = 0; $i -lt $points.Count - 1; $i++) {
            $a = $points[$i]; $b = $points[$i + 1]
            $offset = if ($i % 2 -eq 0) { 2 } else { 1 }
            $g.DrawLine($grainPen, $a.X + $offset, $a.Y, $b.X + $offset, $b.Y)
        }

        # Broken bark bands follow the mass instead of tracing a ruler-straight centerline.
        for ($i = 1; $i -lt $points.Count - 1; $i++) {
            $point = $points[$i]
            $g.FillRectangle($(if ($i % 2) { $shadowBrush } else { $lightBrush }), $point.X - 1, $point.Y - 1, 3, 2)
        }
    }
    finally {
        $outlineBrush.Dispose(); $bodyBrush.Dispose(); $shadowBrush.Dispose(); $lightBrush.Dispose(); $grainPen.Dispose()
    }
}

function Add-Knot([System.Drawing.Graphics]$g, [int]$x, [int]$y, [int]$size = 5) {
    $outline = [System.Drawing.SolidBrush]::new($p[0]); $hollow = [System.Drawing.SolidBrush]::new($p[1])
    try {
        $g.FillEllipse($outline, $x - [int]($size / 2), $y - [int]($size / 2), $size, $size)
        $g.FillRectangle($hollow, $x - 1, $y - 1, 2, 2)
    } finally { $outline.Dispose(); $hollow.Dispose() }
}

function Add-AmberStrand([System.Drawing.Graphics]$g, [int]$x, [int]$y, [int]$length) {
    $pen = [System.Drawing.Pen]::new($p[7], 2)
    $tip = [System.Drawing.SolidBrush]::new($p[8])
    try {
        $g.DrawLine($pen, $x, $y, $x, $y + $length)
        $g.FillRectangle($tip, $x, $y + $length, 2, 2)
    } finally { $pen.Dispose(); $tip.Dispose() }
}

function New-TrunkSheet {
    $path = Join-Path $Root 'Content/Tiles/DeadForestTree.png'
    $temporaryPath = Join-Path $Root 'Content/Tiles/DeadForestTree.generated.png'
    $referencePath = Join-Path $CaptureRoot 'Vanilla-ForestTree-Trunk.png'
    if (-not (Test-Path -LiteralPath $referencePath)) {
        throw "Missing renderer-exported native tree atlas: $referencePath"
    }
    $reference = [System.Drawing.Bitmap]::new($referencePath)
    $out = [System.Drawing.Bitmap]::new(176, 264, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        if ($reference.Width -lt $out.Width -or $reference.Height -ne $out.Height) {
            throw "Native tree atlas is $($reference.Width)x$($reference.Height); expected at least 176x264."
        }
        for ($y = 0; $y -lt $out.Height; $y++) {
            for ($x = 0; $x -lt $out.Width; $x++) {
                $sourceX = $x
                $source = $reference.GetPixel($sourceX, $y)
                $frameX = [int]($x / 22); $frameY = [int]($y / 22)
                $localX = $x % 22; $localY = $y % 22
                if ($source.A -eq 0) {
                    # Thicken only inside the current 22px native frame. The old atlas
                    # inherited a very thin vanilla trunk, which made the broad gnarled
                    # crown look like it was balanced on a pole. Native segmentation is
                    # retained, so any struck trunk tile still chops normally.
                    foreach ($delta in @(-1, 1, -2, 2)) {
                        $candidateLocalX = $localX + $delta
                        if ($candidateLocalX -lt 1 -or $candidateLocalX -gt 20) { continue }
                        $candidateX = $frameX * 22 + $candidateLocalX
                        $candidate = $reference.GetPixel($candidateX, $y)
                        if ($candidate.A -gt 0) {
                            $sourceX = $candidateX
                            $source = $candidate
                            break
                        }
                    }
                }
                if ($source.A -eq 0) { continue }
                $hash = [Math]::Abs((($x + 7) * 73856093) -bxor (($y + 11) * 19349663))
                $luminance = (0.299 * $source.R) + (0.587 * $source.G) + (0.114 * $source.B)
                $color = if ($luminance -lt 35) { $p[0] } elseif ($luminance -lt 62) { $p[1] } elseif ($luminance -lt 88) { $p[2] } elseif ($luminance -lt 116) { $p[3] } elseif ($luminance -lt 150) { $p[4] } else { $p[5] }

                # A few frames carry pale exposed wood or an amber graft seam.
                # They follow existing opaque bark pixels, so tile seams remain native-clean.
                if ((($frameX + $frameY * 3) % 11) -eq 0 -and $localX -ge 8 -and $localX -le 10 -and $localY -ge 5 -and $localY -le 13 -and $luminance -gt 82) {
                    $color = $p[5 + (($localY + $frameX) % 2)]
                }
                if ((($frameX * 5 + $frameY) % 17) -eq 0 -and $localX -ge 11 -and $localX -le 12 -and $localY -ge 9 -and $localY -le 13 -and $hash % 3 -ne 0) {
                    $color = $p[7 + ($localY % 2)]
                }
                $out.SetPixel($x, $y, $color)
            }
        }
        $out.Save($temporaryPath, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $reference.Dispose(); $out.Dispose() }
    Move-Item -LiteralPath $temporaryPath -Destination $path -Force
}

function New-TreeTops {
    $canvas = New-Canvas 246 82
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 82; $center = $left + 41
        $lean = @(-4, 1, 5)[$variant]

        # The crown is a dense continuation of the trunk, not a detached antler.
        # Its broad forks and pale scars echo the approved grove reference while
        # still fitting Terraria's three native 82x82 crown variants.
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-7,81),[System.Drawing.Point]::new($center-6,67),[System.Drawing.Point]::new($center+$lean-6,51),[System.Drawing.Point]::new($center+$lean-2,31),[System.Drawing.Point]::new($center+$lean+2,7)) 22 5
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+7,81),[System.Drawing.Point]::new($center+8,67),[System.Drawing.Point]::new($center+$lean+10,51),[System.Drawing.Point]::new($center+$lean+13,35)) 13 4

        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean-4,58),[System.Drawing.Point]::new($center-14,51),[System.Drawing.Point]::new($center-27,39),[System.Drawing.Point]::new($center-35,24),[System.Drawing.Point]::new($center-33,7)) 12 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-24,41),[System.Drawing.Point]::new($center-37,46),[System.Drawing.Point]::new($center-40,37)) 7 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-18,49),[System.Drawing.Point]::new($center-22,32),[System.Drawing.Point]::new($center-18,18)) 7 2

        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean+4,55),[System.Drawing.Point]::new($center+17,49),[System.Drawing.Point]::new($center+29,36),[System.Drawing.Point]::new($center+36,20),[System.Drawing.Point]::new($center+35,6)) 12 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+20,46),[System.Drawing.Point]::new($center+36,49),[System.Drawing.Point]::new($center+40,39)) 7 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+16,50),[System.Drawing.Point]::new($center+22,32),[System.Drawing.Point]::new($center+20,17)) 7 2

        if ($variant -eq 0) { Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean-1,33),[System.Drawing.Point]::new($center-7,19),[System.Drawing.Point]::new($center-5,4)) 8 3 -Bone }
        if ($variant -eq 1) { Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-17,48),[System.Drawing.Point]::new($center-27,31),[System.Drawing.Point]::new($center-25,15)) 7 2 -Bone }
        if ($variant -eq 2) { Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+15,47),[System.Drawing.Point]::new($center+25,30),[System.Drawing.Point]::new($center+24,13)) 7 2 -Bone }
        Add-Knot $g ($center+$lean-4) 52 8
        Add-AmberStrand $g ($center - 32) (31 + $variant) (7 + $variant * 2)
        Add-AmberStrand $g ($center + 31) (28 + $variant) (10 - $variant)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Tops.png')
}

function New-TreeBranches {
    $canvas = New-Canvas 84 126
    $g = $canvas.Graphics
    for ($row = 0; $row -lt 3; $row++) {
        $top = $row * 42
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(41,$top+40),[System.Drawing.Point]::new(31,$top+33),[System.Drawing.Point]::new(19,$top+23),[System.Drawing.Point]::new(9,$top+7+$row)) 11 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(28,$top+31),[System.Drawing.Point]::new(14,$top+35),[System.Drawing.Point]::new(5,$top+28)) 7 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(22,$top+25),[System.Drawing.Point]::new(17,$top+14),[System.Drawing.Point]::new(19,$top+5)) 6 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(43,$top+40),[System.Drawing.Point]::new(54,$top+33),[System.Drawing.Point]::new(66,$top+23),[System.Drawing.Point]::new(75,$top+7+$row)) 11 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(57,$top+31),[System.Drawing.Point]::new(71,$top+35),[System.Drawing.Point]::new(79,$top+28)) 7 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(63,$top+25),[System.Drawing.Point]::new(68,$top+14),[System.Drawing.Point]::new(66,$top+5)) 6 1
        if ($row -eq 1) { Draw-TaperedLimb $g @([System.Drawing.Point]::new(57,$top+31),[System.Drawing.Point]::new(68,$top+17)) 6 2 -Bone }
        Add-Knot $g 28 ($top+30) 4
        Add-Knot $g 56 ($top+30) 4
        Add-AmberStrand $g (11 + $row * 2) ($top + 17) (5 + $row)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Branches.png')
}

function New-TreeRoots {
    $canvas = New-Canvas 144 32
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 48
        $center = $left + 24 + @(-2, 1, 3)[$variant]
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center,31),[System.Drawing.Point]::new($center-1,20),[System.Drawing.Point]::new($center+1,8),[System.Drawing.Point]::new($center,0)) 23 12
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-2,18),[System.Drawing.Point]::new($left+13,25),[System.Drawing.Point]::new($left+4,30)) 11 3
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+2,19),[System.Drawing.Point]::new($left+35,25),[System.Drawing.Point]::new($left+45,30)) 11 3
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-4,22),[System.Drawing.Point]::new($left+18,30)) 7 2 -Bone:($variant -eq 1)
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+4,22),[System.Drawing.Point]::new($left+31,31)) 7 2 -Bone:($variant -eq 2)
        if ($variant -eq 0) { Add-Knot $g ($center+2) 14 6 }
        Add-AmberStrand $g ($left + 39 - $variant * 4) (21 - $variant) (5 + $variant)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTreeRoots.png')
}

function New-DeadTufts {
    $canvas = New-Canvas 54 18
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 18; $base = $left + 8
        Draw-Path $g @([System.Drawing.Point]::new($base,16),[System.Drawing.Point]::new($base-1,9),[System.Drawing.Point]::new($base-5,4+$variant)) 1
        Draw-Path $g @([System.Drawing.Point]::new($base,16),[System.Drawing.Point]::new($base+2,10),[System.Drawing.Point]::new($base+6,6-$variant)) 1
        Draw-Path $g @([System.Drawing.Point]::new($base-3,16),[System.Drawing.Point]::new($base-6,11),[System.Drawing.Point]::new($base-7,8)) 1
        if ($variant -eq 2) { Add-AmberStrand $g ($base+5) 7 4 }
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadTuft.png')
}

function New-RootWall {
    $canvas = New-Canvas 32 32
    $g = $canvas.Graphics
    $soil = [System.Drawing.SolidBrush]::new($p[2]); $dark = [System.Drawing.SolidBrush]::new($p[1])
    try {
        $g.FillRectangle($soil,0,0,32,32)
        for ($y=2;$y -lt 32;$y+=7) { $g.FillRectangle($dark, (($y*3)%11), $y, 8, 2) }
        Draw-Path $g @([System.Drawing.Point]::new(1,3),[System.Drawing.Point]::new(10,10),[System.Drawing.Point]::new(6,20),[System.Drawing.Point]::new(15,31)) 2
        Draw-Path $g @([System.Drawing.Point]::new(31,2),[System.Drawing.Point]::new(20,11),[System.Drawing.Point]::new(25,20),[System.Drawing.Point]::new(17,31)) 2
        Draw-Path $g @([System.Drawing.Point]::new(4,31),[System.Drawing.Point]::new(13,22),[System.Drawing.Point]::new(21,17),[System.Drawing.Point]::new(29,10)) 1
        Add-AmberStrand $g 13 12 5
    } finally { $soil.Dispose(); $dark.Dispose() }
    Save-Canvas $canvas (Join-Path $Root 'Content/Walls/DeadFlowerWallUnsafe.png')
}

New-TrunkSheet
New-TreeTops
New-TreeBranches
New-TreeRoots
Write-Host 'Generated native-scale Wastes tree trunks, crowns, and branches.'
