$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$failures = [System.Collections.Generic.List[string]]::new()
Add-Type -AssemblyName System.Drawing

function Require-File([string]$relativePath) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing production asset: $relativePath")
    }
}

function Require-SourceContract([string]$relativePath, [string[]]$patterns) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing implementation seam: $relativePath")
        return
    }

    $source = Get-Content -Raw -LiteralPath $path
    foreach ($pattern in $patterns) {
        if ($source -notmatch [regex]::Escape($pattern)) {
            $failures.Add("$relativePath does not cover $pattern")
        }
    }
}

function Require-PngContract([string]$relativePath, [int]$width, [int]$height, [int]$minimumColors) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing production asset: $relativePath")
        return
    }
    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        if ($bitmap.Width -ne $width -or $bitmap.Height -ne $height) {
            $failures.Add("$relativePath is $($bitmap.Width)x$($bitmap.Height); expected $($width)x$($height)")
        }
        $colors = [System.Collections.Generic.HashSet[int]]::new()
		# Sampling every framed cell is sufficient for palette validation and keeps the
		# full background/tile suite fast enough to run on every build.
		$sampleStep = if ($bitmap.Width -gt 500) { 4 } elseif ($bitmap.Width -gt 250) { 3 } else { 1 }
        for ($y = 0; $y -lt $bitmap.Height; $y += $sampleStep) {
            for ($x = 0; $x -lt $bitmap.Width; $x += $sampleStep) {
                $color = $bitmap.GetPixel($x, $y)
                if ($color.A -gt 0) { [void]$colors.Add($color.ToArgb()) }
            }
        }
        if ($colors.Count -lt $minimumColors) {
            $failures.Add("$relativePath has only $($colors.Count) opaque colors; expected at least $minimumColors")
        }
    }
    finally { $bitmap.Dispose() }
}

# Corporate campuses need structural materials, not one repeated shell block.
$corporateFamilies = @('Kessler', 'Helix', 'Sentrix')
$corporateParts = @('Block', 'Trim', 'Floor', 'Glass', 'Beam')
foreach ($family in $corporateFamilies) {
    foreach ($part in $corporateParts) {
        Require-PngContract "Content/Tiles/$family$part.png" 288 270 3
    }
}

# The neutral Wastes and hostile Maw must be visually separate tile families.
$wastesTiles = @('WastesSoil', 'WastesStone', 'WastesGrass', 'WastesSand', 'WastesIce', 'WastesSnow', 'WastesMud')
foreach ($tile in $wastesTiles) {
    Require-PngContract "Content/Tiles/$tile.png" 288 270 5
}

$mawTiles = @('MawDirt', 'Mawstone', 'MawGrass', 'MawSand', 'MawIce', 'MawSnow', 'MawMud', 'MawClay')
foreach ($tile in $mawTiles) {
    Require-PngContract "Content/Tiles/$tile.png" 288 270 8
}

$mawWalls = @('MawDirtWallUnsafe', 'MawStoneWallUnsafe', 'MawGrassWallUnsafe', 'MawSandWallUnsafe', 'MawIceWallUnsafe', 'MawSnowWallUnsafe', 'MawMudWallUnsafe')
foreach ($wall in $mawWalls) {
    Require-PngContract "Content/Walls/$wall.png" 468 180 5
}

foreach ($wall in @('WastesDirtWallUnsafe','WastesStoneWallUnsafe','WastesGrassWallUnsafe','WastesSandWallUnsafe','WastesIceWallUnsafe','WastesSnowWallUnsafe','WastesMudWallUnsafe')) {
    Require-PngContract "Content/Walls/$wall.png" 468 180 5
}

foreach ($wall in @(
    'KesslerBulkheadWall','KesslerWindowWall','HelixLaboratoryWall',
    'HelixObservationWall','SentrixDataWall','SentrixWindowWall'
)) {
    Require-PngContract "Content/Walls/$wall.png" 468 180 5
}

# A complete conversion registry must handle representative natural tile and wall families.
$naturalTiles = @(
    'TileID.Dirt', 'TileID.Stone', 'TileID.Grass', 'TileID.Sand',
    'TileID.ClayBlock', 'TileID.Mud', 'TileID.JungleGrass',
    'TileID.SnowBlock', 'TileID.IceBlock', 'TileID.Sandstone',
    'TileID.HardenedSand', 'TileID.Silt', 'TileID.Slush'
)
$naturalWalls = @(
    'WallID.DirtUnsafe', 'WallID.Stone', 'WallID.GrassUnsafe',
    'WallID.FlowerUnsafe', 'WallID.JungleUnsafe', 'WallID.MudUnsafe',
    'WallID.SnowWallUnsafe', 'WallID.IceUnsafe', 'WallID.Sandstone',
    'WallID.HardenedSand'
)
Require-SourceContract 'Content/World/MawConversionSystem.cs' ($naturalTiles + $naturalWalls)

# Fixed structures need explicit terrain integration rather than a cleared air moat.
Require-SourceContract 'Content/Structures/CorporateTerrainIntegration.cs' @(
    'BlendGroundCampus', 'SealFoundation', 'RestoreTerrainShoulders',
    'PlaceHelixSurfaceDome'
)

# Custom trees require all tModLoader texture surfaces and a registered foliage contract.
foreach ($treeAsset in @(
    'Content/Tiles/DeadForestTree.png',
    'Content/Tiles/DeadForestTree_Branches.png',
    'Content/Tiles/DeadForestTree_Tops.png'
)) {
    Require-File $treeAsset
}
Require-PngContract 'Content/Tiles/DeadForestTree.png' 176 264 8
Require-PngContract 'Content/Tiles/DeadForestTree_Branches.png' 84 126 6
Require-PngContract 'Content/Tiles/DeadForestTree_Tops.png' 246 82 6
Require-SourceContract 'Content/Tiles/DeadForestTree.cs' @(
    'GetTexture', 'GetBranchTextures', 'GetTopTextures'
)

foreach ($blueprint in @('KesslerCampus','HelixCampus','SentrixCampus')) {
    $path = Join-Path $root "Content/Structures/Blueprints/$blueprint.apstructure"
    if (-not (Test-Path -LiteralPath $path)) {
        $failures.Add("Missing blueprint: $blueprint")
        continue
    }
    $source = Get-Content -Raw -LiteralPath $path
    if ($blueprint -ne 'SentrixCampus' -and $source -match '(?m)^size\s+(\d+)\s+(\d+)') {
        $width = $Matches[1]
        $height = $Matches[2]
        if ($source -match "(?m)^clear\s+0\s+0\s+$width\s+$height\s*$") {
            $failures.Add("$blueprint clears its full bounding box and will create an air moat")
        }
    }
}
Require-SourceContract 'Content/Structures/Blueprints/KesslerCampus.apstructure' @('surface 70')
Require-SourceContract 'Content/Structures/Blueprints/HelixCampus.apstructure' @('surface 45')

foreach ($biome in @('Forest','Desert','Jungle','Snow','Corruption','Crimson','Hallow','Ocean','Mushroom','Underworld','Engraft')) {
    Require-PngContract "Content/Backgrounds/$biome/V0_Far.png" 1024 408 5
    Require-PngContract "Content/Backgrounds/$biome/V0_Mid.png" 1024 600 5
    Require-PngContract "Content/Backgrounds/$biome/V0_Close.png" 952 480 5
    Require-PngContract "Content/Backgrounds/$biome/Underground/V0_1.png" 160 96 5
}

if ($failures.Count -gt 0) {
    Write-Host 'WORLD VISUAL INTEGRITY: FAIL' -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host " - $failure" -ForegroundColor Red
    }
    exit 1
}

Write-Host 'WORLD VISUAL INTEGRITY: PASS' -ForegroundColor Green
