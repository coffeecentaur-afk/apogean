param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }

$palettes = @{
    Kessler = @((C '#101214'), (C '#25282b'), (C '#414549'), (C '#665d54'), (C '#8e3f25'), (C '#df6227'), (C '#ff3f25'), (C '#f2a64b'), (C '#d7d1c2'))
    Helix = @((C '#151b1b'), (C '#303a38'), (C '#65736d'), (C '#a8b4aa'), (C '#e1e5dc'), (C '#3d754f'), (C '#68d277'), (C '#d59a31'), (C '#f2d37a'))
    Sentrix = @((C '#060d14'), (C '#102331'), (C '#1c3a4c'), (C '#32647a'), (C '#299ac2'), (C '#49c8ee'), (C '#a5ebf7'), (C '#d5f8ff'), (C '#d38a2d'))
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

function Save-Canvas($canvas, [string]$path) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
    $canvas.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $canvas.Graphics.Dispose()
    $canvas.Bitmap.Dispose()
}

function Brush([System.Drawing.Color]$color) { [System.Drawing.SolidBrush]::new($color) }

function Fill([System.Drawing.Graphics]$g, [System.Drawing.Color]$color, [int]$x, [int]$y, [int]$w, [int]$h) {
    $brush = Brush $color
    try { $g.FillRectangle($brush, $x, $y, $w, $h) } finally { $brush.Dispose() }
}

function Copy-LogicalSprite {
    param(
        [System.Drawing.Bitmap]$Logical,
        [System.Drawing.Bitmap]$Sheet,
        [int]$TileWidth,
        [int]$TileHeight,
        [int]$DestinationX = 0,
        [int]$DestinationY = 0
    )
    for ($y = 0; $y -lt $TileHeight * 16; $y++) {
        for ($x = 0; $x -lt $TileWidth * 16; $x++) {
            $color = $Logical.GetPixel($x, $y)
            $sheetX = $DestinationX + [int][Math]::Floor($x / 16.0) * 18 + ($x % 16)
            $sheetY = $DestinationY + [int][Math]::Floor($y / 16.0) * 18 + ($y % 16)
            if ($sheetX -lt $Sheet.Width -and $sheetY -lt $Sheet.Height) { $Sheet.SetPixel($sheetX, $sheetY, $color) }
        }
    }
}

function New-Wall([string]$kind, [string]$name, [bool]$window) {
    $p = $palettes[$kind]
    $canvas = New-Canvas 32 32
    $g = $canvas.Graphics
    if ($window) {
        # One broad two-tile window bay. The previous asset outlined every 16px
        # cell, which tiled into the bright graph paper seen in the failed campus.
        Fill $g $p[0] 0 0 32 32
        Fill $g $p[1] 2 2 28 28
        Fill $g $p[2] 3 3 26 26
        Fill $g $p[3] 4 4 24 2
        Fill $g $p[0] 15 2 2 28
        Fill $g $p[1] 3 24 26 4
        Fill $g $p[5] 5 9 8 1
        Fill $g $p[5] 19 18 7 1
    } else {
        # Matte armour sheet with sparse two-tile panel rhythm. It deliberately
        # has no outline around each individual wall tile.
        Fill $g $p[1] 0 0 32 32
        Fill $g $p[2] 1 1 30 30
        Fill $g $p[1] 1 14 30 3
        Fill $g $p[0] 7 14 2 3
        Fill $g $p[0] 23 14 2 3
        Fill $g $p[3] 3 3 11 1
        Fill $g $p[3] 18 18 10 1
        Fill $g $p[4] 27 5 2 2
        Fill $g $p[4] 5 25 2 2
    }
    Save-Canvas $canvas (Join-Path $Root "Content/Walls/$name.png")
}

function New-Platform([string]$kind) {
    $p = $palettes[$kind]
    $canvas = New-Canvas 486 18
    for ($frame = 0; $frame -lt 27; $frame++) {
        $x = $frame * 18
        Fill $canvas.Graphics $p[0] ($x + 1) 6 16 9
        Fill $canvas.Graphics $p[3] ($x + 2) 6 14 1
        Fill $canvas.Graphics $p[2] ($x + 2) 7 14 5
        Fill $canvas.Graphics $p[1] ($x + 2) 12 14 3
        if ($frame % 3 -eq 0) { Fill $canvas.Graphics $p[5] ($x + 4) 8 3 1 }
        if ($frame % 5 -eq 0) { Fill $canvas.Graphics $p[4] ($x + 12) 9 2 2 }
    }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Platform.png")
}

function New-Chair([string]$kind) {
    $p = $palettes[$kind]
    $sheet = [System.Drawing.Bitmap]::new(36, 40, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($style = 0; $style -lt 2; $style++) {
        $logical = New-Canvas 16 32
        $g = $logical.Graphics
        $mirror = $style -eq 1
        $backX = if ($mirror) { 11 } else { 3 }
        $seatX = if ($mirror) { 3 } else { 5 }
        Fill $g $p[0] ($backX - 1) 3 4 22
        Fill $g $p[2] $backX 4 2 20
        Fill $g $p[0] ($seatX - 1) 18 9 5
        Fill $g $p[3] $seatX 19 7 2
        Fill $g $p[0] $seatX 22 2 9
        Fill $g $p[0] ($seatX + 6) 22 2 9
        Fill $g $p[4] ($backX + ($mirror ? -1 : 2)) 8 1 4
        Copy-LogicalSprite $logical.Bitmap $sheet 1 2 ($style * 18) 0
        $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    }
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Chair.png")
}

function New-Table([string]$kind) {
    $p = $palettes[$kind]
    $logical = New-Canvas 48 32
    $g = $logical.Graphics
    Fill $g $p[0] 1 15 46 6
    Fill $g $p[3] 2 15 44 2
    Fill $g $p[2] 3 17 42 2
    Fill $g $p[0] 5 20 4 12
    Fill $g $p[0] 39 20 4 12
    Fill $g $p[2] 6 20 2 10
    Fill $g $p[2] 40 20 2 10
    Fill $g $p[4] 15 17 5 1
    Fill $g $p[5] 30 17 3 1
    $sheet = [System.Drawing.Bitmap]::new(54, 36, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    Copy-LogicalSprite $logical.Bitmap $sheet 3 2
    $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Table.png")
}

function New-Workbench([string]$kind) {
    $p = $palettes[$kind]
    $logical = New-Canvas 32 16
    $g = $logical.Graphics
    Fill $g $p[0] 1 5 30 10
    Fill $g $p[3] 2 5 28 2
    Fill $g $p[2] 3 7 26 5
    Fill $g $p[0] 4 12 4 4
    Fill $g $p[0] 24 12 4 4
    Fill $g $p[4] 6 8 6 2
    Fill $g $p[5] 18 8 8 1
    $sheet = [System.Drawing.Bitmap]::new(36, 20, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    Copy-LogicalSprite $logical.Bitmap $sheet 2 1
    $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Workbench.png")
}

function New-Light([string]$kind) {
    $p = $palettes[$kind]
    $canvas = New-Canvas 18 18
    Fill $canvas.Graphics $p[0] 4 4 10 10
    Fill $canvas.Graphics $p[2] 5 5 8 8
    Fill $canvas.Graphics $p[5] 6 6 6 6
    Fill $canvas.Graphics $p[7] 8 7 2 4
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Light.png")
}

function New-Console([string]$kind) {
    $p = $palettes[$kind]
    $logical = New-Canvas 48 32
    $g = $logical.Graphics
    Fill $g $p[0] 1 4 46 28
    Fill $g $p[2] 3 6 42 10
    Fill $g $p[1] 5 8 38 6
    Fill $g $p[5] 7 9 9 2
    Fill $g $p[6] 19 9 4 2
    Fill $g $p[4] 28 9 12 1
    Fill $g $p[2] 5 18 38 8
    for ($x = 7; $x -lt 39; $x += 7) { Fill $g $p[5] $x 20 3 2 }
    Fill $g $p[0] 5 26 5 6
    Fill $g $p[0] 38 26 5 6
    $sheet = [System.Drawing.Bitmap]::new(54, 36, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    Copy-LogicalSprite $logical.Bitmap $sheet 3 2
    $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Console.png")
}

function New-Locker([string]$kind) {
    $p = $palettes[$kind]
    $logical = New-Canvas 32 48
    $g = $logical.Graphics
    Fill $g $p[0] 1 1 30 47
    Fill $g $p[2] 3 3 26 43
    Fill $g $p[1] 5 5 10 39
    Fill $g $p[1] 17 5 10 39
    Fill $g $p[3] 6 6 8 1
    Fill $g $p[3] 18 6 8 1
    for ($y = 10; $y -lt 18; $y += 3) { Fill $g $p[0] 8 $y 4 1; Fill $g $p[0] 20 $y 4 1 }
    Fill $g $p[5] 12 24 2 3
    Fill $g $p[5] 18 24 2 3
    Fill $g $p[4] 5 39 10 3
    Fill $g $p[4] 17 39 10 3
    $sheet = [System.Drawing.Bitmap]::new(36, 54, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    Copy-LogicalSprite $logical.Bitmap $sheet 2 3
    $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$($kind)Locker.png")
}

function Draw-Signature([string]$kind, [System.Drawing.Graphics]$g, [int]$frame) {
    $p = $palettes[$kind]
    Fill $g $p[0] 2 1 44 63
    Fill $g $p[2] 5 3 38 58
    Fill $g $p[1] 8 7 32 50
    if ($kind -eq 'Kessler') {
        Fill $g $p[3] 18 10 12 9; Fill $g $p[0] 20 12 8 5
        Fill $g $p[3] 14 21 20 19; Fill $g $p[0] 18 23 12 14
        Fill $g $p[4] 10 22 5 26; Fill $g $p[4] 33 22 5 26
        Fill $g $p[3] 15 40 7 16; Fill $g $p[3] 26 40 7 16
        Fill $g $p[6] (8 + $frame * 8) 59 5 2
    } elseif ($kind -eq 'Helix') {
        Fill $g $p[4] 7 3 34 7; Fill $g $p[4] 7 54 34 7
        Fill $g $p[5] 12 11 24 42
        $body = 22 + @(-2,0,2,0)[$frame]
        Fill $g $p[0] $body 17 5 24
        Fill $g $p[0] ($body - 6) (26 + $frame) 8 3
        Fill $g $p[0] ($body + 3) (33 - $frame) 8 3
        Fill $g $p[8] 17 56 4 2; Fill $g $p[6] (27 + $frame) 56 4 2
    } else {
        Fill $g $p[0] 8 48 32 13; Fill $g $p[2] 12 43 24 14
        Fill $g $p[0] 20 35 8 10
        $radius = 8 + ($frame % 2) * 2
        Fill $g $p[5] (24 - $radius) 21 ($radius * 2) 2
        Fill $g $p[5] (24 - $radius) (19 + $radius * 2) ($radius * 2) 2
        Fill $g $p[5] (24 - $radius) 23 2 ($radius * 2 - 4)
        Fill $g $p[5] (22 + $radius) 23 2 ($radius * 2 - 4)
        Fill $g $p[7] (21 + $frame) 27 6 6
    }
}

function New-Signature([string]$kind, [string]$name) {
    $sheet = [System.Drawing.Bitmap]::new(54, 288, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($frame = 0; $frame -lt 4; $frame++) {
        $logical = New-Canvas 48 64
        Draw-Signature $kind $logical.Graphics $frame
        Copy-LogicalSprite $logical.Bitmap $sheet 3 4 0 ($frame * 72)
        $logical.Graphics.Dispose(); $logical.Bitmap.Dispose()
    }
    $canvas = @{ Bitmap = $sheet; Graphics = [System.Drawing.Graphics]::FromImage($sheet) }
    Save-Canvas $canvas (Join-Path $Root "Content/Tiles/$name.png")
}

foreach ($kind in @('Kessler', 'Helix', 'Sentrix')) {
    New-Platform $kind
    New-Chair $kind
    New-Table $kind
    New-Workbench $kind
    New-Light $kind
    New-Console $kind
    New-Locker $kind
}

# Corporate walls are generated by New-NativeWorldTiles.ps1 against Terraria's
# complete 468x180 wall-frame mask. Do not emit the obsolete 32x32 prototypes
# here; doing so silently replaces a valid atlas with a non-native texture.

New-Signature 'Kessler' 'KesslerPowerArmorRack'
New-Signature 'Helix' 'HelixSymbioteTank'
New-Signature 'Sentrix' 'SentrixHologramCore'

Write-Host 'Generated complete native-scale corporate Campus tile families.'
