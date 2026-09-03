param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure([string]$message) {
    $failures.Add($message)
}

function Test-NormalizedCopy([string]$sourceRelativePath, [string]$outputRelativePath, [bool]$usesMagentaKey) {
    $sourcePath = Join-Path $Root $sourceRelativePath
    $outputPath = Join-Path $Root $outputRelativePath
    if (-not (Test-Path -LiteralPath $outputPath)) {
        Add-Failure "Missing output asset: $outputRelativePath"
        return
    }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($outputPath)
    try {
        if ($source.Width -ne $output.Width -or $source.Height -ne $output.Height) {
            Add-Failure "Dimension mismatch: $outputRelativePath is $($output.Width)x$($output.Height), expected $($source.Width)x$($source.Height)"
            return
        }

        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $expected = $source.GetPixel($x, $y)
                if ($usesMagentaKey -and $expected.R -gt 220 -and $expected.B -gt 220 -and $expected.G -lt 160) {
                    $expected = [System.Drawing.Color]::Transparent
                }

                $actual = $output.GetPixel($x, $y)
                if ($expected.A -ne $actual.A -or ($expected.A -gt 0 -and $expected.ToArgb() -ne $actual.ToArgb())) {
                    Add-Failure "Pixel topology differs at ($x,$y): $outputRelativePath"
                    return
                }
            }
        }
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

function Test-CandidateAtlas(
    [string]$maskRelativePath,
    [string]$candidateRelativePath,
    [int]$maximumOpaqueColors,
    [int]$expectedOpaquePixels
) {
    $maskPath = Join-Path $Root $maskRelativePath
    $candidatePath = Join-Path $Root $candidateRelativePath
    if (-not (Test-Path -LiteralPath $candidatePath)) {
        Add-Failure "Missing candidate asset: $candidateRelativePath"
        return
    }

    $mask = [System.Drawing.Bitmap]::new($maskPath)
    $candidate = [System.Drawing.Bitmap]::new($candidatePath)
    try {
        if ($mask.Width -ne $candidate.Width -or $mask.Height -ne $candidate.Height) {
            Add-Failure "Candidate dimensions are $($candidate.Width)x$($candidate.Height): $candidateRelativePath; expected $($mask.Width)x$($mask.Height)"
            return
        }

        $opaqueColors = [System.Collections.Generic.HashSet[int]]::new()
        $opaquePixels = 0
        for ($y = 0; $y -lt $candidate.Height; $y++) {
            for ($x = 0; $x -lt $candidate.Width; $x++) {
                $pixel = $candidate.GetPixel($x, $y)

                if ($pixel.A -ne 0 -and $pixel.A -ne 255) {
                    Add-Failure "Semi-transparent pixel at ($x,$y): $candidateRelativePath"
                    return
                }
                if ($pixel.A -gt 0) {
					$opaquePixels++
                    [void]$opaqueColors.Add($pixel.ToArgb())
                    if ($pixel.R -gt 220 -and $pixel.B -gt 220 -and $pixel.G -lt 160) {
                        Add-Failure "Magenta-key pixel leaked into candidate at ($x,$y): $candidateRelativePath"
                        return
                    }
                }
            }
        }

        if ($opaqueColors.Count -gt $maximumOpaqueColors) {
            Add-Failure "Candidate uses $($opaqueColors.Count) opaque colors: $candidateRelativePath; maximum is $maximumOpaqueColors"
        }
        if ($opaquePixels -ne $expectedOpaquePixels) {
            Add-Failure "Candidate has $opaquePixels opaque pixels: $candidateRelativePath; expected exported-atlas topology count $expectedOpaquePixels"
        }
    }
    finally {
        $mask.Dispose()
        $candidate.Dispose()
    }
}

function Test-FixedAtlasContract(
    [string]$candidateRelativePath,
    [int]$expectedWidth,
    [int]$expectedHeight,
    [int]$maximumOpaqueColors,
    [int]$expectedOpaquePixels,
    [string]$expectedAlphaHash
) {
    $candidatePath = Join-Path $Root $candidateRelativePath
    if (-not (Test-Path -LiteralPath $candidatePath)) {
        Add-Failure "Missing candidate asset: $candidateRelativePath"
        return
    }

    $candidate = [System.Drawing.Bitmap]::new($candidatePath)
    try {
        if ($candidate.Width -ne $expectedWidth -or $candidate.Height -ne $expectedHeight) {
            Add-Failure "Candidate dimensions are $($candidate.Width)x$($candidate.Height): $candidateRelativePath; expected ${expectedWidth}x${expectedHeight}"
            return
        }

        $opaqueColors = [System.Collections.Generic.HashSet[int]]::new()
        $alphaBytes = [byte[]]::new($candidate.Width * $candidate.Height)
        $opaquePixels = 0
        $index = 0
        for ($y = 0; $y -lt $candidate.Height; $y++) {
            for ($x = 0; $x -lt $candidate.Width; $x++) {
                $pixel = $candidate.GetPixel($x, $y)
                $alphaBytes[$index++] = $pixel.A
                if ($pixel.A -ne 0 -and $pixel.A -ne 255) {
                    Add-Failure "Semi-transparent pixel at ($x,$y): $candidateRelativePath"
                    return
                }
                if ($pixel.A -gt 0) {
                    $opaquePixels++
                    [void]$opaqueColors.Add($pixel.ToArgb())
                    if ($pixel.R -gt 220 -and $pixel.B -gt 220 -and $pixel.G -lt 160) {
                        Add-Failure "Magenta-key pixel leaked into candidate at ($x,$y): $candidateRelativePath"
                        return
                    }
                }
            }
        }

        if ($opaqueColors.Count -gt $maximumOpaqueColors) {
            Add-Failure "Candidate uses $($opaqueColors.Count) opaque colors: $candidateRelativePath; maximum is $maximumOpaqueColors"
        }
        if ($opaquePixels -ne $expectedOpaquePixels) {
            Add-Failure "Candidate has $opaquePixels opaque pixels: $candidateRelativePath; expected $expectedOpaquePixels"
        }

        $alphaHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($alphaBytes))
        if ($alphaHash -ne $expectedAlphaHash) {
            Add-Failure "Candidate alpha topology changed: $candidateRelativePath; got $alphaHash"
        }
    }
    finally {
        $candidate.Dispose()
    }
}

function Test-AlphaTopology(
    [string]$sourceRelativePath,
    [string]$candidateRelativePath,
    [int]$maximumOpaqueColors
) {
    $sourcePath = Join-Path $Root $sourceRelativePath
    $candidatePath = Join-Path $Root $candidateRelativePath
    if (-not (Test-Path -LiteralPath $sourcePath) -or -not (Test-Path -LiteralPath $candidatePath)) {
        Add-Failure "Missing alpha-topology pair: $sourceRelativePath -> $candidateRelativePath"
        return
    }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $candidate = [System.Drawing.Bitmap]::new($candidatePath)
    try {
        if ($source.Width -ne $candidate.Width -or $source.Height -ne $candidate.Height) {
            Add-Failure "Alpha-topology dimensions differ: $candidateRelativePath"
            return
        }
        $colors = [System.Collections.Generic.HashSet[int]]::new()
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $expectedAlpha = $source.GetPixel($x, $y).A
                $pixel = $candidate.GetPixel($x, $y)
                if ($pixel.A -ne $expectedAlpha) {
                    Add-Failure "Alpha topology differs at ($x,$y): $candidateRelativePath"
                    return
                }
                if ($pixel.A -ne 0 -and $pixel.A -ne 255) {
                    Add-Failure "Semi-transparent pixel at ($x,$y): $candidateRelativePath"
                    return
                }
                if ($pixel.A -gt 0) { [void]$colors.Add($pixel.ToArgb()) }
            }
        }
        if ($colors.Count -gt $maximumOpaqueColors) {
            Add-Failure "$candidateRelativePath uses $($colors.Count) colors; maximum is $maximumOpaqueColors"
        }
    }
    finally {
        $source.Dispose()
        $candidate.Dispose()
    }
}

Test-NormalizedCopy 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/TileLabBlock.png' $true
Test-NormalizedCopy 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/TileLabWall.png' $false
Test-CandidateAtlas 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/WastesSoilCandidate.png' 5 44104
Test-CandidateAtlas 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/WastesDirtWallCandidate.png' 5 28864
Test-FixedAtlasContract 'Content/Tiles/Diagnostics/WastesGrassCandidate.png' 288 1980 11 97004 '2FFA92E98A879378BAAEC342A8ABAC7DF12969ED3C5A152655B27E53261455B1'
Test-FixedAtlasContract 'Content/Walls/Diagnostics/WastesGrassWallCandidate.png' 468 180 5 26776 '56885CF9A064ACF22E00E30531BDB3320CC4B27D6F6B9A661F5AC5ABC4C07E73'
Test-NormalizedCopy 'Content/Tiles/Diagnostics/WastesSoilCandidate.png' 'Content/Tiles/WastesSoil.png' $false
Test-NormalizedCopy 'Content/Walls/Diagnostics/WastesDirtWallCandidate.png' 'Content/Walls/WastesDirtWallUnsafe.png' $false
Test-NormalizedCopy 'Content/Tiles/Diagnostics/WastesGrassCandidate.png' 'Content/Tiles/WastesGrass.png' $false
Test-NormalizedCopy 'Content/Walls/Diagnostics/WastesGrassWallCandidate.png' 'Content/Walls/WastesGrassWallUnsafe.png' $false
$terrainFamilyContracts = @(
	@('Stone', 10, 44104, '0B36C6ACA6ACAB6A45422FB240F6527516C8B11C66A34404EACCB57609902EE6'),
	@('Sand', 11, 44192, '71DC4A4E79B3E38A5BA27D771AD80C04CF6DE059FE7979EC958913539F928958'),
	@('Ice', 11, 44036, 'DD70F89A74332CB985E3396A8827A36039B9D9890B86C7F20E6DB73E9D071360'),
	@('Snow', 9, 44048, 'E5515B1CAE899864E7E53C22C12EA6F6E2790C9A69E45D0E7096F773C74379DE'),
	@('Mud', 10, 44104, '0B36C6ACA6ACAB6A45422FB240F6527516C8B11C66A34404EACCB57609902EE6')
)
$wallFamilyContracts = @(
	@('Stone', 6, 28864, '7873B1EA474A86161ECE424638475FC6644601B0A456A9E7C0AE2B9055305B6F'),
	@('Sand', 5, 28864, '7873B1EA474A86161ECE424638475FC6644601B0A456A9E7C0AE2B9055305B6F'),
	@('Ice', 6, 30528, 'B858C0628306DEEE27994AAECBDE924B587FA999342DC7F401713DC0D1F2B1F3'),
	@('Snow', 5, 28864, '7873B1EA474A86161ECE424638475FC6644601B0A456A9E7C0AE2B9055305B6F'),
	@('Mud', 6, 28864, '7873B1EA474A86161ECE424638475FC6644601B0A456A9E7C0AE2B9055305B6F')
)
foreach ($contract in $terrainFamilyContracts) {
	$name = $contract[0]
	Test-FixedAtlasContract "Content/Tiles/Diagnostics/Wastes${name}Candidate.png" 288 270 $contract[1] $contract[2] $contract[3]
	Test-NormalizedCopy "Content/Tiles/Diagnostics/Wastes${name}Candidate.png" "Content/Tiles/Wastes${name}.png" $false
}
foreach ($contract in $wallFamilyContracts) {
	$name = $contract[0]
	Test-FixedAtlasContract "Content/Walls/Diagnostics/Wastes${name}WallCandidate.png" 468 180 $contract[1] $contract[2] $contract[3]
	Test-NormalizedCopy "Content/Walls/Diagnostics/Wastes${name}WallCandidate.png" "Content/Walls/Wastes${name}WallUnsafe.png" $false
}
Test-FixedAtlasContract 'Content/Tiles/DeadTuft.png' 144 18 7 964 '902D24197188F5057CE119DF61353D2D6263ED7A410E494A49003F49EAD171B3'
Test-FixedAtlasContract 'Content/Tiles/WastesBristle.png' 108 54 7 2385 'D7E04DA3EAEC618259AB8A47CFE1BD6166209B712F7825017DBD3096672393D3'
Test-FixedAtlasContract 'Content/Tiles/WastesRootShrub.png' 162 36 7 2811 '1C3FC10C9D52C9161914F32D28C6645541225F9A5CA3D5F00680C3BAC11E5243'
$terrainItemContracts = @(
	@('Content/Items/Placeable/WastesSoilBlock.png', 16, 16, 6, 232, '0D1D9C3688390FD9FEA30D3A805AC89399C1B7A83CB515D4E65C6F9369AC55ED'),
	@('Content/Items/Placeable/WastesStoneBlock.png', 16, 16, 6, 232, '9FA9DF1AE8557DF833F0FF9EEA1DF1572A8EEA392257C12062904F5A722E5FDC'),
	@('Content/Items/Placeable/WastesSandBlock.png', 16, 16, 6, 232, '9FA9DF1AE8557DF833F0FF9EEA1DF1572A8EEA392257C12062904F5A722E5FDC'),
	@('Content/Items/Placeable/WastesIceBlock.png', 16, 16, 6, 240, 'C2CC0456E15E9DE773A2A52B9D31449CAADD167F17923DC63AAB72A754305DF3'),
	@('Content/Items/Placeable/WastesSnowBlock.png', 16, 16, 6, 200, '2B9CF230B5D40020620F8D114771461F2D92AD68F3642DCCDDE534DE5993AFC9'),
	@('Content/Items/Placeable/WastesMudBlock.png', 16, 16, 6, 232, '0D1D9C3688390FD9FEA30D3A805AC89399C1B7A83CB515D4E65C6F9369AC55ED'),
	@('Content/Projectiles/WastesSandBallProjectile.png', 14, 14, 6, 148, '97393CB66056628400FDAB6AE30B482031581B06D55DB21F6E26F3153050221D')
)
foreach ($contract in $terrainItemContracts) {
	Test-FixedAtlasContract $contract[0] $contract[1] $contract[2] $contract[3] $contract[4] $contract[5]
}
$mawTilePairs = @(
	@('WastesSoil', 'MawDirt'),
	@('WastesStone', 'MawStone'),
	@('WastesGrass', 'MawGrass'),
	@('WastesSand', 'MawSand'),
	@('WastesIce', 'MawIce'),
	@('WastesSnow', 'MawSnow'),
	@('WastesMud', 'MawMud'),
	@('WastesSoil', 'MawClay')
)
foreach ($pair in $mawTilePairs) {
	Test-AlphaTopology "Content/Tiles/$($pair[0]).png" "Content/Tiles/Diagnostics/$($pair[1])Candidate.png" 6
	$productionName = if ($pair[1] -eq 'MawStone') { 'Mawstone' } else { $pair[1] }
	Test-NormalizedCopy "Content/Tiles/Diagnostics/$($pair[1])Candidate.png" "Content/Tiles/$productionName.png" $false
}
$mawWallPairs = @('Dirt', 'Stone', 'Grass', 'Sand', 'Ice', 'Snow', 'Mud')
foreach ($name in $mawWallPairs) {
	Test-AlphaTopology "Content/Walls/Wastes${name}WallUnsafe.png" "Content/Walls/Diagnostics/Maw${name}WallCandidate.png" 6
	Test-NormalizedCopy "Content/Walls/Diagnostics/Maw${name}WallCandidate.png" "Content/Walls/Maw${name}WallUnsafe.png" $false
}
foreach ($pair in @(
	@('WastesSoilBlock', 'MawDirtBlock'),
	@('WastesStoneBlock', 'MawstoneBlock'),
	@('WastesSandBlock', 'MawSandBlock'),
	@('WastesIceBlock', 'MawIceBlock'),
	@('WastesSnowBlock', 'MawSnowBlock'),
	@('WastesMudBlock', 'MawMudBlock'),
	@('WastesSoilBlock', 'MawClayBlock')
)) {
	Test-AlphaTopology "Content/Items/Placeable/$($pair[0]).png" "Content/Items/Placeable/$($pair[1]).png" 6
}
Test-AlphaTopology 'Content/Projectiles/WastesSandBallProjectile.png' 'Content/Projectiles/MawSandBallProjectile.png' 6

foreach ($required in @(
	'Content/Diagnostics/TileLabContent.cs',
	'Content/Diagnostics/TileLabGallery.cs',
	'Content/Diagnostics/GrassLabGallery.cs',
	'Content/Diagnostics/VegetationLabGallery.cs',
	'Content/Diagnostics/WastesTerrainFamilyGallery.cs',
	'Content/Diagnostics/WastesTerrainPropertyGallery.cs',
	'Content/Diagnostics/MawConversionGallery.cs',
	'Content/Diagnostics/WastesTerrainLabContent.cs',
	'Content/Projectiles/WastesSandBallFallingProjectile.cs',
	'Content/Projectiles/MawSandBallProjectile.cs',
	'Content/Items/Placeable/WastesTerrainBlocks.cs',
	'Content/Items/Placeable/MawTerrainBlocks.cs',
	'Content/Diagnostics/TileLabPlayer.cs',
	'Content/Diagnostics/VanillaAtlasExporter.cs',
	'Tools/New-WastesSoilCandidate.ps1',
	'Tools/New-WastesGrassCandidate.ps1',
	'Tools/New-WastesTerrainFamily.ps1',
	'Tools/New-WastesTerrainItems.ps1',
	'Tools/New-MawTerrainFamily.ps1',
	'Tools/New-WastesGroundCover.ps1',
	'Content/Tiles/WastesGroundCoverTiles.cs',
	'Art/Reference/WastesGroundCover-reference-v1.png',
	'Art/Reference/WastesGroundCover-reference-v2.png',
	'Art/Reference/WastesTerrainFamily-reference-v1.png'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $required))) {
        Add-Failure "Missing Tile Lab source: $required"
    }
}

$mawBiomePath = Join-Path $Root 'Content/Biomes/EngraftBiome.cs'
$mawBiomeSource = Get-Content -LiteralPath $mawBiomePath -Raw
if ($mawBiomeSource -match 'override\s+ModSurfaceBackgroundStyle\s+SurfaceBackgroundStyle' -and
	$mawBiomeSource -notmatch 'override\s+ModWaterStyle\s+WaterStyle') {
	Add-Failure 'Capture-unsafe biome: a custom surface background must not be paired with an unset water style.'
}

if ($failures.Count -gt 0) {
    Write-Host "TILE LAB CONTRACT: FAIL ($($failures.Count) problems)" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" }
    exit 1
}

Write-Host 'TILE LAB CONTRACT: PASS' -ForegroundColor Green
