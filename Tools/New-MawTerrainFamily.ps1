param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$Hex) {
    [System.Drawing.ColorTranslator]::FromHtml($Hex)
}

function Convert-Atlas(
    [string]$SourceRelativePath,
    [string]$CandidateRelativePath,
    [string]$ProductionRelativePath,
    [string[]]$PaletteHex
) {
    $sourcePath = Join-Path $Root $SourceRelativePath
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Missing validated neutral source atlas: $sourcePath"
    }

    $palette = @($PaletteHex | ForEach-Object { Convert-HexColor $_ })
    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $luminances = [System.Collections.Generic.List[double]]::new()
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -gt 0) {
                    $luminances.Add((0.299 * $pixel.R) + (0.587 * $pixel.G) + (0.114 * $pixel.B))
                }
            }
        }
        if ($luminances.Count -eq 0) {
            throw "Source atlas contains no opaque pixels: $sourcePath"
        }

        $minimum = ($luminances | Measure-Object -Minimum).Minimum
        $maximum = ($luminances | Measure-Object -Maximum).Maximum
        $range = [Math]::Max(1.0, $maximum - $minimum)
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -eq 0) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $luminance = (0.299 * $pixel.R) + (0.587 * $pixel.G) + (0.114 * $pixel.B)
                $normalized = [Math]::Clamp(($luminance - $minimum) / $range, 0.0, 1.0)
                $index = [Math]::Min($palette.Count - 1, [int][Math]::Floor($normalized * $palette.Count))
                $chosen = $palette[$index]
                $output.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($pixel.A, $chosen.R, $chosen.G, $chosen.B))
            }
        }

        foreach ($relativePath in @($CandidateRelativePath, $ProductionRelativePath)) {
            if ([string]::IsNullOrWhiteSpace($relativePath)) { continue }
            $path = Join-Path $Root $relativePath
            $directory = Split-Path -Parent $path
            if (-not (Test-Path -LiteralPath $directory)) {
                New-Item -ItemType Directory -Path $directory | Out-Null
            }
            $temporaryPath = "$path.generated.png"
            $output.Save($temporaryPath, [System.Drawing.Imaging.ImageFormat]::Png)
            Move-Item -LiteralPath $temporaryPath -Destination $path -Force
        }
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

$dirtPalette = @('#15130f', '#29231a', '#49381e', '#6d4b19', '#986916', '#cd941e')
$stonePalette = @('#181713', '#2c2920', '#49422c', '#6c5b2d', '#967322', '#c99a2c')
$grassPalette = @('#15130f', '#2a2115', '#51401c', '#80601a', '#b37b13', '#e3aa24')
$sandPalette = @('#332613', '#5b4018', '#85601d', '#ad7e25', '#d6a43a', '#f0ca66')
$icePalette = @('#1d2621', '#334137', '#53604a', '#777b54', '#a39a5b', '#d0bb72')
$snowPalette = @('#38351f', '#5a5230', '#7f7040', '#aa9252', '#d1b772', '#ead593')
$mudPalette = @('#12130f', '#25271b', '#3e3d22', '#5c5427', '#7c6725', '#a47e24')
$clayPalette = @('#24170f', '#482818', '#6c3d1c', '#965722', '#bd752c', '#df9b4b')

$tileFamilies = @(
    @('Content/Tiles/WastesSoil.png',  'Content/Tiles/Diagnostics/MawDirtCandidate.png',  'Content/Tiles/MawDirt.png',  $dirtPalette),
    @('Content/Tiles/WastesStone.png', 'Content/Tiles/Diagnostics/MawStoneCandidate.png', 'Content/Tiles/Mawstone.png',  $stonePalette),
    @('Content/Tiles/WastesGrass.png', 'Content/Tiles/Diagnostics/MawGrassCandidate.png', 'Content/Tiles/MawGrass.png',  $grassPalette),
    @('Content/Tiles/WastesSand.png',  'Content/Tiles/Diagnostics/MawSandCandidate.png',  'Content/Tiles/MawSand.png',  $sandPalette),
    @('Content/Tiles/WastesIce.png',   'Content/Tiles/Diagnostics/MawIceCandidate.png',   'Content/Tiles/MawIce.png',   $icePalette),
    @('Content/Tiles/WastesSnow.png',  'Content/Tiles/Diagnostics/MawSnowCandidate.png',  'Content/Tiles/MawSnow.png',  $snowPalette),
    @('Content/Tiles/WastesMud.png',   'Content/Tiles/Diagnostics/MawMudCandidate.png',   'Content/Tiles/MawMud.png',   $mudPalette),
    @('Content/Tiles/WastesSoil.png',  'Content/Tiles/Diagnostics/MawClayCandidate.png',  'Content/Tiles/MawClay.png',  $clayPalette)
)
foreach ($family in $tileFamilies) {
    Convert-Atlas $family[0] $family[1] $family[2] $family[3]
}

$wallFamilies = @(
    @('Content/Walls/WastesDirtWallUnsafe.png',  'Content/Walls/Diagnostics/MawDirtWallCandidate.png',  'Content/Walls/MawDirtWallUnsafe.png',  $dirtPalette),
    @('Content/Walls/WastesStoneWallUnsafe.png', 'Content/Walls/Diagnostics/MawStoneWallCandidate.png', 'Content/Walls/MawStoneWallUnsafe.png', $stonePalette),
    @('Content/Walls/WastesGrassWallUnsafe.png', 'Content/Walls/Diagnostics/MawGrassWallCandidate.png', 'Content/Walls/MawGrassWallUnsafe.png', $grassPalette),
    @('Content/Walls/WastesSandWallUnsafe.png',  'Content/Walls/Diagnostics/MawSandWallCandidate.png',  'Content/Walls/MawSandWallUnsafe.png',  $sandPalette),
    @('Content/Walls/WastesIceWallUnsafe.png',   'Content/Walls/Diagnostics/MawIceWallCandidate.png',   'Content/Walls/MawIceWallUnsafe.png',   $icePalette),
    @('Content/Walls/WastesSnowWallUnsafe.png',  'Content/Walls/Diagnostics/MawSnowWallCandidate.png',  'Content/Walls/MawSnowWallUnsafe.png',  $snowPalette),
    @('Content/Walls/WastesMudWallUnsafe.png',   'Content/Walls/Diagnostics/MawMudWallCandidate.png',   'Content/Walls/MawMudWallUnsafe.png',   $mudPalette)
)
foreach ($family in $wallFamilies) {
    Convert-Atlas $family[0] $family[1] $family[2] $family[3]
}

$itemFamilies = @(
    @('Content/Items/Placeable/WastesSoilBlock.png',  'Content/Items/Placeable/MawDirtBlock.png',  $dirtPalette),
    @('Content/Items/Placeable/WastesStoneBlock.png', 'Content/Items/Placeable/MawstoneBlock.png',  $stonePalette),
    @('Content/Items/Placeable/WastesSandBlock.png',  'Content/Items/Placeable/MawSandBlock.png',  $sandPalette),
    @('Content/Items/Placeable/WastesIceBlock.png',   'Content/Items/Placeable/MawIceBlock.png',   $icePalette),
    @('Content/Items/Placeable/WastesSnowBlock.png',  'Content/Items/Placeable/MawSnowBlock.png',  $snowPalette),
    @('Content/Items/Placeable/WastesMudBlock.png',   'Content/Items/Placeable/MawMudBlock.png',   $mudPalette),
    @('Content/Items/Placeable/WastesSoilBlock.png',  'Content/Items/Placeable/MawClayBlock.png',  $clayPalette)
)
foreach ($family in $itemFamilies) {
    Convert-Atlas $family[0] '' $family[1] $family[2]
}
Convert-Atlas 'Content/Projectiles/WastesSandBallProjectile.png' '' 'Content/Projectiles/MawSandBallProjectile.png' $sandPalette

Write-Host 'Generated native-topology Maw terrain, walls, items, and sand projectile.' -ForegroundColor Green
