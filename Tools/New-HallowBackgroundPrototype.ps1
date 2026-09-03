param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# Hallow V0 is an authored decomposition of the approved crystallized
# radio-astronomy campus. Image generation establishes the silhouettes; this
# deterministic pass removes the baked transparency preview, scales without
# interpolation, and enforces Terraria's exact layer dimensions and palettes.
$layers = @(
    @{
        Name = 'Far'
        CheckerMode = 'Neutral'
        Source = 'Art/Source/Backgrounds/Hallow/V0-Far-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/HallowConceptV0_Far.png'
        ProductionOutput = 'Content/Backgrounds/Hallow/V0_Far.png'
        Width = 1024
        Height = 408
        HorizonTop = 70
        EdgeBlendWidth = 0
        EdgeTopOffset = 24
        FloorTopPaletteIndex = 3
        Palette = @('#596989', '#6f80a2', '#8596b8', '#9cacc8', '#b2c0d6', '#c8d2e2', '#dce3ec', '#eeeef2', '#bfa9df', '#86c8e8')
    },
    @{
        Name = 'Mid'
        CheckerMode = 'Neutral'
        Source = 'Art/Source/Backgrounds/Hallow/V0-Mid-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/HallowConceptV0_Mid.png'
        ProductionOutput = 'Content/Backgrounds/Hallow/V0_Mid.png'
        Width = 1024
        Height = 600
        HorizonTop = 70
        EdgeBlendWidth = 0
        EdgeTopOffset = 38
        FloorTopPaletteIndex = 2
        Palette = @('#10182c', '#1d2b49', '#304363', '#4a5e7e', '#6f82a1', '#a8b8cd', '#d9e0e9', '#75d9ef', '#b18be8', '#f0b4ef')
    },
    @{
        Name = 'Close'
        CheckerMode = 'BlueGrey'
        Source = 'Art/Source/Backgrounds/Hallow/V0-Close-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/HallowConceptV0_Close.png'
        ProductionOutput = 'Content/Backgrounds/Hallow/V0_Close.png'
        Width = 952
        Height = 480
        HorizonTop = 100
        EdgeBlendWidth = 0
        EdgeTopOffset = 30
        FloorTopPaletteIndex = 2
        Palette = @('#080d1b', '#111a2e', '#1c2a45', '#2c3e60', '#405778', '#617797', '#91a5be', '#66d7e9', '#b27be6', '#f1b1ee')
    }
)

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Test-CheckerPixel([System.Drawing.Color]$color, [string]$mode) {
    $minimum = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
    $maximum = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))

    if ($mode -eq 'BlueGrey') {
        # The close extraction uses a blue-grey checker. Machinery remains
        # darker and crystals have much stronger cyan/magenta chroma.
        $blueOffset = [int]$color.B - [int]$color.R
        $greenOffset = [int]$color.G - [int]$color.R
        return $color.R -ge 135 -and $color.R -le 180 -and
            $blueOffset -ge 8 -and $blueOffset -le 30 -and
            $greenOffset -ge 1 -and $greenOffset -le 16
    }

    # Far and mid use bright neutral checker greys. A narrow chroma test keeps
    # pearl architecture and pastel crystal highlights intact.
    return $minimum -ge 185 -and ($maximum - $minimum) -le 8
}

function Find-ArtBounds([System.Drawing.Bitmap]$bitmap, [string]$checkerMode) {
    $left = $bitmap.Width
    $top = $bitmap.Height
    $right = -1
    $bottom = -1

    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            if (Test-CheckerPixel ($bitmap.GetPixel($x, $y)) $checkerMode) { continue }
            $left = [Math]::Min($left, $x)
            $top = [Math]::Min($top, $y)
            $right = [Math]::Max($right, $x)
            $bottom = [Math]::Max($bottom, $y)
        }
    }

    if ($right -lt $left -or $bottom -lt $top) {
        throw 'The Hallow extraction contains no artwork after checkerboard removal.'
    }

    [System.Drawing.Rectangle]::FromLTRB($left, $top, $right + 1, $bottom + 1)
}

function Find-NearestPaletteColor([System.Drawing.Color]$source, [System.Drawing.Color[]]$palette) {
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
    $best
}

function Convert-Layer($specification) {
    $sourcePath = Join-Path $Root $specification.Source
    $outputPath = Join-Path $Root $specification.Output
    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new(
        [int]$specification.Width,
        [int]$specification.Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $palette = [System.Drawing.Color[]]@($specification.Palette | ForEach-Object { Convert-HexColor $_ })
    $colorCache = [System.Collections.Generic.Dictionary[int, System.Drawing.Color]]::new()

    try {
        $checkerMode = [string]$specification.CheckerMode
        $bounds = Find-ArtBounds $source $checkerMode
        $availableHeight = $output.Height - [int]$specification.HorizonTop - 20
        $scale = [Math]::Min(
            [double]$output.Width / [double]$bounds.Width,
            [double]$availableHeight / [double]$bounds.Height)
        $scaledWidth = [Math]::Max(1, [int][Math]::Floor($bounds.Width * $scale))
        $scaledHeight = [Math]::Max(1, [int][Math]::Floor($bounds.Height * $scale))
        $offsetX = [int][Math]::Floor(($output.Width - $scaledWidth) / 2.0)
        $offsetY = [int]$specification.HorizonTop

        for ($y = 0; $y -lt $output.Height; $y++) {
            for ($x = 0; $x -lt $output.Width; $x++) {
                $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
            }
        }

        for ($destinationY = 0; $destinationY -lt $scaledHeight; $destinationY++) {
            $sourceY = $bounds.Top + [Math]::Min($bounds.Height - 1, [int][Math]::Floor($destinationY / $scale))
            for ($destinationX = 0; $destinationX -lt $scaledWidth; $destinationX++) {
                $sourceX = $bounds.Left + [Math]::Min($bounds.Width - 1, [int][Math]::Floor($destinationX / $scale))
                $pixel = $source.GetPixel($sourceX, $sourceY)
                if (Test-CheckerPixel $pixel $checkerMode) { continue }
                $key = $pixel.ToArgb()
                $mapped = [System.Drawing.Color]::Empty
                if (-not $colorCache.TryGetValue($key, [ref]$mapped)) {
                    $mapped = Find-NearestPaletteColor $pixel $palette
                    $colorCache[$key] = $mapped
                }
                $output.SetPixel($offsetX + $destinationX, $offsetY + $destinationY,
                    [System.Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
            }
        }

        # An irregular, palette-stepped corrupted shelf closes camera-height
        # gaps without leaving a flat safety rectangle behind the terrain.
        $floorStart = [Math]::Min($output.Height - 12, $offsetY + $scaledHeight)
        $floorTopPaletteIndex = [int]$specification.FloorTopPaletteIndex
        for ($x = 0; $x -lt $output.Width; $x++) {
            $jitter = (($x * 17 + [int]($x / 13) * 7) % 11) - 5
            $columnTop = [Math]::Max(0, [Math]::Min($output.Height - 12, $floorStart + $jitter))
            $depth = [Math]::Max(1, $output.Height - $columnTop)
            for ($y = $columnTop; $y -lt $output.Height; $y++) {
                $progress = [double]($y - $columnTop) / [double]$depth
                $index = [Math]::Max(0, [Math]::Min($floorTopPaletteIndex,
                    $floorTopPaletteIndex - [int][Math]::Floor($progress * ($floorTopPaletteIndex + 1))))
                if ((($x * 5 + $y * 3) % 23) -eq 0 -and $index -gt 0) { $index-- }
                $floorColor = $palette[$index]
                $output.SetPixel($x, $y,
                    [System.Drawing.Color]::FromArgb(255, $floorColor.R, $floorColor.G, $floorColor.B))
            }
        }

        # Hallow's structures already continue beyond both source edges.
        # EdgeBlendWidth is zero to avoid synthetic dark diagonal slabs; the
        # exact final-column seal below remains the repeat contract.
        $edgeWidth = [int]$specification.EdgeBlendWidth
        $edgeTop = $floorStart - [int]$specification.EdgeTopOffset
        foreach ($side in 0..1) {
            for ($distance = 0; $distance -lt $edgeWidth; $distance++) {
                $x = if ($side -eq 0) { $distance } else { $output.Width - 1 - $distance }
                $originalTop = $floorStart
                for ($y = 0; $y -lt $floorStart; $y++) {
                    if ($output.GetPixel($x, $y).A -gt 0) {
                        $originalTop = $y
                        break
                    }
                }

                $amount = [double]$distance / [double]$edgeWidth
                $targetTop = [int][Math]::Round($edgeTop + ($originalTop - $edgeTop) * $amount)
                for ($y = 0; $y -lt $targetTop; $y++) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                }
                for ($y = $targetTop; $y -lt $floorStart; $y++) {
                    if ($output.GetPixel($x, $y).A -eq 0) {
                        $edgeColor = $palette[([Math]::Abs($x + $y) % 5 -eq 0) ? 1 : 0]
                        $output.SetPixel($x, $y,
                            [System.Drawing.Color]::FromArgb(255, $edgeColor.R, $edgeColor.G, $edgeColor.B))
                    }
                }
            }
        }

        for ($y = 0; $y -lt $output.Height; $y++) {
            $output.SetPixel($output.Width - 1, $y, $output.GetPixel(0, $y))
        }

        New-Item -ItemType Directory -Path (Split-Path -Parent $outputPath) -Force | Out-Null
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)

        $productionPath = Join-Path $Root $specification.ProductionOutput
        New-Item -ItemType Directory -Path (Split-Path -Parent $productionPath) -Force | Out-Null
        $output.Save($productionPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "$($specification.Name): $($output.Width)x$($output.Height), source bounds $bounds, scale $([Math]::Round($scale, 3))"
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

foreach ($layer in $layers) {
    Convert-Layer $layer
}

Write-Host 'Generated the diagnostic and production Hallow V0 parallax set.'
