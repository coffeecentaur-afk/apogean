param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

$palettes = @{
    Forest = @{ Far = C '#736955'; FarLine = C '#8a7c60'; Mid = C '#50483a'; MidLine = C '#685d46'; Close = C '#2f2b26'; Accent = C '#a17b32' }
    Desert = @{ Far = C '#a37b4c'; FarLine = C '#bc9560'; Mid = C '#765334'; MidLine = C '#916942'; Close = C '#433126'; Accent = C '#d0a04a' }
    Jungle = @{ Far = C '#586448'; FarLine = C '#75805a'; Mid = C '#384333'; MidLine = C '#536044'; Close = C '#202821'; Accent = C '#b28c31' }
    Snow = @{ Far = C '#87939a'; FarLine = C '#b8c1c2'; Mid = C '#5c686e'; MidLine = C '#87969a'; Close = C '#333d43'; Accent = C '#d1b65a' }
    Corruption = @{ Far = C '#655a78'; FarLine = C '#827396'; Mid = C '#443b55'; MidLine = C '#5d506f'; Close = C '#282331'; Accent = C '#b08d42' }
    Crimson = @{ Far = C '#7b5149'; FarLine = C '#9b6b5d'; Mid = C '#58352f'; MidLine = C '#75473d'; Close = C '#312322'; Accent = C '#c69b3f' }
    Hallow = @{ Far = C '#78919b'; FarLine = C '#a8bec2'; Mid = C '#526b75'; MidLine = C '#7794a0'; Close = C '#303f49'; Accent = C '#d2ad57' }
    Ocean = @{ Far = C '#627b86'; FarLine = C '#8ca0a5'; Mid = C '#405c68'; MidLine = C '#66818b'; Close = C '#263943'; Accent = C '#bc913a' }
    Engraft = @{ Far = C '#625b46'; FarLine = C '#837650'; Mid = C '#3d392d'; MidLine = C '#5b5036'; Close = C '#211f1c'; Accent = C '#c2952e' }
}

function New-Canvas([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Fill-Terrain {
    param($graphics, [int]$width, [int]$height, [int]$startY, [int]$amplitude, [System.Drawing.Color]$fill,
        [System.Drawing.Color]$line, [int]$seed, [int]$step = 64)

    $random = [System.Random]::new($seed)
    $points = [System.Collections.Generic.List[System.Drawing.Point]]::new()
    $points.Add([System.Drawing.Point]::new(0, $startY))
    for ($x = $step; $x -lt $width; $x += $step) {
        $y = $startY + $random.Next(-$amplitude, $amplitude + 1)
        $points.Add([System.Drawing.Point]::new($x, $y))
    }
    $points.Add([System.Drawing.Point]::new($width - 1, $startY))
    $ridge = $points.ToArray()
    $points.Add([System.Drawing.Point]::new($width - 1, $height - 1))
    $points.Add([System.Drawing.Point]::new(0, $height - 1))

    $brush = [System.Drawing.SolidBrush]::new($fill)
    $pen = [System.Drawing.Pen]::new($line, 3)
    try {
        $graphics.FillPolygon($brush, $points.ToArray())
        $graphics.DrawLines($pen, $ridge)
        foreach ($offset in @(38, 82)) {
            [System.Drawing.Point[]]$contour = foreach ($point in $ridge) {
                [System.Drawing.Point]::new($point.X, [Math]::Min($height - 3, $point.Y + $offset))
            }
            $graphics.DrawLines($pen, $contour)
        }
    }
    finally {
        $brush.Dispose()
        $pen.Dispose()
    }
}

function Draw-Tower {
    param($graphics, [int]$x, [int]$groundY, [int]$width, [int]$height, [System.Drawing.Color]$body,
        [System.Drawing.Color]$line, [bool]$broken = $true)

    $brush = [System.Drawing.SolidBrush]::new($body)
    $pen = [System.Drawing.Pen]::new($line, 3)
    try {
        $top = $groundY - $height
        $graphics.FillRectangle($brush, $x, $top, $width, $height)
        $graphics.DrawRectangle($pen, $x, $top, $width, $height)
        if ($broken) {
            $graphics.FillRectangle([System.Drawing.Brushes]::Transparent, $x + [int]($width * 0.55), $top, [int]($width * 0.45), 8)
            $graphics.DrawLine($pen, $x + 5, $top, $x + [int]($width * 0.45), $top - 8)
        }
        for ($windowY = $top + 12; $windowY -lt $groundY - 8; $windowY += 16) {
            $graphics.DrawLine($pen, $x + 6, $windowY, $x + $width - 6, $windowY)
        }
    }
    finally {
        $brush.Dispose()
        $pen.Dispose()
    }
}

function Draw-DeadTree {
    param($graphics, [int]$x, [int]$groundY, [int]$height, [System.Drawing.Color]$color)
    $pen = [System.Drawing.Pen]::new($color, 5)
    try {
        $top = $groundY - $height
        $graphics.DrawLine($pen, $x, $groundY, $x + 2, $top)
        $graphics.DrawLine($pen, $x + 1, $top + 18, $x - 15, $top + 5)
        $graphics.DrawLine($pen, $x + 1, $top + 28, $x + 17, $top + 12)
        $graphics.DrawLine($pen, $x - 8, $top + 12, $x - 19, $top + 9)
    }
    finally { $pen.Dispose() }
}

function Draw-Cables {
    param($graphics, [int]$x1, [int]$x2, [int]$y, [System.Drawing.Color]$color, [int]$drop = 28)
    $pen = [System.Drawing.Pen]::new($color, 3)
    try {
        [System.Drawing.Point[]]$points = @(
            [System.Drawing.Point]::new($x1, $y),
            [System.Drawing.Point]::new([int](($x1 * 2 + $x2) / 3), $y + $drop),
            [System.Drawing.Point]::new([int](($x1 + $x2 * 2) / 3), $y + $drop),
            [System.Drawing.Point]::new($x2, $y)
        )
        $graphics.DrawLines($pen, $points)
    }
    finally { $pen.Dispose() }
}

function Draw-BiomeDetails {
    param($graphics, [string]$biome, [string]$layer, [int]$variant, [int]$width, [int]$height, $palette, [int]$groundY)

    $shift = if ($variant -eq 0) { 0 } else { 86 }
    $linePen = [System.Drawing.Pen]::new($palette.MidLine, 4)
    $accentPen = [System.Drawing.Pen]::new($palette.Accent, 3)
    $accentBrush = [System.Drawing.SolidBrush]::new($palette.Accent)
    try {
        if ($layer -eq 'Far') {
            Draw-Tower $graphics (160 + $shift) $groundY 38 110 $palette.Far $palette.FarLine
            Draw-Tower $graphics (610 - [int]($shift / 2)) $groundY 52 150 $palette.Far $palette.FarLine
            $graphics.DrawLine($linePen, 470, $groundY - 45, 470, $groundY - 165)
            $graphics.DrawLine($linePen, 450, $groundY - 145, 492, $groundY - 145)
        }

        switch ($biome) {
            'Forest' {
                if ($layer -ne 'Far') {
                    Draw-DeadTree $graphics (115 + $shift) $groundY 115 $palette.MidLine
                    Draw-DeadTree $graphics (820 - $shift) $groundY 92 $palette.MidLine
                    $graphics.DrawLine($linePen, 290, $groundY - 54, 290, $groundY - 155)
                    $graphics.DrawLine($linePen, 275, $groundY - 135, 305, $groundY - 135)
                    Draw-Cables $graphics 290 515 ($groundY - 135) $palette.MidLine 22
                    $graphics.DrawLine($linePen, 515, $groundY - 135, 515, $groundY - 45)
                }
            }
            'Desert' {
                if ($layer -eq 'Far') {
                    $mesaBrush = [System.Drawing.SolidBrush]::new($palette.FarLine)
                    $graphics.FillPolygon($mesaBrush, [System.Drawing.Point[]]@(
                        [System.Drawing.Point]::new(40, $groundY), [System.Drawing.Point]::new(160, $groundY - 105),
                        [System.Drawing.Point]::new(280, $groundY), [System.Drawing.Point]::new(40, $groundY)))
                    $mesaBrush.Dispose()
                }
                else {
                    $roadY = $groundY - 100
                    $graphics.DrawLine($linePen, 65, $roadY, 900, $roadY)
                    $graphics.DrawLine($linePen, 65, $roadY + 8, 900, $roadY + 8)
                    foreach ($x in @(180, 430, 690, 850)) { $graphics.DrawLine($linePen, $x, $roadY + 8, $x - 8, $groundY) }
                }
            }
            'Jungle' {
                if ($layer -ne 'Far') {
                    $graphics.DrawRectangle($linePen, 330 + [int]($shift / 2), $groundY - 105, 190, 88)
                    $graphics.DrawArc($linePen, 350 + [int]($shift / 2), $groundY - 150, 150, 90, 180, 180)
                    Draw-Cables $graphics 215 620 ($groundY - 165) $palette.MidLine 38
                    foreach ($x in @(130, 245, 700, 840)) { Draw-DeadTree $graphics $x $groundY 125 $palette.MidLine }
                }
            }
            'Snow' {
                if ($layer -ne 'Far') {
                    $graphics.DrawLine($linePen, 360, $groundY - 35, 360, $groundY - 175)
                    $graphics.DrawArc($linePen, 320, $groundY - 190, 80, 45, 180, 180)
                    $graphics.DrawLine($linePen, 560, $groundY - 82, 850, $groundY - 82)
                    foreach ($x in @(610, 790)) { $graphics.DrawLine($linePen, $x, $groundY - 82, $x, $groundY) }
                }
            }
            'Corruption' {
                if ($layer -ne 'Far') {
                    foreach ($x in @(130, 350, 590, 835)) {
                        $graphics.DrawLine($linePen, $x, $groundY, $x + 18, $groundY - 125)
                        $graphics.DrawLine($linePen, $x + 18, $groundY - 125, $x - 4, $groundY - 165)
                    }
                    Draw-Cables $graphics 150 850 ($groundY - 108) $palette.MidLine 55
                }
            }
            'Crimson' {
                if ($layer -ne 'Far') {
                    foreach ($x in @(190, 475, 760)) {
                        $graphics.DrawArc($linePen, $x, $groundY - 160, 95, 145, 180, 180)
                        $graphics.DrawLine($linePen, $x, $groundY - 86, $x + 95, $groundY - 86)
                    }
                    $graphics.DrawLine($accentPen, 240, $groundY - 105, 720, $groundY - 105)
                }
            }
            'Hallow' {
                if ($layer -ne 'Far') {
                    foreach ($x in @(170, 400, 690)) {
                        $graphics.DrawPolygon($linePen, [System.Drawing.Point[]]@(
                            [System.Drawing.Point]::new($x, $groundY), [System.Drawing.Point]::new($x + 30, $groundY - 155),
                            [System.Drawing.Point]::new($x + 62, $groundY), [System.Drawing.Point]::new($x, $groundY)))
                    }
                    Draw-Cables $graphics 200 750 ($groundY - 92) $palette.Accent 24
                }
            }
            'Ocean' {
                $waterY = $groundY - 100
                $graphics.DrawLine($linePen, 0, $waterY, $width, $waterY)
                $graphics.DrawLine($linePen, 0, $waterY + 12, $width, $waterY + 12)
                if ($layer -ne 'Far') {
                    $graphics.DrawLine($linePen, 215, $waterY, 215, $waterY - 135)
                    $graphics.DrawLine($linePen, 215, $waterY - 135, 350, $waterY - 70)
                    $graphics.DrawLine($linePen, 350, $waterY - 70, 350, $waterY)
                    $graphics.DrawPolygon($linePen, [System.Drawing.Point[]]@(
                        [System.Drawing.Point]::new(590, $waterY - 12), [System.Drawing.Point]::new(820, $waterY - 12),
                        [System.Drawing.Point]::new(760, $waterY + 42), [System.Drawing.Point]::new(620, $waterY + 42)))
                }
            }
            'Engraft' {
                if ($layer -ne 'Far') {
                    foreach ($x in @(95, 260, 455, 660, 850)) {
                        $graphics.DrawLine($linePen, $x, $groundY, $x + 15, $groundY - 150)
                        $graphics.DrawLine($accentPen, $x + 15, $groundY - 150, $x + 34, $groundY - 84)
                        foreach ($offset in @(0, 36, 72)) { $graphics.FillEllipse($accentBrush, $x + $offset, $groundY - 68 - $offset, 10, 14) }
                    }
                    Draw-Cables $graphics 110 890 ($groundY - 120) $palette.Accent 48
                }
            }
        }
    }
    finally {
        $linePen.Dispose()
        $accentPen.Dispose()
        $accentBrush.Dispose()
    }
}

function Seal-Edges([System.Drawing.Bitmap]$bitmap) {
    for ($x = 0; $x -lt $bitmap.Width; $x++) {
        $bitmap.SetPixel($x, $bitmap.Height - 1, $bitmap.GetPixel($x, $bitmap.Height - 2))
    }
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        $bitmap.SetPixel($bitmap.Width - 1, $y, $bitmap.GetPixel(0, $y))
    }
}

function Save-Layer([string]$biome, [int]$variant, [string]$layer, [int]$width, [int]$height) {
    $palette = $palettes[$biome]
    $canvas = New-Canvas $width $height
    $bitmap = $canvas.Bitmap
    $graphics = $canvas.Graphics
    try {
        switch ($layer) {
            'Far' {
                $groundY = 190 + $variant * 12
                Fill-Terrain $graphics $width $height $groundY 45 $palette.Far $palette.FarLine (1000 + $variant * 83 + $biome.Length * 17) 64
            }
            'Mid' {
                $groundY = 300 + $variant * 18
                Fill-Terrain $graphics $width $height $groundY 58 $palette.Mid $palette.MidLine (2000 + $variant * 97 + $biome.Length * 23) 72
            }
            'Close' {
                $groundY = 286 + $variant * 16
                Fill-Terrain $graphics $width $height $groundY 46 $palette.Close $palette.MidLine (3000 + $variant * 109 + $biome.Length * 29) 68
            }
        }

        Draw-BiomeDetails $graphics $biome $layer $variant $width $height $palette $groundY
        Seal-Edges $bitmap
        $destination = Join-Path $Root "Content/Backgrounds/$biome/V$($variant)_$layer.png"
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        $bitmap.Save($destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Save-UndergroundLayer([string]$biome, [int]$variant, [int]$index) {
    $palette = $palettes[$biome]
    $height = if ($index -eq 0 -or $index -eq 2) { 16 } else { 96 }
    $bitmap = [System.Drawing.Bitmap]::new(160, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear($palette.Mid)
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $line = [System.Drawing.Pen]::new($palette.MidLine, 2)
        $accent = [System.Drawing.Pen]::new($palette.Accent, 2)
        $dark = [System.Drawing.SolidBrush]::new($palette.Close)
        try {
            if ($height -eq 16) {
                $graphics.FillRectangle($dark, 0, $(if ($index -eq 0) { 10 } else { 0 }), 160, 6)
                for ($x = 4 + $variant * 7; $x -lt 128; $x += 24) {
                    $graphics.DrawLine($line, $x, 2, $x + 12, 13)
                }
            }
            else {
                for ($y = 4; $y -lt $height; $y += 18) {
                    for ($x = (($y / 18 + $variant) % 2) * -10; $x -lt 128; $x += 30) {
                        $graphics.DrawRectangle($line, [int]$x, $y, 26, 14)
                    }
                }

                if ($biome -eq 'Engraft') {
                    foreach ($x in @(14, 49, 83, 116)) {
                        $graphics.DrawLine($accent, $x, 0, $x + 9, 95)
                        $graphics.FillEllipse([System.Drawing.SolidBrush]::new($palette.Accent), $x + 4, 30 + (($x + $variant * 11) % 42), 5, 7)
                    }
                }
                elseif ($biome -in @('Forest', 'Desert', 'Snow', 'Ocean')) {
                    $graphics.DrawLine($accent, 8, 68, 122, 68)
                    foreach ($x in @(24, 62, 104)) { $graphics.DrawLine($line, $x, 68, $x, 94) }
                }
                else {
                    foreach ($x in @(22, 58, 94)) { $graphics.DrawLine($accent, $x, 12, $x + 18, 84) }
                }
            }
        }
        finally { $line.Dispose(); $accent.Dispose(); $dark.Dispose() }

        # tModLoader's underground contract repeats the leftmost 32 pixels at the right edge.
        for ($y = 0; $y -lt $height; $y++) {
            for ($x = 0; $x -lt 32; $x++) {
                $bitmap.SetPixel(128 + $x, $y, $bitmap.GetPixel($x, $y))
            }
        }

        $destination = Join-Path $Root "Content/Backgrounds/$biome/Underground/V$($variant)_$index.png"
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        $bitmap.Save($destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
}

foreach ($biome in $palettes.Keys) {
    foreach ($variant in 0..1) {
        Save-Layer $biome $variant 'Far' 1024 408
        Save-Layer $biome $variant 'Mid' 1024 600
        Save-Layer $biome $variant 'Close' 952 480
        foreach ($index in 0..3) { Save-UndergroundLayer $biome $variant $index }
    }
}

Write-Host 'Generated two native parallax and underground background sets for all nine biomes.'
