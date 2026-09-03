param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourcePath = Join-Path $Root 'Art/Source/Trees/DeadForestTree-reference-source-v1.png'
$destinationPath = Join-Path $Root 'Content/Tiles/DeadForestTreeOverlay.png'

if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "Missing tree source: $sourcePath"
}

$palette = @(
    [System.Drawing.ColorTranslator]::FromHtml('#130f0d'),
    [System.Drawing.ColorTranslator]::FromHtml('#211916'),
    [System.Drawing.ColorTranslator]::FromHtml('#35261f'),
    [System.Drawing.ColorTranslator]::FromHtml('#523827'),
    [System.Drawing.ColorTranslator]::FromHtml('#735033'),
    [System.Drawing.ColorTranslator]::FromHtml('#967041'),
    [System.Drawing.ColorTranslator]::FromHtml('#ccb17d'),
    [System.Drawing.ColorTranslator]::FromHtml('#efdbae'),
    [System.Drawing.ColorTranslator]::FromHtml('#9f5d13'),
    [System.Drawing.ColorTranslator]::FromHtml('#d18a20')
)

function Find-NearestPaletteColor([System.Drawing.Color]$color) {
    $nearest = $palette[0]
    $nearestDistance = [double]::MaxValue
    foreach ($candidate in $palette) {
        $red = [int]$color.R - [int]$candidate.R
        $green = [int]$color.G - [int]$candidate.G
        $blue = [int]$color.B - [int]$candidate.B
        $distance = (0.30 * $red * $red) + (0.59 * $green * $green) + (0.11 * $blue * $blue)
        if ($distance -lt $nearestDistance) {
            $nearestDistance = $distance
            $nearest = $candidate
        }
    }
    return $nearest
}

$source = [System.Drawing.Bitmap]::new($sourcePath)
try {
    $minimumX = $source.Width
    $minimumY = $source.Height
    $maximumX = -1
    $maximumY = -1

    for ($y = 0; $y -lt $source.Height; $y++) {
        for ($x = 0; $x -lt $source.Width; $x++) {
            if ($source.GetPixel($x, $y).A -lt 96) { continue }
            $minimumX = [Math]::Min($minimumX, $x)
            $minimumY = [Math]::Min($minimumY, $y)
            $maximumX = [Math]::Max($maximumX, $x)
            $maximumY = [Math]::Max($maximumY, $y)
        }
    }

    if ($maximumX -lt $minimumX -or $maximumY -lt $minimumY) {
        throw 'The source image contains no sufficiently opaque tree pixels.'
    }

    $sourceBounds = [System.Drawing.Rectangle]::new(
        $minimumX,
        $minimumY,
        $maximumX - $minimumX + 1,
        $maximumY - $minimumY + 1
    )

    # 256 px tall is approximately a tall Terraria forest tree. The 128 px canvas
    # deliberately allows branches and roots to overhang the one-tile gameplay trunk.
    $canvasWidth = 128
    $canvasHeight = 272
    $drawHeight = 256
    $drawWidth = [Math]::Max(1, [int][Math]::Round($sourceBounds.Width * ($drawHeight / [double]$sourceBounds.Height)))
    $drawLeft = [int](($canvasWidth - $drawWidth) / 2)
    $drawTop = $canvasHeight - $drawHeight - 4

    $resampled = [System.Drawing.Bitmap]::new($canvasWidth, $canvasHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($resampled)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.DrawImage(
            $source,
            [System.Drawing.Rectangle]::new($drawLeft, $drawTop, $drawWidth, $drawHeight),
            $sourceBounds,
            [System.Drawing.GraphicsUnit]::Pixel
        )
    }
    finally {
        $graphics.Dispose()
    }

    # Collapse generated soft edges and excess colors into a hard native-scale
    # pixel sprite. The source remains in Art/Source; only this cleaned derivative
    # is loaded by tModLoader.
    for ($y = 0; $y -lt $resampled.Height; $y++) {
        for ($x = 0; $x -lt $resampled.Width; $x++) {
            $pixel = $resampled.GetPixel($x, $y)
            if ($pixel.A -lt 112) {
                $resampled.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                continue
            }
            $nearest = Find-NearestPaletteColor $pixel
            $resampled.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $nearest.R, $nearest.G, $nearest.B))
        }
    }

    $resampled.Save($destinationPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $resampled.Dispose()
}
finally {
    $source.Dispose()
}

# Terraria still asks ModTree for its native trunk, branch, and top atlases.
# Transparent, correctly-sized sheets keep all vanilla tree mechanics and
# framing while the tile draw hook supplies the wider reference silhouette.
foreach ($hiddenSheet in @(
    @{ Name = 'DeadForestTreeHidden.png'; Width = 176; Height = 264 },
    @{ Name = 'DeadForestTreeHidden_Branches.png'; Width = 84; Height = 126 },
    @{ Name = 'DeadForestTreeHidden_Tops.png'; Width = 246; Height = 82 }
)) {
    $hidden = [System.Drawing.Bitmap]::new($hiddenSheet.Width, $hiddenSheet.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $hidden.Save((Join-Path $Root "Content/Tiles/$($hiddenSheet.Name)"), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $hidden.Dispose()
    }
}

Write-Host "Generated reference-faithful tree overlay: $destinationPath"
