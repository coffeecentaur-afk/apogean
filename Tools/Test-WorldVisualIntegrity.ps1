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

function Require-OrderedSourceContract([string]$relativePath, [string[]]$patterns) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing implementation seam: $relativePath")
        return
    }

    $source = Get-Content -Raw -LiteralPath $path
    $cursor = 0
    foreach ($pattern in $patterns) {
        $index = $source.IndexOf($pattern, $cursor, [System.StringComparison]::Ordinal)
        if ($index -lt 0) {
            $failures.Add("$relativePath does not place $pattern after the preceding world-generation pass")
            return
        }
        $cursor = $index + $pattern.Length
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

function Require-TransparentPng([string]$relativePath, [int]$width, [int]$height) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing transparent renderer asset: $relativePath")
        return
    }
    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        if ($bitmap.Width -ne $width -or $bitmap.Height -ne $height) {
            $failures.Add("$relativePath is $($bitmap.Width)x$($bitmap.Height); expected $($width)x$($height)")
        }
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                if ($bitmap.GetPixel($x, $y).A -ne 0) {
                    $failures.Add("$relativePath leaks visible native-tree pixels at $x,$y")
                    return
                }
            }
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
    $height = if ($tile -eq 'WastesGrass') { 1980 } else { 270 }
    Require-PngContract "Content/Tiles/$tile.png" 288 $height 5
}

$mawTiles = @('MawDirt', 'Mawstone', 'MawGrass', 'MawSand', 'MawIce', 'MawSnow', 'MawMud', 'MawClay')
foreach ($tile in $mawTiles) {
    $height = if ($tile -eq 'MawGrass') { 1980 } else { 270 }
    Require-PngContract "Content/Tiles/$tile.png" 288 $height 4
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
    'TileID.HardenedSand', 'TileID.Silt', 'TileID.Slush',
    'TileID.Ebonstone', 'TileID.Crimsand', 'TileID.HallowedIce',
    'TileID.MushroomGrass', 'TileID.Ash'
)
$naturalWalls = @(
    'WallID.DirtUnsafe', 'WallID.Stone', 'WallID.GrassUnsafe',
    'WallID.FlowerUnsafe', 'WallID.JungleUnsafe', 'WallID.MudUnsafe',
    'WallID.SnowWallUnsafe', 'WallID.IceUnsafe', 'WallID.Sandstone',
    'WallID.HardenedSand', 'WallID.EbonstoneUnsafe', 'WallID.CrimsonHardenedSand',
    'WallID.HallowUnsafe1', 'WallID.MushroomUnsafe', 'WallID.LavaUnsafe1'
)
Require-SourceContract 'Content/World/MawConversionSystem.cs' ($naturalTiles + $naturalWalls)
Require-SourceContract 'Content/Diagnostics/MawConversionGallery.cs' @(
    'Corrupt Stone', 'Crimson Sand', 'Hallow Ice', 'Jungle Grass',
    'Mushroom Grass', 'Underworld Ash', 'PlaceAndValidatePreservedContent'
)

Require-SourceContract 'Content/Diagnostics/TileLabPlayer.cs' @(
	'ApogeanLiveValidation.request',
	'case "conversion"',
	'case "forest-background"',
	'case "desert-background"',
	'case "jungle-background"',
	'case "snow-background"',
	'case "corruption-background"',
	'case "crimson-background"',
	'case "hallow-background"',
	'case "ocean-background"',
	'case "mushroom-background"',
	'case "forest-background-night"',
	'case "forest-background-eclipse"',
	'case "kessler-construction"',
	'case "kessler-world"',
	'BuildMawConversionAndReport(scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Desert, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Jungle, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Snow, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Corruption, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Crimson, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Hallow, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Ocean, scheduleCaptureProbe: true)',
	'BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Mushroom, scheduleCaptureProbe: true)',
	'SurfaceBackgroundLighting.Midnight',
	'SurfaceBackgroundLighting.Eclipse',
	'BuildKesslerConstructionAndReport(scheduleCaptureProbe: true)',
	'InspectKesslerWorldAndReport(scheduleCaptureProbe: true)',
	'LIVE VALIDATION REQUEST CONSUMED',
	'LIVE VALIDATION REQUEST FAILED'
)

Require-SourceContract 'Tools/Request-LiveValidation.ps1' @(
	"[ValidateSet('conversion', 'vegetation', 'wastes-terrain', 'wastes-properties', 'material', 'grass', 'entity-scale', 'forest-background', 'forest-background-aerial', 'forest-background-night', 'forest-background-eclipse', 'desert-background', 'jungle-background', 'jungle-routing', 'snow-background', 'corruption-background', 'crimson-background', 'hallow-background', 'ocean-background', 'mushroom-background', 'underworld-background', 'kessler-construction', 'helix-construction', 'kessler-campus', 'kessler-world', 'forest-restoration-wastes', 'forest-restoration-mixed', 'forest-restoration-green')]",
	'ApogeanLiveValidation.request',
	'Set-Content -LiteralPath $requestPath'
)

Require-SourceContract 'Content/Backgrounds/RuinedBackgroundSelectionSystem.cs' @(
	'SurfaceRenderLabBiome',
	'ToggleSurfaceConceptRenderLab'
)

& (Join-Path $PSScriptRoot 'Test-BackgroundHdContracts.ps1')
$hdBackgroundContractsPassed = $?
if (-not $hdBackgroundContractsPassed) {
	$failures.Add('The surface HD background benchmark failed its native-detail renderer contracts.')
}
Require-SourceContract 'Content/Diagnostics/SurfaceBackgroundLabGallery.cs' @(
	'WastesSandCandidate',
	'ClearEverything',
	'Teleport'
)
Require-SourceContract 'Content/Diagnostics/KesslerConstructionGallery.cs' @(
	'KesslerBlock', 'KesslerTrim', 'KesslerFloor', 'KesslerGlass', 'KesslerBeam',
	'KesslerBulkheadWall', 'KesslerWindowWall', 'KesslerPowerArmorRack', 'KesslerWarBanner',
	'TileObjectData.GetTileData', 'WorldGen.PlaceObject', 'could not place'
)
Require-SourceContract 'Content/Diagnostics/KesslerWorldGallery.cs' @(
	'RequiredWorldName', 'GetLandmark(ApogeanLandmarkKind.KesslerCampus)',
	'ValidateRectangleType', 'ValidateRectangleEmpty', 'CompoundGen.UnsealCompound',
	'CompoundGen.ReArmCompound', 'KesslerPowerArmorRack', 'KesslerWarBanner',
	'InspectTerrainContact', 'supportedColumns < requiredSupport', 'capture.Inflate(2, 2)'
)
Require-SourceContract 'Content/Tiles/CorporateStructureTiles.cs' @(
	'Main.tileNoAttach[Type] = false'
)
Require-SourceContract 'Content/Tiles/CorporateFurnitureTiles.cs' @(
	'AnchorType.SolidTile | AnchorType.SolidWithTop'
)
Require-SourceContract 'Tools/New-KesslerConstructionSet.ps1' @(
	'Vanilla-GrayBrick-Tile.png', 'Vanilla-GrayBrick-Wall.png', 'New-WarBanner'
)
Require-PngContract 'Content/Tiles/KesslerWarBanner.png' 72 288 7

# Fixed structures need explicit terrain integration rather than a cleared air moat.
Require-SourceContract 'Content/Structures/CorporateTerrainIntegration.cs' @(
    'BlendGroundCampus', 'SealFoundation', 'RestoreTerrainShoulders',
    'PlaceHelixSurfaceDome'
)
Require-SourceContract 'Content/Structures/AuthoredStructureTemplate.cs' @(
    'TileObjectData.GetTileData', 'WorldGen.PlaceObject', 'command.Alternate',
    'could not place', 'Resolve the shell first'
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
Require-PngContract 'Content/Tiles/DeadForestTree_Branches.png' 84 126 3
Require-PngContract 'Content/Tiles/DeadForestTree_Tops.png' 246 82 3
Require-SourceContract 'Content/Tiles/DeadForestTreeRootGlobalTile.cs' @(
    'Reserved compatibility type', 'intentionally draws nothing'
)
Require-SourceContract 'Content/Tiles/DeadForestTree.cs' @(
    'GetTexture', 'GetBranchTextures', 'GetTopTextures',
    'Content/Tiles/DeadForestTree', 'Content/Tiles/DeadForestTree_Branches', 'Content/Tiles/DeadForestTree_Tops'
)
Require-SourceContract 'Content/Tiles/DeadTuft.cs' @('DrawYOffset = 4')
Require-SourceContract 'Content/Tiles/WastesGroundCoverTiles.cs' @('DrawYOffset = 4')
Require-SourceContract 'Content/World/RuinedSurfaceSystem.cs' @('MinimumDeadTreeSpacing = 12', 'ThinDeadForest')

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
Require-SourceContract 'Content/Structures/Blueprints/KesslerCampus.apstructure' @('size 152 72', 'surface 54')
Require-SourceContract 'Content/Structures/Blueprints/HelixCampus.apstructure' @('surface 45')
Require-SourceContract 'Common/WorldGeneration/WorldAtlasPlanner.cs' @(
	'54,', '28,', '152,', 'CenteredSurfaceFootprint(bounds, surfaceFootprintWidth)',
	'FindSurfaceBaseline(centerX, surfaceFootprintWidth, findSurface)'
)
Require-OrderedSourceContract 'Common/WorldGeneration/ApogeanWorldGenerationSystem.cs' @(
	'new PassLegacy("The Maw"', 'new PassLegacy("A World Picked Clean"',
	'"Apogean Compounds"', '"Apogean Ruins"'
)
Require-SourceContract 'Content/Structures/Blueprints/SentrixCampus.apstructure' @(
    'wall SentrixWindowWall 6 19 41 6', 'wall SentrixWindowWall 129 48 41 6',
    'wall SentrixWindowWall 6 77 41 6', 'wall SentrixWindowWall 129 106 41 6'
)

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
