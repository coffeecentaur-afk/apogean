param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure([string]$message) {
    $failures.Add($message)
}

function Read-Source([string]$relativePath) {
    $path = Join-Path $Root $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing source: $relativePath"
        return ''
    }
    Get-Content -Raw -LiteralPath $path
}

function Get-SheetMetrics([string]$relativePath, [int]$frameCount = 1) {
    $path = Join-Path $Root $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing sprite sheet: $relativePath"
        return $null
    }

    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        if ($frameCount -lt 1 -or $bitmap.Height % $frameCount -ne 0) {
            Add-Failure "$relativePath height $($bitmap.Height) is not divisible by its $frameCount frames"
            return $null
        }

        $frameHeight = [int]($bitmap.Height / $frameCount)
        $frames = @()
        for ($frame = 0; $frame -lt $frameCount; $frame++) {
            $minX = $bitmap.Width
            $minY = $frameHeight
            $maxX = -1
            $maxY = -1
            $opaque = 0
            for ($localY = 0; $localY -lt $frameHeight; $localY++) {
                $y = $frame * $frameHeight + $localY
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    if ($bitmap.GetPixel($x, $y).A -eq 0) { continue }
                    $opaque++
                    $minX = [Math]::Min($minX, $x)
                    $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $localY)
                    $maxY = [Math]::Max($maxY, $localY)
                }
            }
            $frames += [pscustomobject]@{
                Width = if ($maxX -ge 0) { $maxX - $minX + 1 } else { 0 }
                Height = if ($maxY -ge 0) { $maxY - $minY + 1 } else { 0 }
                Opaque = $opaque
            }
        }

        [pscustomobject]@{
            Width = $bitmap.Width
            Height = $bitmap.Height
            FrameHeight = $frameHeight
            Frames = $frames
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

# A Wastes tree is a Terraria forest tree with dead bark and no foliage. Crowns and
# wide custom root flares are specifically rejected because they caused the copied,
# overlapping grove visible in the September 4 playtest.
$treeTop = Get-SheetMetrics 'Content/Tiles/DeadForestTree_Tops.png'
if ($null -ne $treeTop -and ($treeTop.Width -ne 246 -or $treeTop.Height -ne 82)) {
	Add-Failure "DeadForestTree_Tops is $($treeTop.Width)x$($treeTop.Height); Terraria's three-crown contract is 246x82"
}
if ($null -ne $treeTop -and ($treeTop.Frames | Measure-Object Opaque -Sum).Sum -gt 6100) {
    Add-Failure 'DeadForestTree_Tops still contains a leaf-crown-sized opaque mass instead of sparse woody forks'
}
$treeBranches = Get-SheetMetrics 'Content/Tiles/DeadForestTree_Branches.png'
if ($null -ne $treeBranches -and ($treeBranches.Width -ne 84 -or $treeBranches.Height -ne 126)) {
	Add-Failure "DeadForestTree_Branches is $($treeBranches.Width)x$($treeBranches.Height); Terraria's paired-branch contract is 84x126"
}
if ($null -ne $treeBranches -and ($treeBranches.Frames | Measure-Object Opaque -Sum).Sum -gt 3000) {
    Add-Failure 'DeadForestTree_Branches still contains foliage-sized opaque masses instead of bare limbs'
}
$rootOverlay = Read-Source 'Content/Tiles/DeadForestTreeRootGlobalTile.cs'
if ($rootOverlay -match 'SpriteBatch\.Draw|spriteBatch\.Draw') {
    Add-Failure 'DeadForestTree still adds a wide custom root overlay instead of retaining a vanilla-width base'
}

# The seam repair must explicitly support slopes; the existing solid-only skirt leaves
# camera-background pixels visible between sloped grass and Wastes soil.
$terrainSource = Read-Source 'Content/Tiles/WorldTerrainTiles.cs'
if ($terrainSource -notmatch 'NeedsGrassFraming\[Type\]\s*=\s*true') {
    Add-Failure 'WastesGrass is not registered for Terraria grass framing'
}
if ($terrainSource -notmatch 'NeedsGrassFramingDirt\[Type\]\s*=\s*ModContent\.TileType<WastesSoil>') {
    Add-Failure 'WastesGrass does not identify WastesSoil as its framing substrate'
}

# Native-detail panoramas need a camera-travel coverage path, not only one image draw
# anchored around worldSurface. This assertion is paired with an aerial live fixture.
$hdRenderer = Read-Source 'Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs'
if ($hdRenderer -notmatch 'DrawVerticallyCoveredLayer') {
    Add-Failure 'HD surface renderer does not guarantee vertical coverage during flight'
}

# Prevent the Forest fallback from silently replacing Jungle while validating biome routing.
$selectionSource = Read-Source 'Content/Backgrounds/RuinedBackgroundSelectionSystem.cs'
if ($selectionSource -notmatch 'player\.ZoneJungle.*RuinedBackgroundBiome\.Jungle') {
    Add-Failure 'Jungle surface background routing is missing'
}

# These are gameplay-scale minimums, not arbitrary texture enlargement. Each animation
# frame must have enough readable silhouette to exceed ambient birds and debris.
$mawling = Get-SheetMetrics 'Content/NPCs/Engraft/Mawling.png' 4
if ($null -ne $mawling) {
    if ($mawling.Width -lt 28 -or $mawling.FrameHeight -lt 24) {
        Add-Failure "Mawling frame canvas is only $($mawling.Width)x$($mawling.FrameHeight); require at least 28x24"
    }
    if (@($mawling.Frames | Where-Object { $_.Width -lt 16 -or $_.Height -lt 12 }).Count -gt 0) {
        Add-Failure 'Mawling visible silhouette remains smaller than a readable Terraria flying enemy'
    }
}

$hound = Get-SheetMetrics 'Content/NPCs/Engraft/GraftHound.png' 4
if ($null -ne $hound) {
    if ($hound.Width -lt 48 -or $hound.FrameHeight -lt 24) {
        Add-Failure "GraftHound frame canvas is only $($hound.Width)x$($hound.FrameHeight); require at least 48x24"
    }
    if (@($hound.Frames | Where-Object { $_.Width -lt 32 -or $_.Height -lt 18 }).Count -gt 0) {
        Add-Failure 'GraftHound visible silhouette remains smaller than a Terraria hound-sized enemy'
    }
}

$fibre = Get-SheetMetrics 'Content/Items/Materials/MawFibre.png'
if ($null -ne $fibre -and ($fibre.Frames[0].Width -lt 14 -or $fibre.Frames[0].Height -lt 14)) {
    Add-Failure 'MawFibre inventory silhouette is smaller than 14x14 pixels'
}

# The reusable authoring pipeline is itself a product requirement. These project-side
# wrappers ensure future blocks and entities cannot bypass the same checks.
foreach ($required in @(
    'Tools/Test-TModLoaderAtlas.ps1',
    'Tools/New-TModLoaderPaletteAtlas.ps1',
    'Tools/Test-TModLoaderEntitySheet.ps1'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $required))) {
        Add-Failure "Missing reusable visual-content tool: $required"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "REPORTED VISUAL REGRESSIONS: FAIL ($($failures.Count) problems)" -ForegroundColor Red
    foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
    exit 1
}

Write-Host 'REPORTED VISUAL REGRESSIONS: PASS' -ForegroundColor Green
