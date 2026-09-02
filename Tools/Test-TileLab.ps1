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

Test-NormalizedCopy 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/TileLabBlock.png' $true
Test-NormalizedCopy 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/TileLabWall.png' $false
Test-CandidateAtlas 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/WastesSoilCandidate.png' 5 44104
Test-CandidateAtlas 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/WastesDirtWallCandidate.png' 5 28864
Test-NormalizedCopy 'Content/Tiles/Diagnostics/WastesSoilCandidate.png' 'Content/Tiles/WastesSoil.png' $false
Test-NormalizedCopy 'Content/Walls/Diagnostics/WastesDirtWallCandidate.png' 'Content/Walls/WastesDirtWallUnsafe.png' $false

foreach ($required in @(
	'Content/Diagnostics/TileLabContent.cs',
	'Content/Diagnostics/TileLabGallery.cs',
	'Content/Diagnostics/TileLabPlayer.cs',
	'Content/Diagnostics/VanillaAtlasExporter.cs',
	'Tools/New-WastesSoilCandidate.ps1'
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
