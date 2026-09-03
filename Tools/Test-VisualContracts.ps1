Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure([string]$message) {
    $failures.Add($message)
}

function Test-Layer {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Failure "Missing layered background: $Path"
        return
    }

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            Add-Failure "Wrong layer dimensions: $Path is $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
        }

        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            if ($bitmap.GetPixel(0, $y).ToArgb() -ne $bitmap.GetPixel($bitmap.Width - 1, $y).ToArgb()) {
                Add-Failure "Layer edges do not tile: $Path at row $y"
                break
            }
        }

        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            if ($bitmap.GetPixel($x, $bitmap.Height - 1).A -ne 255) {
                Add-Failure "Layer leaves a bottom gap: $Path at column $x"
                break
            }
        }

        if ($bitmap.GetPixel([int]($bitmap.Width / 2), 0).A -ne 0) {
            Add-Failure "Layer bakes over Terraria's sky instead of exposing it: $Path"
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-PixelSheet {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight,
        [int]$MaximumOpaqueColors
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Failure "Missing pixel sheet: $Path"
        return
    }

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            Add-Failure "Wrong pixel-sheet dimensions: $Path is $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
        }

        $colors = [System.Collections.Generic.HashSet[int]]::new()
        $hasSoftAlpha = $false
        $opaquePixels = 0
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $hasSoftAlpha = $true }
                if ($pixel.A -eq 255) {
                    [void]$colors.Add($pixel.ToArgb())
                    $opaquePixels++
                }
            }
        }

        if ($opaquePixels -eq 0) { Add-Failure "Pixel sheet is empty: $Path" }
        if ($colors.Count -gt $MaximumOpaqueColors) {
            Add-Failure "Pixel sheet uses $($colors.Count) opaque colors; maximum is $MaximumOpaqueColors`: $Path"
        }
        if ($hasSoftAlpha) { Add-Failure "Pixel sheet contains soft alpha: $Path" }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-TransparentSheet {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Failure "Missing transparent renderer sheet: $Path"
        return
    }

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            Add-Failure "Wrong transparent-sheet dimensions: $Path is $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
        }
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                if ($bitmap.GetPixel($x, $y).A -ne 0) {
                    Add-Failure "Transparent renderer sheet leaks visible pixels: $Path at $x,$y"
                    return
                }
            }
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

$layerSpecs = @{
    'Far' = @(1024, 408)
    'Mid' = @(1024, 600)
    'Close' = @(952, 480)
}

foreach ($biome in @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption', 'Crimson', 'Hallow', 'Ocean', 'Mushroom', 'Underworld', 'Engraft')) {
    foreach ($variant in 0..1) {
        foreach ($layer in $layerSpecs.Keys) {
            $spec = $layerSpecs[$layer]
            Test-Layer -Path (Join-Path $projectRoot "Content/Backgrounds/$biome/V$($variant)_$layer.png") -ExpectedWidth $spec[0] -ExpectedHeight $spec[1]
        }

        foreach ($index in 0..3) {
            $undergroundPath = Join-Path $projectRoot "Content/Backgrounds/$biome/Underground/V$($variant)_$index.png"
            if (-not (Test-Path -LiteralPath $undergroundPath)) {
                Add-Failure "Missing underground background: $undergroundPath"
                continue
            }
            $expectedHeight = if ($index -eq 0 -or $index -eq 2) { 16 } else { 96 }
            $bitmap = [System.Drawing.Bitmap]::new($undergroundPath)
            try {
                if ($bitmap.Width -ne 160 -or $bitmap.Height -ne $expectedHeight) {
                    Add-Failure "Wrong underground dimensions: $undergroundPath"
                }
                for ($y = 0; $y -lt $bitmap.Height; $y++) {
                    for ($x = 0; $x -lt 32; $x++) {
                        if ($bitmap.GetPixel($x, $y).ToArgb() -ne $bitmap.GetPixel(128 + $x, $y).ToArgb()) {
                            Add-Failure "Underground wrap strip differs: $undergroundPath"
                            $y = $bitmap.Height
                            break
                        }
                    }
                }
            }
            finally { $bitmap.Dispose() }
        }
    }
}

foreach ($approvedBiome in @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption')) {
    foreach ($layer in $layerSpecs.Keys) {
        $spec = $layerSpecs[$layer]
        $candidate = Join-Path $projectRoot "Content/Backgrounds/Diagnostics/$($approvedBiome)ConceptV0_$layer.png"
        $production = Join-Path $projectRoot "Content/Backgrounds/$approvedBiome/V0_$layer.png"
        Test-Layer -Path $candidate -ExpectedWidth $spec[0] -ExpectedHeight $spec[1]
        Test-PixelSheet -Path $candidate -ExpectedWidth $spec[0] -ExpectedHeight $spec[1] -MaximumOpaqueColors 10
        if ((Get-FileHash -Algorithm SHA256 -LiteralPath $candidate).Hash -ne
            (Get-FileHash -Algorithm SHA256 -LiteralPath $production).Hash) {
            Add-Failure "Production $approvedBiome V0 $layer differs from its renderer-approved candidate"
        }
    }
}

foreach ($index in 0..3) {
    $height = if ($index -eq 0 -or $index -eq 2) { 16 } else { 96 }
    $candidate = Join-Path $projectRoot "Content/Backgrounds/Diagnostics/ForestUndergroundConceptV0_$index.png"
    $production = Join-Path $projectRoot "Content/Backgrounds/Forest/Underground/V0_$index.png"
    Test-PixelSheet -Path $candidate -ExpectedWidth 160 -ExpectedHeight $height -MaximumOpaqueColors 10
    if (Test-Path -LiteralPath $candidate) {
        $bitmap = [System.Drawing.Bitmap]::new($candidate)
        try {
            $undergroundFailed = $false
            for ($y = 0; $y -lt $bitmap.Height; $y++) {
                if ($bitmap.GetPixel(0, $y).ToArgb() -ne $bitmap.GetPixel(127, $y).ToArgb()) {
                    Add-Failure "Underground candidate core seam differs: $candidate"
                    $undergroundFailed = $true
                    break
                }
                for ($x = 0; $x -lt 32; $x++) {
                    if ($bitmap.GetPixel($x, $y).ToArgb() -ne $bitmap.GetPixel(128 + $x, $y).ToArgb()) {
                        Add-Failure "Underground candidate wrap strip differs: $candidate"
                        $undergroundFailed = $true
                        break
                    }
                }
                if ($undergroundFailed) { break }
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    if ($bitmap.GetPixel($x, $y).A -ne 255) {
                        Add-Failure "Underground candidate is not a fully opaque cave material: $candidate"
                        $undergroundFailed = $true
                        break
                    }
                }
                if ($undergroundFailed) { break }
            }
        }
        finally { $bitmap.Dispose() }
    }
    if ((Get-FileHash -Algorithm SHA256 -LiteralPath $candidate).Hash -ne
        (Get-FileHash -Algorithm SHA256 -LiteralPath $production).Hash) {
        Add-Failure "Production Forest underground V0 index $index differs from its renderer-approved candidate"
    }
}

$surfaceBackgroundSource = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'Content/Backgrounds/ApogeanSurfaceBackgroundStyles.cs')
if ($surfaceBackgroundSource -match 'fades\[i\]\s*=|fades\[i\]\s*\+=|fades\[i\]\s*-=') {
    Add-Failure 'Surface style manually advances the installed runtime front fade and can desynchronize the close layer'
}
if ($surfaceBackgroundSource -notmatch 'GetModSurfaceBackgroundStyle\(style\)\s*!=\s*null') {
    Add-Failure 'Global surface replacement does not preserve third-party background style slots'
}
if ($surfaceBackgroundSource -notmatch 'GetModUndergroundBackgroundStyle\(style\)\s*!=\s*null') {
    Add-Failure 'Global underground replacement does not preserve third-party background style slots'
}
if ($surfaceBackgroundSource -notmatch 'player\.ZoneUnderworldHeight') {
    Add-Failure 'Ordinary cave background routing still attempts to replace Terraria''s separate Underworld panorama'
}

foreach ($sprite in @('RendHook', 'AmberSiphon', 'SinewBow', 'MawEffigy')) {
    $path = Join-Path $projectRoot "Content/Items/Weapons/$sprite.png"
    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        $colors = [System.Collections.Generic.HashSet[int]]::new()
        $hasSoftAlpha = $false
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $hasSoftAlpha = $true }
                if ($pixel.A -eq 255) { [void]$colors.Add($pixel.ToArgb()) }
            }
        }
        if ($colors.Count -gt 10) { Add-Failure "$sprite uses $($colors.Count) opaque colors; native Terraria-scale target is at most 10" }
        if ($hasSoftAlpha) { Add-Failure "$sprite contains soft alpha instead of hard pixel edges: $sprite" }
    }
    finally {
        $bitmap.Dispose()
    }
}

foreach ($tileSheet in @(
    'Mawstone',
    'OssuaryBone',
    'KesslerPlating',
    'HelixContainmentPanel',
    'SentrixPanel',
    'KesslerRuinBlock',
    'HelixRuinBlock',
    'SentrixRuinBlock',
    'PrewarConcrete',
    'MawResearchBlock'
)) {
    Test-PixelSheet -Path (Join-Path $projectRoot "Content/Tiles/$tileSheet.png") -ExpectedWidth 288 -ExpectedHeight 270 -MaximumOpaqueColors 12
}

foreach ($fixtureSheet in @('KesslerPowerArmorRack', 'HelixSymbioteTank', 'SentrixHologramCore')) {
    Test-PixelSheet -Path (Join-Path $projectRoot "Content/Tiles/$fixtureSheet.png") -ExpectedWidth 54 -ExpectedHeight 288 -MaximumOpaqueColors 16
}

$corporateFurniture = @{
    'Platform' = @(486, 18)
    'Chair' = @(36, 40)
    'Table' = @(54, 36)
    'Workbench' = @(36, 20)
    'Light' = @(18, 18)
    'Console' = @(54, 36)
    'Locker' = @(36, 54)
}
foreach ($faction in @('Kessler', 'Helix', 'Sentrix')) {
    foreach ($family in $corporateFurniture.Keys) {
        $dimensions = $corporateFurniture[$family]
        Test-PixelSheet -Path (Join-Path $projectRoot "Content/Tiles/$faction$family.png") -ExpectedWidth $dimensions[0] -ExpectedHeight $dimensions[1] -MaximumOpaqueColors 12
    }
}
foreach ($wall in @('KesslerBulkheadWall','KesslerWindowWall','HelixLaboratoryWall','HelixObservationWall','SentrixDataWall','SentrixWindowWall')) {
    Test-PixelSheet -Path (Join-Path $projectRoot "Content/Walls/$wall.png") -ExpectedWidth 468 -ExpectedHeight 180 -MaximumOpaqueColors 12
}

Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/DeadForestTree.png') -ExpectedWidth 176 -ExpectedHeight 264 -MaximumOpaqueColors 12
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/DeadForestTree_Branches.png') -ExpectedWidth 84 -ExpectedHeight 126 -MaximumOpaqueColors 12
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/DeadForestTree_Tops.png') -ExpectedWidth 246 -ExpectedHeight 82 -MaximumOpaqueColors 12
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/DeadForestTreeRoots.png') -ExpectedWidth 144 -ExpectedHeight 32 -MaximumOpaqueColors 12
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/DeadTuft.png') -ExpectedWidth 144 -ExpectedHeight 18 -MaximumOpaqueColors 7
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/WastesBristle.png') -ExpectedWidth 108 -ExpectedHeight 54 -MaximumOpaqueColors 7
Test-PixelSheet -Path (Join-Path $projectRoot 'Content/Tiles/WastesRootShrub.png') -ExpectedWidth 162 -ExpectedHeight 36 -MaximumOpaqueColors 7

foreach ($blueprint in @('KesslerCampus','HelixCampus','SentrixCampus')) {
    $path = Join-Path $projectRoot "Content/Structures/Blueprints/$blueprint.apstructure"
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing authored Campus blueprint: $blueprint"
        continue
    }
    $source = Get-Content -Raw -LiteralPath $path
    if ($source -notmatch '(?m)^size\s+\d+\s+\d+\s*$') { Add-Failure "$blueprint has no fixed size" }
    if ($source -notmatch '(?m)^entrance\s+\d+\s+\d+\s+\d+\s+\d+\s*$') { Add-Failure "$blueprint has no semantic entrance" }
    if (($source | Select-String -Pattern '(?m)^object\s+' -AllMatches).Matches.Count -lt 16) { Add-Failure "$blueprint has too little authored furniture" }
    if (($source | Select-String -Pattern '(?m)^wall\s+' -AllMatches).Matches.Count -lt 2) { Add-Failure "$blueprint has no complete wall program" }
}

$compoundSource = Get-Content -Raw (Join-Path $projectRoot 'Content/Structures/CompoundGen.cs')
if ($compoundSource -notmatch 'CorporateCampusBlueprints\.Place') { Add-Failure 'Campus generation does not place immutable authored blueprints' }
if ($compoundSource -match 'PlaceOutline|PlaceHorizontalRun|PlaceTower') { Add-Failure 'Campus generation still contains procedural shell construction' }

$tetherSource = Get-Content -Raw (Join-Path $projectRoot 'Content/Projectiles/UmbilicalTether.cs')
if ($tetherSource -match 'DrawSegment') { Add-Failure 'Umbilical tether still uses long scaled line primitives' }
if ($tetherSource -notmatch 'CordPixelSize\s*=\s*3') { Add-Failure 'Umbilical tether does not enforce a three-pixel visual thickness' }

$effigySource = Get-Content -Raw (Join-Path $projectRoot 'Content/Items/Weapons/MawEffigy.cs')
if ($effigySource -notmatch 'MawEffigyBuff') { Add-Failure 'Maw Effigy does not register a removable summon buff' }
if (-not (Test-Path (Join-Path $projectRoot 'Content/Buffs/MawEffigyBuff.cs'))) { Add-Failure 'Maw Effigy buff implementation is missing' }
if (-not (Test-Path (Join-Path $projectRoot 'Content/Tiles/DeadGrass.cs'))) { Add-Failure 'Ruined surface still lacks a real dead-grass tile' }

if ($failures.Count -gt 0) {
    Write-Host "VISUAL CONTRACT: FAIL ($($failures.Count) problems)" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host 'VISUAL CONTRACT: PASS' -ForegroundColor Green
