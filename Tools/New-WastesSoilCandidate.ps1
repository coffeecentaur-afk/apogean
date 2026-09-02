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

# Five-value ramps deliberately match Terraria's native dirt value structure. The
# hue moves toward dry umber and old ochre without green, purple, flesh-red, or glow.
$tilePalette = @{
    '3B2929' = Convert-HexColor '#241D19'
    '7B5549' = Convert-HexColor '#493728'
    '976B4B' = Convert-HexColor '#654A30'
    'AD7F58' = Convert-HexColor '#80613C'
    'C99B6D' = Convert-HexColor '#A17C4B'
}

$wallPalette = @{
    '1B1313' = Convert-HexColor '#15110F'
    '392622' = Convert-HexColor '#2B211A'
    '452F23' = Convert-HexColor '#3C2D20'
    '503828' = Convert-HexColor '#4D3926'
    '5C4532' = Convert-HexColor '#624A2E'
}

Convert-AtlasPalette `
    (Join-Path $CaptureRoot 'Vanilla-Dirt-Tile.png') `
    'Content/Tiles/Diagnostics/WastesSoilCandidate.png' `
    $tilePalette

Convert-AtlasPalette `
    (Join-Path $CaptureRoot 'Vanilla-DirtUnsafe-Wall.png') `
    'Content/Walls/Diagnostics/WastesDirtWallCandidate.png' `
    $wallPalette

if ($Promote) {
    Convert-AtlasPalette `
        (Join-Path $CaptureRoot 'Vanilla-Dirt-Tile.png') `
        'Content/Tiles/WastesSoil.png' `
        $tilePalette

    Convert-AtlasPalette `
        (Join-Path $CaptureRoot 'Vanilla-DirtUnsafe-Wall.png') `
        'Content/Walls/WastesDirtWallUnsafe.png' `
        $wallPalette

    Write-Host 'Promoted the renderer-validated Wastes soil and dirt wall atlases.' -ForegroundColor Cyan
}

Write-Host 'Generated Wastes soil Tile Lab candidate from live-exported Terraria atlas topology.' -ForegroundColor Green
