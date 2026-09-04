param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# Ruined Deep V0 uses an opaque far panorama and two hard-alpha overlays.
# Image generation supplies the composition; this pass enforces a small shared
# palette, nearest-neighbor sampling, exact dimensions, and repeat-safe edges.
$width = 1024
$height = 576
$paletteHex = @(
    '#0d0908', '#160d0b', '#21120e', '#2d1812',
    '#3b2017', '#4b281b', '#5e3020', '#743a23',
    '#8a4325', '#a24d26', '#ba5927', '#d16627',
    '#e67728', '#f08b2c', '#f4a13a', '#ffd064'
)
$layers = @(
    @{
        Name = 'Far'
        Source = 'Art/Source/Backgrounds/Underworld/V0-Far-extraction-v1.png'
        Output = 'Content/Backgrounds/Underworld/PanoramaV0_Far.png'
        Opaque = $true
    },
    @{
        Name = 'Mid'
        Source = 'Art/Source/Backgrounds/Underworld/V0-Mid-extraction-v1.png'
        Output = 'Content/Backgrounds/Underworld/PanoramaV0_Mid.png'
        Opaque = $false
    },
    @{
        Name = 'Close'
        Source = 'Art/Source/Backgrounds/Underworld/V0-Close-extraction-v1.png'
        Output = 'Content/Backgrounds/Underworld/PanoramaV0_Close.png'
        Opaque = $false
    }
)

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Test-TransparencyPreview([System.Drawing.Color]$color) {
    if ($color.A -eq 0) { return $true }
    $minimum = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
    $maximum = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
    # Image-generation previews bake a white/very-light-gray checker into the
    # pixels and soften some checker edges below RGB 240. Treat the complete
    # neutral-light family as preview transparency before palette reduction;
    # otherwise those edge pixels quantize into false amber sparks in-game.
    return $minimum -ge 220 -and ($maximum - $minimum) -le 18
}

function Find-ArtBounds([System.Drawing.Bitmap]$bitmap) {
    $left = $bitmap.Width
    $top = $bitmap.Height
    $right = -1
    $bottom = -1
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            if (Test-TransparencyPreview $bitmap.GetPixel($x, $y)) { continue }
            $left = [Math]::Min($left, $x)
            $top = [Math]::Min($top, $y)
            $right = [Math]::Max($right, $x)
            $bottom = [Math]::Max($bottom, $y)
        }
    }
    if ($right -lt $left -or $bottom -lt $top) {
        throw 'The Underworld extraction contains no artwork after transparency removal.'
    }
    [System.Drawing.Rectangle]::FromLTRB($left, $top, $right + 1, $bottom + 1)
}

function Find-NearestPaletteColor(
    [System.Drawing.Color]$source,
    [System.Drawing.Color[]]$palette,
    [System.Collections.Generic.Dictionary[int, System.Drawing.Color]]$cache) {
    $key = $source.ToArgb()
    $cached = [System.Drawing.Color]::Empty
    if ($cache.TryGetValue($key, [ref]$cached)) { return $cached }

    $best = $palette[0]
    $bestDistance = [int64]::MaxValue
    foreach ($candidate in $palette) {
        $red = [int]$source.R - [int]$candidate.R
        $green = [int]$source.G - [int]$candidate.G
        $blue = [int]$source.B - [int]$candidate.B
        $distance = [int64]($red * $red + $green * $green + $blue * $blue)
        if ($distance -lt $bestDistance) {
            $bestDistance = $distance
            $best = $candidate
        }
    }
    $cache[$key] = $best
    $best
}

function Convert-UnderworldLayer($specification) {
    $sourcePath = Join-Path $Root $specification.Source
    $outputPath = Join-Path $Root $specification.Output
    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new(
        $width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $palette = [System.Drawing.Color[]]@($paletteHex | ForEach-Object { Convert-HexColor $_ })
    $cache = [System.Collections.Generic.Dictionary[int, System.Drawing.Color]]::new()

    try {
        $opaque = [bool]$specification.Opaque
        $bounds = if ($opaque) {
            [System.Drawing.Rectangle]::new(0, 0, $source.Width, $source.Height)
        } else {
            Find-ArtBounds $source
        }
        $scaleX = [double]$width / [double]$bounds.Width
        $scaleY = if ($opaque) { [double]$height / [double]$bounds.Height } else { $scaleX }
        $scaledHeight = [Math]::Max(1, [int][Math]::Round($bounds.Height * $scaleY))
        $offsetY = if ($opaque) { 0 } else { $height - $scaledHeight }

        $graphics = [System.Drawing.Graphics]::FromImage($output)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
        }
        finally {
            $graphics.Dispose()
        }

        for ($destinationY = 0; $destinationY -lt $scaledHeight; $destinationY++) {
            $outputY = $offsetY + $destinationY
            if ($outputY -lt 0 -or $outputY -ge $height) { continue }
            $sourceY = $bounds.Top + [Math]::Min(
                $bounds.Height - 1,
                [int][Math]::Floor($destinationY / $scaleY))
            for ($destinationX = 0; $destinationX -lt $width; $destinationX++) {
                $sourceX = $bounds.Left + [Math]::Min(
                    $bounds.Width - 1,
                    [int][Math]::Floor($destinationX / $scaleX))
                $pixel = $source.GetPixel($sourceX, $sourceY)
                if (-not $opaque -and (Test-TransparencyPreview $pixel)) { continue }
                $mapped = Find-NearestPaletteColor $pixel $palette $cache
                $output.SetPixel($destinationX, $outputY,
                    [System.Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
            }
        }

        # Both edges share the same column so horizontal tiling cannot reveal a seam.
        for ($y = 0; $y -lt $height; $y++) {
            if ($opaque -or $output.GetPixel(0, $y).A -gt 0 -or $output.GetPixel($width - 1, $y).A -gt 0) {
                $output.SetPixel($width - 1, $y, $output.GetPixel(0, $y))
            }
        }

        New-Item -ItemType Directory -Path (Split-Path -Parent $outputPath) -Force | Out-Null
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "$($specification.Name): ${width}x${height}, source bounds $bounds"
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

foreach ($layer in $layers) {
    Convert-UnderworldLayer $layer
}

Write-Host 'Generated the Ruined Deep V0 custom-sky panorama set.'
