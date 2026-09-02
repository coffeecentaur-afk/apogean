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

$layerSpecs = @{
    'Far' = @(1024, 408)
    'Mid' = @(1024, 600)
    'Close' = @(952, 480)
}

foreach ($biome in @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption', 'Crimson', 'Hallow', 'Ocean', 'Engraft')) {
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
    Test-PixelSheet -Path (Join-Path $projectRoot "Content/Tiles/$tileSheet.png") -ExpectedWidth 234 -ExpectedHeight 270 -MaximumOpaqueColors 12
}

foreach ($fixtureSheet in @('KesslerPowerArmorRack', 'HelixSymbioteTank', 'SentrixHologramCore')) {
    Test-PixelSheet -Path (Join-Path $projectRoot "Content/Tiles/$fixtureSheet.png") -ExpectedWidth 54 -ExpectedHeight 288 -MaximumOpaqueColors 16
}

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
