param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$Hex) {
    [System.Drawing.ColorTranslator]::FromHtml($Hex)
}

# This exporter intentionally derives the native sprites from the approved
# concept sheet. It preserves those silhouettes instead of approximating them
# with vector-like line primitives.
$outline = Convert-HexColor '#211916'
$shadow = Convert-HexColor '#35261F'
$wood = Convert-HexColor '#59402A'
$ochre = Convert-HexColor '#8F6C3D'
$straw = Convert-HexColor '#B9924E'
$pale = Convert-HexColor '#D0B66F'
$amber = Convert-HexColor '#D78A19'

function New-TransparentBitmap([int]$Width, [int]$Height) {
    $bitmap = [System.Drawing.Bitmap]::new($Width, $Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $Height; $y++) {
        for ($x = 0; $x -lt $Width; $x++) {
            $bitmap.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
        }
    }
    $bitmap
}

function Test-IsConceptPixel([System.Drawing.Color]$Color) {
    # The generated reference has a baked white/light-gray checkerboard. All
    # intended roots, outlines, and amber pods sit safely below this threshold.
    $Color.A -gt 0 -and -not ($Color.R -ge 190 -and $Color.G -ge 170 -and $Color.B -ge 150)
}

function Get-PaletteColor([int]$R, [int]$G, [int]$B) {
    if ($R -gt 105 -and $R -gt ($G * 1.42) -and $G -lt 125) {
        return $amber
    }

    $luminance = (0.299 * $R) + (0.587 * $G) + (0.114 * $B)
    if ($luminance -lt 48) { return $outline }
    if ($luminance -lt 70) { return $shadow }
    if ($luminance -lt 95) { return $wood }
    if ($luminance -lt 124) { return $ochre }
    if ($luminance -lt 154) { return $straw }
    return $pale
}

function New-NativeSprite(
    [System.Drawing.Bitmap]$Reference,
    [System.Drawing.Rectangle]$Crop,
    [int]$OutputWidth,
    [int]$OutputHeight
) {
    $bitmap = New-TransparentBitmap $OutputWidth $OutputHeight
    $scale = [Math]::Min(($OutputWidth - 2.0) / $Crop.Width, ($OutputHeight - 2.0) / $Crop.Height)
    $drawWidth = [Math]::Max(1, [Math]::Floor($Crop.Width * $scale))
    $drawHeight = [Math]::Max(1, [Math]::Floor($Crop.Height * $scale))
    $left = [Math]::Floor(($OutputWidth - $drawWidth) / 2.0)
    $top = $OutputHeight - $drawHeight

    for ($dy = 0; $dy -lt $drawHeight; $dy++) {
        for ($dx = 0; $dx -lt $drawWidth; $dx++) {
            $sourceLeft = $Crop.X + [Math]::Floor(($dx * $Crop.Width) / $drawWidth)
            $sourceRight = $Crop.X + [Math]::Ceiling((($dx + 1) * $Crop.Width) / $drawWidth) - 1
            $sourceTop = $Crop.Y + [Math]::Floor(($dy * $Crop.Height) / $drawHeight)
            $sourceBottom = $Crop.Y + [Math]::Ceiling((($dy + 1) * $Crop.Height) / $drawHeight) - 1

            $sampleCount = 0
            $rootCount = 0
            $red = 0L
            $green = 0L
            $blue = 0L
            for ($sy = $sourceTop; $sy -le $sourceBottom; $sy++) {
                for ($sx = $sourceLeft; $sx -le $sourceRight; $sx++) {
                    $sampleCount++
                    $pixel = $Reference.GetPixel($sx, $sy)
                    if (-not (Test-IsConceptPixel $pixel)) { continue }
                    $rootCount++
                    $red += $pixel.R
                    $green += $pixel.G
                    $blue += $pixel.B
                }
            }

            # Low enough to retain tapered tips; high enough to reject the
            # concept image's anti-aliased fringe and checkerboard.
            if ($rootCount -eq 0 -or ($rootCount / [double]$sampleCount) -lt 0.18) { continue }
            $color = Get-PaletteColor ([Math]::Round($red / $rootCount)) ([Math]::Round($green / $rootCount)) ([Math]::Round($blue / $rootCount))
            $bitmap.SetPixel($left + $dx, $top + $dy, $color)
        }
    }

    # Grow a one-pixel outline outside the concept silhouette, then restore the
    # quantized interior. Replacing edge pixels made narrow roots turn entirely
    # black; growing outward keeps their ochre mass and improves readability.
    $outlined = New-TransparentBitmap $OutputWidth $OutputHeight
    for ($y = 0; $y -lt $OutputHeight; $y++) {
        for ($x = 0; $x -lt $OutputWidth; $x++) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -eq 0) { continue }
            foreach ($offset in @(@(-1, -1), @(0, -1), @(1, -1), @(-1, 0), @(1, 0), @(-1, 1), @(0, 1), @(1, 1))) {
                $neighborX = $x + $offset[0]
                $neighborY = $y + $offset[1]
                if ($neighborX -ge 0 -and $neighborY -ge 0 -and $neighborX -lt $OutputWidth -and $neighborY -lt $OutputHeight) {
                    $outlined.SetPixel($neighborX, $neighborY, $outline)
                }
            }
        }
    }
    for ($y = 0; $y -lt $OutputHeight; $y++) {
        for ($x = 0; $x -lt $OutputWidth; $x++) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -gt 0) {
                $outlined.SetPixel($x, $y, $pixel)
            }
        }
    }
    $bitmap.Dispose()
    $outlined
}

function Copy-LogicalSprite($Source, $Atlas, [int]$StyleIndex, [int]$StyleStride) {
    # tModLoader's object atlas reserves a hidden two-pixel gutter after each
    # 16px tile cell. Packing around it prevents cross-tile branches vanishing.
    for ($y = 0; $y -lt $Source.Height; $y++) {
        for ($x = 0; $x -lt $Source.Width; $x++) {
            $color = $Source.GetPixel($x, $y)
            if ($color.A -eq 0) { continue }
            $atlasX = ($StyleIndex * $StyleStride) + $x + (2 * [Math]::Floor($x / 16))
            $atlasY = $y + (2 * [Math]::Floor($y / 16))
            $Atlas.SetPixel($atlasX, $atlasY, $color)
        }
    }
}

function Export-Family(
    [System.Drawing.Bitmap]$Reference,
    [System.Drawing.Rectangle[]]$Crops,
    [int]$LogicalWidth,
    [int]$LogicalHeight,
    [int]$StyleStride,
    [int]$AtlasHeight,
    [string]$RelativePath
) {
    $atlas = New-TransparentBitmap ($Crops.Length * $StyleStride) $AtlasHeight
    try {
        for ($style = 0; $style -lt $Crops.Length; $style++) {
            $sprite = New-NativeSprite $Reference $Crops[$style] $LogicalWidth $LogicalHeight
            try { Copy-LogicalSprite $sprite $atlas $style $StyleStride }
            finally { $sprite.Dispose() }
        }

        $path = Join-Path $Root $RelativePath
        $directory = Split-Path -Parent $path
        if (-not (Test-Path -LiteralPath $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $atlas.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $atlas.Dispose()
    }
}

$referencePath = Join-Path $Root 'Art/Reference/WastesGroundCover-reference-v2.png'
if (-not (Test-Path -LiteralPath $referencePath)) {
    throw "Missing approved ground-cover reference: $referencePath"
}

$reference = [System.Drawing.Bitmap]::new($referencePath)
try {
    $tufts = @(
        [System.Drawing.Rectangle]::new(62, 88, 285, 166),
        [System.Drawing.Rectangle]::new(405, 60, 277, 195),
        [System.Drawing.Rectangle]::new(719, 78, 287, 177),
        [System.Drawing.Rectangle]::new(982, 106, 356, 149)
    )
    $bristles = @(
        [System.Drawing.Rectangle]::new(132, 303, 258, 403),
        [System.Drawing.Rectangle]::new(554, 315, 242, 388),
        [System.Drawing.Rectangle]::new(943, 303, 250, 400)
    )
    $shrubs = @(
        [System.Drawing.Rectangle]::new(44, 749, 408, 311),
        [System.Drawing.Rectangle]::new(486, 748, 454, 318),
        [System.Drawing.Rectangle]::new(926, 755, 432, 317)
    )

    Export-Family $reference $tufts 32 16 36 18 'Content/Tiles/DeadTuft.png'
    Export-Family $reference $bristles 32 48 36 54 'Content/Tiles/WastesBristle.png'
    Export-Family $reference $shrubs 48 32 54 36 'Content/Tiles/WastesRootShrub.png'
}
finally {
    $reference.Dispose()
}

Write-Host 'Exported concept-derived, gutter-aware Wastes ground-cover sheets.' -ForegroundColor Green
