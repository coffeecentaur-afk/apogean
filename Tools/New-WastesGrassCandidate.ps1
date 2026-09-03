param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$CaptureRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'),
    [switch]$Promote
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Convert-AtlasPalette(
    [string]$sourcePath,
    [string]$outputRelativePath,
    [hashtable]$palette
) {
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Missing exported vanilla atlas: $sourcePath. Load the Tile Lab world or run /apogean exportatlases first."
    }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -eq 0) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $key = '{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
                if (-not $palette.ContainsKey($key)) {
                    throw "Unexpected source color #$key in $sourcePath at ($x,$y). Refusing to guess at atlas conversion."
                }

                $output.SetPixel($x, $y, $palette[$key])
            }
        }

        $outputPath = Join-Path $Root $outputRelativePath
        $directory = Split-Path -Parent $outputPath
        if (-not (Test-Path -LiteralPath $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

# Preserve the complete vanilla grass atlas, including its white engine masks.
# Dirt colors match the renderer-approved Wastes soil exactly. Green foliage is
# replaced with a dry five-value root/straw ramp without glow or flesh hues.
$grassTilePalette = @{
    '3B2929' = Convert-HexColor '#241D19'
    '7B5549' = Convert-HexColor '#493728'
    '976B4B' = Convert-HexColor '#654A30'
    'AD7F58' = Convert-HexColor '#80613C'
    'C99B6D' = Convert-HexColor '#A17C4B'
    '253538' = Convert-HexColor '#2A2219'
    '28473A' = Convert-HexColor '#4B3920'
    '396346' = Convert-HexColor '#715426'
    '3B8044' = Convert-HexColor '#98702E'
    '5DA34F' = Convert-HexColor '#C09943'
    'FFFFFF' = Convert-HexColor '#FFFFFF'
}

$grassWallPalette = @{
    '172022' = Convert-HexColor '#191611'
    '1A2B24' = Convert-HexColor '#2A2318'
    '253B2C' = Convert-HexColor '#3C301D'
    '284D2C' = Convert-HexColor '#523D21'
    '3C6234' = Convert-HexColor '#725426'
}

Convert-AtlasPalette `
    (Join-Path $CaptureRoot 'Vanilla-Grass-Tile.png') `
    'Content/Tiles/Diagnostics/WastesGrassCandidate.png' `
    $grassTilePalette

Convert-AtlasPalette `
    (Join-Path $CaptureRoot 'Vanilla-GrassUnsafe-Wall.png') `
    'Content/Walls/Diagnostics/WastesGrassWallCandidate.png' `
    $grassWallPalette

if ($Promote) {
    Convert-AtlasPalette `
        (Join-Path $CaptureRoot 'Vanilla-Grass-Tile.png') `
        'Content/Tiles/WastesGrass.png' `
        $grassTilePalette

    Convert-AtlasPalette `
        (Join-Path $CaptureRoot 'Vanilla-GrassUnsafe-Wall.png') `
        'Content/Walls/WastesGrassWallUnsafe.png' `
        $grassWallPalette

    Write-Host 'Promoted the renderer-validated Wastes grass and grass-wall atlases.' -ForegroundColor Cyan
}

Write-Host 'Generated Wastes grass Tile Lab candidates from live-exported Terraria atlas topology.' -ForegroundColor Green
