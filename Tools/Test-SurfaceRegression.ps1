param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$failures = [System.Collections.Generic.List[string]]::new()

function Read-Source([string]$relativePath) {
    $path = Join-Path $Root $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        $failures.Add("Missing source: $relativePath")
        return ''
    }
    Get-Content -Raw -LiteralPath $path
}

function Require-Match([string]$relativePath, [string]$pattern, [string]$message) {
    $source = Read-Source $relativePath
    if ($source -notmatch $pattern) { $failures.Add($message) }
}

function Reject-Match([string]$relativePath, [string]$pattern, [string]$message) {
    $source = Read-Source $relativePath
    if ($source -match $pattern) { $failures.Add($message) }
}

function Reject-FileOrMatch([string]$relativePath, [string]$pattern, [string]$message) {
    $path = Join-Path $Root $relativePath
    if (-not (Test-Path -LiteralPath $path)) { return }
    $source = Get-Content -Raw -LiteralPath $path
    if ($source -match $pattern) { $failures.Add($message) }
}

function Require-File([string]$relativePath, [string]$message) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relativePath))) {
        $failures.Add($message)
    }
}

function Require-OpaqueTopRatio([string]$relativePath, [double]$maximumRatio) {
    $path = Join-Path $Root $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        $failures.Add("Missing background: $relativePath")
        return
    }

    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        $top = $bitmap.Height
        for ($y = 0; $y -lt $bitmap.Height -and $top -eq $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                if ($bitmap.GetPixel($x, $y).A -gt 0) {
                    $top = $y
                    break
                }
            }
        }
        $ratio = $top / [double]$bitmap.Height
        if ($ratio -gt $maximumRatio) {
            $failures.Add("$relativePath begins at y=$top ($([Math]::Round($ratio * 100, 1))% of its canvas); scenery is still anchored too low")
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

# Multi-cell plants sway each tile cell independently in Terraria. These Wastes props are brittle
# root debris, so they must remain one static silhouette when the player walks through them.
Reject-Match 'Content/Tiles/DeadTuft.cs' 'sways\s*:\s*true' 'DeadTuft still splits under per-cell wind sway'
Reject-Match 'Content/Tiles/WastesGroundCoverTiles.cs' 'sways\s*:\s*true' 'A multi-cell Wastes ground-cover object still uses per-cell wind sway'
Require-Match 'Content/Tiles/WastesGroundCoverTiles.cs' 'TileDrawing\.TileCounterType\.CustomNonSolid' 'Wastes debris is not drawn as one rigid multi-tile sprite'
Require-Match 'Content/Tiles/WastesGroundCoverTiles.cs' 'Texture \+ "_Whole"' 'Wastes debris has no padding-free whole-object atlas'
Require-Match 'Content/Tiles/WorldTerrainTiles.cs' 'Texture \+ "Roots"' 'Wastes grass has no terrain-seam root overlay'
Require-Match 'Content/Tiles/WorldTerrainTiles.cs' 'below\.TileType != ModContent\.TileType<WastesSoil>' 'Wastes grass root overlay is not limited to the grass/soil seam'
Require-File 'Content/Tiles/WastesGrassRoots.png' 'Wastes grass root-skirt atlas is missing'

# A whole-tree overlay cannot preserve Terraria chopping semantics: removing one trunk tile makes
# the remaining tree rescale. The visible art must come from ModTree's segmented native atlases.
Reject-Match 'Content/Tiles/DeadForestTree.cs' 'DeadForestTreeHidden' 'DeadForestTree still hides the segmented native tree atlases'
Require-Match 'Content/Tiles/DeadForestTree.cs' 'Content/Tiles/DeadForestTree"' 'DeadForestTree does not expose its visible trunk atlas'
Require-Match 'Content/Tiles/DeadForestTree.cs' 'SetTreeFoliageSettings' 'DeadForestTree does not deterministically vary crowns and branches'
Reject-FileOrMatch 'Content/Tiles/DeadForestTreeOverlaySystem.cs' 'drawHeight\s*=|trunkTiles|SpriteBatch\.Draw' 'Whole-tree scaling overlay is still active and will shrink after chopping'
Require-Match 'Content/Tiles/DeadForestTreeRootGlobalTile.cs' 'variant \* 48, 0, 48, 32' 'Dead tree root flare is not bounded to a fixed 48x32 base sprite'
Reject-Match 'Content/Tiles/DeadForestTreeRootGlobalTile.cs' 'trunkOverlay|sourceRow|distanceFromRoot' 'Dead tree still has a custom whole-trunk renderer instead of Terraria-native trunk cells'
Require-Match 'Tools/New-WastesVegetation.ps1' 'Vanilla-ForestTree-Tops\.png' 'Dead tree crowns are not derived from Terraria native tree topology'
Require-Match 'Tools/New-WastesVegetation.ps1' 'Vanilla-ForestTree-Branches\.png' 'Dead tree branches are not derived from Terraria native tree topology'
Require-Match 'Content/Diagnostics/VegetationLabGallery.cs' 'ValidateMidTrunkChop' 'Vegetation fixture does not exercise native mid-trunk chopping'
Require-Match 'Content/Diagnostics/VegetationLabGallery.cs' 'unsupported\.HasTile.*TileID\.Trees' 'Vegetation fixture does not reject a floating canopy after chopping'

# Global background overrides are applied after scene-effect arbitration, while CaptureBiome reads
# CurrentSceneEffect. The validation panorama must resolve the same ruined slot explicitly and must
# sanitize the ModBiome water style before the renderer indexes its liquid texture array.
Require-Match 'Content/Backgrounds/ApogeanSurfaceBackgroundStyles.cs' 'ResolveRuinedSurfaceStyle' 'Ruined surface routing has no shared resolver for live and capture rendering'
Require-Match 'Content/Diagnostics/TileLabPlayer.cs' 'new\(captureBackground, captureWater, sceneEffect\.tileColorStyle\)' 'Capture probe still relies on a vanilla CurrentSceneEffect background slot'
Require-Match 'Content/Diagnostics/TileLabPlayer.cs' 'sceneWater\s*>=\s*0.*Main\.maxLiquidTypes' 'Capture probe does not reject invalid negative/out-of-range water styles'

# Wide leafless trees need deliberate thinning after vanilla forest conversion, otherwise adjacent
# roots become one unreadable copied grove.
Require-Match 'Content/World/RuinedSurfaceSystem.cs' 'MinimumDeadTreeSpacing\s*=\s*1[1-3]' 'Wastes tree conversion has no explicit 11-13 tile minimum spacing'
Require-Match 'Content/World/RuinedSurfaceSystem.cs' 'ThinDeadForest' 'Wastes tree conversion does not thin inherited vanilla tree clusters'

# Keep authored horizons visible at ordinary ground-level camera height. This catches source/export
# regressions with most of a layer left as transparent sky above bottom-anchored scenery.
Require-OpaqueTopRatio 'Content/Backgrounds/Forest/V0_Far.png' 0.28
Require-OpaqueTopRatio 'Content/Backgrounds/Forest/V0_Mid.png' 0.52
Require-OpaqueTopRatio 'Content/Backgrounds/Forest/V0_Close.png' 0.32

# The Kessler campus must read as a compact fortified compound, not detached graph-paper
# rectangles. Semantic blueprint markers make this cheaply verifiable before an in-engine capture.
Require-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' '# Connected perimeter wall' 'Kessler campus lacks a connected military perimeter'
Require-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' '# Compact headquarters' 'Kessler campus lacks a compact authored headquarters'
Require-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' '# Public frontage' 'Kessler campus lacks a day-one public frontage'
Require-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' 'object KesslerWarBanner 42 9 4 4' 'Kessler campus lacks its western animated war standard'
Require-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' 'object KesslerWarBanner 106 9 4 4' 'Kessler campus lacks its eastern animated war standard'
Reject-Match 'Content/Structures/Blueprints/KesslerCampus.apstructure' 'frame\s+KesslerBlock\s+32\s+20\s+144\s+54' 'Kessler campus still uses the giant rectangular headquarters shell seen in the failed render'

if ($failures.Count -gt 0) {
    Write-Host "SURFACE REGRESSION: FAIL ($($failures.Count) problems)" -ForegroundColor Red
    foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
    exit 1
}

Write-Host 'SURFACE REGRESSION: PASS' -ForegroundColor Green
