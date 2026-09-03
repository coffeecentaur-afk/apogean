param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$layers = @(
    @{
        Name = 'Far'
        Source = 'Art/Source/Backgrounds/Forest/V0-Far-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/ForestConceptV0_Far.png'
        Width = 1024
        Height = 408
        Palette = @('#4a4237', '#594f41', '#685c4b', '#776a56', '#877760', '#98866c', '#aa9679', '#bba88c')
    },
    @{
        Name = 'Mid'
        Source = 'Art/Source/Backgrounds/Forest/V0-Mid-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/ForestConceptV0_Mid.png'
        Width = 1024
        Height = 600
        Palette = @('#211d19', '#2d2721', '#3a3128', '#483b2f', '#584838', '#695742', '#7c684e', '#917a5b', '#a68e6c', '#bba17d')
    },
    @{
        Name = 'Close'
        Source = 'Art/Source/Backgrounds/Forest/V0-Close-extraction-v1.png'
        Output = 'Content/Backgrounds/Diagnostics/ForestConceptV0_Close.png'
        Width = 952
        Height = 480
        Palette = @('#12100f', '#1b1815', '#27211b', '#342a20', '#443526', '#58452f', '#705838', '#8b6d43', '#a8844d', '#c09a58')
    }
)

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Test-CheckerPixel([System.Drawing.Color]$color) {
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
        throw 'The source contains no artwork after checkerboard removal.'
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
        $scale = [Math]::Min(
            [double]$output.Width / [double]$bounds.Width,
            [double]$output.Height / [double]$bounds.Height)
        $scaledWidth = [Math]::Max(1, [int][Math]::Floor($bounds.Width * $scale))
        $scaledHeight = [Math]::Max(1, [int][Math]::Floor($bounds.Height * $scale))
        $offsetX = [int][Math]::Floor(($output.Width - $scaledWidth) / 2.0)
        $offsetY = $output.Height - $scaledHeight

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

        # The generated silhouettes already span the image. Seal only the final
        # twelve rows with one low-frequency earth tone; stretching each source
        # column to the floor creates a visible barcode in Terraria's draw scale.
        $floor = [System.Drawing.Color]::FromArgb(255, $palette[0].R, $palette[0].G, $palette[0].B)
        for ($y = $output.Height - 12; $y -lt $output.Height; $y++) {
            for ($x = 0; $x -lt $output.Width; $x++) {
                $output.SetPixel($x, $y, $floor)
            }
        }

        # Terraria repeats these textures horizontally. Keep the terminal column
        # byte-identical to the first; the generated source already keeps both edge
        # silhouettes low and generic so this final seal does not move a landmark.
        for ($y = 0; $y -lt $output.Height; $y++) {
            $output.SetPixel($output.Width - 1, $y, $output.GetPixel(0, $y))
        }

        $directory = Split-Path -Parent $outputPath
        if (-not (Test-Path -LiteralPath $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
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

Write-Host 'Generated the diagnostic Forest concept parallax set.'
