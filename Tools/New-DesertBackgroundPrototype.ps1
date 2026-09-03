param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

# Desert V0 is an authored decomposition of the approved full-scene reference.
# The extraction files deliberately retain their generated checkerboard so this
# deterministic pass can remove it, quantize it, and satisfy Terraria's exact
# surface-background dimensions without asking the renderer to scale soft art.
$layers = @(
    @{
        Name = 'Far'
        Source = 'Art/Source/Backgrounds/Desert/V0-Far-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/DesertConceptV0_Far.png'
        ProductionOutput = 'Content/Backgrounds/Desert/V0_Far.png'
        Width = 1024
        Height = 408
        HorizonTop = 130
        HorizontalMargin = 0
        EdgeBlendWidth = 120
        EdgeTopOffset = 30
        Palette = @('#8b6749', '#9d7654', '#b18862', '#c79d72', '#d7b083', '#e4c294', '#efd2a7', '#f5dfba')
    },
    @{
        Name = 'Mid'
        Source = 'Art/Source/Backgrounds/Desert/V0-Mid-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/DesertConceptV0_Mid.png'
        ProductionOutput = 'Content/Backgrounds/Desert/V0_Mid.png'
        Width = 1024
        Height = 600
        HorizonTop = 220
        HorizontalMargin = 0
        EdgeBlendWidth = 128
        EdgeTopOffset = 44
        Palette = @('#33231b', '#493024', '#5e3d2a', '#755036', '#8f623e', '#aa7544', '#c28749', '#d99b55', '#746151', '#92806b')
    },
    @{
        Name = 'Close'
        Source = 'Art/Source/Backgrounds/Desert/V0-Close-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/DesertConceptV0_Close.png'
        ProductionOutput = 'Content/Backgrounds/Desert/V0_Close.png'
        Width = 952
        Height = 480
        HorizonTop = 170
        HorizontalMargin = 0
        EdgeBlendWidth = 96
        EdgeTopOffset = 34
        Palette = @('#1b1210', '#2a1811', '#3b2114', '#502b17', '#66371b', '#7d461f', '#965725', '#b2692c')
    }
)

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Test-CheckerPixel([System.Drawing.Color]$color) {
    # ImageGen's displayed transparency grid is baked into these extraction
    # sources. Its two near-white greys are neutral; the warm desert art is not.
    $minimum = [Math]::Min($color.R, [Math]::Min($color.G, $color.B))
    $maximum = [Math]::Max($color.R, [Math]::Max($color.G, $color.B))
    return $minimum -ge 224 -and ($maximum - $minimum) -le 12
}

function Find-ArtBounds([System.Drawing.Bitmap]$bitmap) {
    $left = $bitmap.Width
    $top = $bitmap.Height
    $right = -1
    $bottom = -1

    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            if (Test-CheckerPixel $bitmap.GetPixel($x, $y)) { continue }
            $left = [Math]::Min($left, $x)
            $top = [Math]::Min($top, $y)
            $right = [Math]::Max($right, $x)
            $bottom = [Math]::Max($bottom, $y)
        }
    }

    if ($right -lt $left -or $bottom -lt $top) {
        throw 'The Desert extraction contains no artwork after checkerboard removal.'
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
        $bounds = Find-ArtBounds $source
        $availableWidth = $output.Width - 2 * [int]$specification.HorizontalMargin
        $availableHeight = $output.Height - [int]$specification.HorizonTop - 20
        $scale = [Math]::Min(
            [double]$availableWidth / [double]$bounds.Width,
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
                if (Test-CheckerPixel $pixel) { continue }
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

        # A quiet earth bed prevents camera-height gaps. Every authored source
        # spans the full width: transparent side gutters become enormous holes
        # after Terraria applies each layer's independent parallax scale.
        $floor = [System.Drawing.Color]::FromArgb(255, $palette[0].R, $palette[0].G, $palette[0].B)
        $floorStart = [Math]::Min($output.Height - 12, $offsetY + $scaledHeight)
        for ($y = $floorStart; $y -lt $output.Height; $y++) {
            for ($x = 0; $x -lt $output.Width; $x++) {
                $output.SetPixel($x, $y, $floor)
            }
        }

        # Both sides descend into the same quiet valley before Terraria repeats
        # the texture. This preserves the authored interior while preventing a
        # tall mesa, highway, or foreground bank from ending in a vertical wall.
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

        $directory = Split-Path -Parent $outputPath
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
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

Write-Host 'Generated the diagnostic and production Desert V0 parallax set.'
