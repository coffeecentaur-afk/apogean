param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# Ocean V0 is an authored decomposition of the approved drowned industrial
# port. Image generation establishes the silhouettes; this
# deterministic pass removes the baked transparency preview, scales without
# interpolation, and enforces Terraria's exact layer dimensions and palettes.
$layers = @(
    @{
        Name = 'Far'
        CheckerMode = 'Neutral'
        Source = 'Art/Source/Backgrounds/Ocean/V0-Far-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/OceanConceptV0_Far.png'
        ProductionOutput = 'Content/Backgrounds/Ocean/V0_Far.png'
        Width = 1024
        Height = 408
        HorizonTop = 70
        EdgeBlendWidth = 0
        EdgeTopOffset = 24
        FloorTopPaletteIndex = 3
        Palette = @('#344552', '#465b68', '#5c707c', '#738791', '#899ba3', '#a1afb3', '#b8c1c1', '#cbd0cc', '#77634b', '#9a7650')
    },
    @{
        Name = 'Mid'
        CheckerMode = 'Neutral'
        Source = 'Art/Source/Backgrounds/Ocean/V0-Mid-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/OceanConceptV0_Mid.png'
        ProductionOutput = 'Content/Backgrounds/Ocean/V0_Mid.png'
        Width = 1024
        Height = 600
        HorizonTop = -40
        EdgeBlendWidth = 0
        EdgeTopOffset = 38
        FloorTopPaletteIndex = 2
        Palette = @('#111820', '#1d2a33', '#2e404a', '#435863', '#5c7079', '#788991', '#9ca7a8', '#c0c5c0', '#745239', '#a36e45')
    },
    @{
        Name = 'Close'
        CheckerMode = 'Neutral'
        Source = 'Art/Source/Backgrounds/Ocean/V0-Close-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/OceanConceptV0_Close.png'
        ProductionOutput = 'Content/Backgrounds/Ocean/V0_Close.png'
        Width = 952
        Height = 480
        HorizonTop = -30
        EdgeBlendWidth = 0
        EdgeTopOffset = 30
        FloorTopPaletteIndex = 2
        Palette = @('#070b0f', '#101820', '#1a2730', '#293b45', '#3d515a', '#596b71', '#7a898b', '#9aa3a0', '#76513b', '#6d3f65')
    }
)

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Test-CheckerPixel([System.Drawing.Color]$color, [string]$mode) {
    $minimum = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
    $maximum = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))

    # All three sources use a bright neutral transparency preview. The high
    # floor preserves steel highlights and white surf while removing the
    # checker field around the extraction.
    return $minimum -ge 225 -and ($maximum - $minimum) -le 8
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
        throw 'The Ocean extraction contains no artwork after checkerboard removal.'
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
            $outputY = $offsetY + $destinationY
            if ($outputY -lt 0 -or $outputY -ge $output.Height) { continue }
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
                $output.SetPixel($offsetX + $destinationX, $outputY,
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

        # Ocean's water and wreckage already continue beyond both source edges.
        # EdgeBlendWidth remains zero to avoid synthetic diagonal slabs; the
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

Write-Host 'Generated the diagnostic and production Ocean V0 parallax set.'
