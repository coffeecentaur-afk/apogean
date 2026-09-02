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

Test-NormalizedCopy 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/TileLabBlock.png' $true
Test-NormalizedCopy 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/TileLabWall.png' $false

foreach ($required in @(
	'Content/Diagnostics/TileLabContent.cs',
	'Content/Diagnostics/TileLabGallery.cs',
	'Content/Diagnostics/TileLabPlayer.cs'
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
