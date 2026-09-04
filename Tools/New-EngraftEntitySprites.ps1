param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [Drawing.ColorTranslator]::FromHtml($hex) }
$entityPalette = @(
    (C '#171316'), (C '#282126'), (C '#3d3030'), (C '#584238'),
    (C '#765945'), (C '#9a744b'), (C '#c39755'), (C '#e2bd72'),
    (C '#7b4b11'), (C '#b87512'), (C '#eea91b'), (C '#ffd958'),
    (C '#b9aa8d'), (C '#e3d5ad')
)

function Get-OpaqueBounds([Drawing.Bitmap]$bitmap, [Drawing.Rectangle]$search, [int]$alphaThreshold = 72) {
    $minX = $search.Right; $minY = $search.Bottom; $maxX = -1; $maxY = -1
    for ($y = $search.Top; $y -lt $search.Bottom; $y++) {
        for ($x = $search.Left; $x -lt $search.Right; $x++) {
            if ($bitmap.GetPixel($x, $y).A -lt $alphaThreshold) { continue }
            $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
            $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
        }
    }
    if ($maxX -lt 0) { throw "No visible source art in band $search" }
    [Drawing.Rectangle]::FromLTRB($minX, $minY, $maxX + 1, $maxY + 1)
}

function Find-Nearest([Drawing.Color]$pixel, [Drawing.Color[]]$palette) {
    $best = $palette[0]; $distance = [double]::MaxValue
    foreach ($candidate in $palette) {
        $dr = [double]$pixel.R - $candidate.R
        $dg = [double]$pixel.G - $candidate.G
        $db = [double]$pixel.B - $candidate.B
        $candidateDistance = $dr * $dr + $dg * $dg + $db * $db
        if ($candidateDistance -lt $distance) { $best = $candidate; $distance = $candidateDistance }
    }
    $best
}

function Convert-Frame(
    [Drawing.Bitmap]$source,
    [Drawing.Rectangle]$sourceBounds,
    [int]$canvasWidth,
    [int]$canvasHeight,
    [int]$maximumWidth,
    [int]$maximumHeight,
    [ValidateSet('Center', 'Ground')][string]$anchor
) {
    $scale = [Math]::Min($maximumWidth / [double]$sourceBounds.Width, $maximumHeight / [double]$sourceBounds.Height)
    $width = [Math]::Max(1, [int][Math]::Round($sourceBounds.Width * $scale))
    $height = [Math]::Max(1, [int][Math]::Round($sourceBounds.Height * $scale))
    $left = [int][Math]::Floor(($canvasWidth - $width) / 2.0)
    $top = if ($anchor -eq 'Ground') { $canvasHeight - $height - 2 } else { [int][Math]::Floor(($canvasHeight - $height) / 2.0) }

    $sample = [Drawing.Bitmap]::new($width, $height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [Drawing.Graphics]::FromImage($sample)
    try {
        $graphics.Clear([Drawing.Color]::Transparent)
        $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.DrawImage($source, [Drawing.Rectangle]::new(0, 0, $width, $height), $sourceBounds, [Drawing.GraphicsUnit]::Pixel)
    }
    finally { $graphics.Dispose() }

    $frame = [Drawing.Bitmap]::new($canvasWidth, $canvasHeight, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $sample.Height; $y++) {
            for ($x = 0; $x -lt $sample.Width; $x++) {
                $pixel = $sample.GetPixel($x, $y)
                if ($pixel.A -lt 96) { continue }
                $mapped = Find-Nearest $pixel $entityPalette
                $frame.SetPixel($left + $x, $top + $y, [Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
            }
        }
        return $frame
    }
    finally { $sample.Dispose() }
}

function New-StackedSheet(
    [string]$sourceRelative,
    [string]$destinationRelative,
    [int]$canvasWidth,
    [int]$canvasHeight,
    [int]$maximumWidth,
    [int]$maximumHeight,
    [ValidateSet('Center', 'Ground')][string]$anchor
) {
    $source = [Drawing.Bitmap]::new((Join-Path $Root $sourceRelative))
    $sheet = [Drawing.Bitmap]::new($canvasWidth, $canvasHeight * 4, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($frameIndex = 0; $frameIndex -lt 4; $frameIndex++) {
            $bandTop = [int][Math]::Floor($source.Height * $frameIndex / 4.0)
            $bandBottom = [int][Math]::Floor($source.Height * ($frameIndex + 1) / 4.0)
            $search = [Drawing.Rectangle]::FromLTRB(0, $bandTop, $source.Width, $bandBottom)
            $bounds = Get-OpaqueBounds $source $search
            $frame = Convert-Frame $source $bounds $canvasWidth $canvasHeight $maximumWidth $maximumHeight $anchor
            try {
                for ($y = 0; $y -lt $canvasHeight; $y++) {
                    for ($x = 0; $x -lt $canvasWidth; $x++) {
                        $sheet.SetPixel($x, $frameIndex * $canvasHeight + $y, $frame.GetPixel($x, $y))
                    }
                }
            }
            finally { $frame.Dispose() }
        }
        $sheet.Save((Join-Path $Root $destinationRelative), [Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $source.Dispose(); $sheet.Dispose() }
}

function New-ItemIcon([string]$sourceRelative, [string]$destinationRelative) {
    $source = [Drawing.Bitmap]::new((Join-Path $Root $sourceRelative))
    try {
        $bounds = Get-OpaqueBounds $source ([Drawing.Rectangle]::new(0, 0, $source.Width, $source.Height))
        $strand = Convert-Frame $source $bounds 16 20 8 18 'Center'
        $icon = [Drawing.Bitmap]::new(24, 24, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            # Three offset strands turn the generated hook into a readable bundle
            # instead of preserving the old one-pixel-wide inventory silhouette.
            foreach ($offset in @(@(-4, 3), @(0, 0), @(4, -3))) {
                for ($y = 0; $y -lt $strand.Height; $y++) {
                    for ($x = 0; $x -lt $strand.Width; $x++) {
                        $pixel = $strand.GetPixel($x, $y)
                        if ($pixel.A -eq 0) { continue }
                        $targetX = 4 + $x + $offset[0]
                        $targetY = 2 + $y + $offset[1]
                        if ($targetX -ge 0 -and $targetX -lt $icon.Width -and $targetY -ge 0 -and $targetY -lt $icon.Height) {
                            $icon.SetPixel($targetX, $targetY, $pixel)
                        }
                    }
                }
            }
            $icon.Save((Join-Path $Root $destinationRelative), [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $strand.Dispose(); $icon.Dispose() }
    }
    finally { $source.Dispose() }
}

New-StackedSheet 'Art/Source/Entities/Mawling-concept-v2.png' 'Content/NPCs/Engraft/Mawling.png' 40 32 36 28 'Center'
New-StackedSheet 'Art/Source/Entities/GraftHound-concept-v2.png' 'Content/NPCs/Engraft/GraftHound.png' 64 36 60 30 'Ground'
New-ItemIcon 'Art/Source/Entities/MawFibre-concept-v2.png' 'Content/Items/Materials/MawFibre.png'

Write-Host 'Generated gameplay-scale Mawling, Graft Hound, and Maw Fibre pixel assets.' -ForegroundColor Green
