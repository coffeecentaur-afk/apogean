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

        # Broken bark bands follow the mass instead of tracing a ruler-straight centerline.
        for ($i = 1; $i -lt $points.Count - 1; $i++) {
            $point = $points[$i]
            $g.FillRectangle($(if ($i % 2) { $shadowBrush } else { $lightBrush }), $point.X - 1, $point.Y - 1, 3, 2)
        }
    }
    finally {
        $outlineBrush.Dispose(); $bodyBrush.Dispose(); $shadowBrush.Dispose(); $lightBrush.Dispose()
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
                $source = $reference.GetPixel($x, $y)
                if ($source.A -eq 0) { continue }
                $frameX = [int]($x / 22); $frameY = [int]($y / 22)
                $localX = $x % 22; $localY = $y % 22
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
        $lean = @(-5, 1, 5)[$variant]
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-6,81),[System.Drawing.Point]::new($center-3,66),[System.Drawing.Point]::new($center+$lean-5,49),[System.Drawing.Point]::new($center+$lean,30),[System.Drawing.Point]::new($center+$lean+5,7)) 15 4
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+5,81),[System.Drawing.Point]::new($center+2,68),[System.Drawing.Point]::new($center+$lean+5,51)) 10 4
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean-3,55),[System.Drawing.Point]::new($center-15,48),[System.Drawing.Point]::new($center-27,35),[System.Drawing.Point]::new($center-34,17),[System.Drawing.Point]::new($center-32,7)) 9 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-23,39),[System.Drawing.Point]::new($center-37,42),[System.Drawing.Point]::new($center-40,35)) 5 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean+1,46),[System.Drawing.Point]::new($center+17,40),[System.Drawing.Point]::new($center+29,27),[System.Drawing.Point]::new($center+35,10)) 8 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+17,40),[System.Drawing.Point]::new($center+34,45),[System.Drawing.Point]::new($center+40,38)) 5 1
        if ($variant -eq 1) { Draw-TaperedLimb $g @([System.Drawing.Point]::new($center+$lean,31),[System.Drawing.Point]::new($center-7,18),[System.Drawing.Point]::new($center-13,5)) 6 2 -Bone }
        if ($variant -eq 2) { Draw-TaperedLimb $g @([System.Drawing.Point]::new($center-17,47),[System.Drawing.Point]::new($center-25,32),[System.Drawing.Point]::new($center-22,18)) 5 2 -Bone }
        Add-Knot $g ($center+$lean-3) 48 6
        Add-AmberStrand $g ($center - 30) (31 + $variant) (7 + $variant * 2)
        Add-AmberStrand $g ($center + 29) (29 + $variant) (10 - $variant)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Tops.png')
}

function New-TreeBranches {
    $canvas = New-Canvas 84 126
    $g = $canvas.Graphics
    for ($row = 0; $row -lt 3; $row++) {
        $top = $row * 42
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(40,$top+38),[System.Drawing.Point]::new(29,$top+31),[System.Drawing.Point]::new(17,$top+21),[System.Drawing.Point]::new(8,$top+7+$row)) 8 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(27,$top+30),[System.Drawing.Point]::new(13,$top+33),[System.Drawing.Point]::new(6,$top+27)) 5 1
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(44,$top+38),[System.Drawing.Point]::new(55,$top+31),[System.Drawing.Point]::new(67,$top+21),[System.Drawing.Point]::new(76,$top+7+$row)) 8 2
        Draw-TaperedLimb $g @([System.Drawing.Point]::new(57,$top+30),[System.Drawing.Point]::new(71,$top+33),[System.Drawing.Point]::new(78,$top+27)) 5 1
        if ($row -eq 1) { Draw-TaperedLimb $g @([System.Drawing.Point]::new(57,$top+30),[System.Drawing.Point]::new(68,$top+18)) 5 1 -Bone }
        Add-Knot $g 28 ($top+30) 4
        Add-Knot $g 56 ($top+30) 4
        Add-AmberStrand $g (11 + $row * 2) ($top + 17) (5 + $row)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Branches.png')
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
Write-Host 'Generated native-scale Wastes tree trunks, crowns, and branches.'
