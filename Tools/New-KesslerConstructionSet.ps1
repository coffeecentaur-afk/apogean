param(
	[string]$Root = (Split-Path -Parent $PSScriptRoot),
	[string]$ReferenceRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }

$tileReference = Join-Path $ReferenceRoot 'Vanilla-GrayBrick-Tile.png'
$wallReference = Join-Path $ReferenceRoot 'Vanilla-GrayBrick-Wall.png'
if (-not (Test-Path -LiteralPath $tileReference) -or -not (Test-Path -LiteralPath $wallReference)) {
	throw 'Missing vanilla Gray Brick references. Load the client and run /apogean exportatlases first.'
}

$palettes = @{
	Block = @((C '#111416'), (C '#1b2023'), (C '#282e31'), (C '#383f42'), (C '#50575a'), (C '#6e716d'), (C '#989386'))
	Trim = @((C '#171315'), (C '#271b1a'), (C '#3d2520'), (C '#5a3026'), (C '#783b2b'), (C '#a54a2d'), (C '#d06a32'))
	Floor = @((C '#101315'), (C '#191d20'), (C '#242a2d'), (C '#333a3d'), (C '#464d4f'), (C '#5c615e'), (C '#77766d'))
	Glass = @((C '#0d1418'), (C '#142027'), (C '#1d2d35'), (C '#29404a'), (C '#3a5660'), (C '#58727a'), (C '#7f9699'))
	Beam = @((C '#0d0f10'), (C '#151819'), (C '#202425'), (C '#2d3030'), (C '#413733'), (C '#633c31'), (C '#925036'))
	BulkheadWall = @((C '#151719'), (C '#1b1e20'), (C '#222629'), (C '#2b3033'), (C '#373c3e'), (C '#464a48'))
	WindowWall = @((C '#10171b'), (C '#162127'), (C '#1d2d34'), (C '#263b43'), (C '#334c53'), (C '#4b6366'))
}

$black = C '#0b0d0e'
$deep = C '#171a1c'
$mid = C '#30363a'
$steel = C '#51585a'
$highlight = C '#878a83'
$rust = C '#7e3826'
$orange = C '#d95a27'
$amber = C '#f1a23f'
$red = C '#fa3f2a'
$clothDark = C '#351616'
$cloth = C '#6f2720'

function New-Bitmap([int]$width, [int]$height) {
	[System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
}

function Save-Bitmap([System.Drawing.Bitmap]$bitmap, [string]$relativePath) {
	$path = Join-Path $Root $relativePath
	$directory = Split-Path -Parent $path
	if (-not (Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
	$bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
	$bitmap.Dispose()
}

function Put([System.Drawing.Bitmap]$bitmap, [int]$x, [int]$y, [System.Drawing.Color]$color) {
	if ($x -ge 0 -and $y -ge 0 -and $x -lt $bitmap.Width -and $y -lt $bitmap.Height) {
		$bitmap.SetPixel($x, $y, $color)
	}
}

function Fill([System.Drawing.Bitmap]$bitmap, [System.Drawing.Color]$color, [int]$x, [int]$y, [int]$width, [int]$height) {
	for ($py = $y; $py -lt $y + $height; $py++) {
		for ($px = $x; $px -lt $x + $width; $px++) { Put $bitmap $px $py $color }
	}
}

function Convert-Atlas([string]$sourcePath, [string]$outputPath, [System.Drawing.Color[]]$palette) {
	$source = [System.Drawing.Bitmap]::new($sourcePath)
	$output = New-Bitmap $source.Width $source.Height
	try {
		$minimumLuma = 255
		$maximumLuma = 0
		for ($y = 0; $y -lt $source.Height; $y++) {
			for ($x = 0; $x -lt $source.Width; $x++) {
				$pixel = $source.GetPixel($x, $y)
				if ($pixel.A -eq 0) { continue }
				$luma = [int](($pixel.R * 30 + $pixel.G * 59 + $pixel.B * 11) / 100)
				$minimumLuma = [Math]::Min($minimumLuma, $luma)
				$maximumLuma = [Math]::Max($maximumLuma, $luma)
			}
		}
		$lumaRange = [Math]::Max(1.0, $maximumLuma - $minimumLuma)
		for ($y = 0; $y -lt $source.Height; $y++) {
			for ($x = 0; $x -lt $source.Width; $x++) {
				$pixel = $source.GetPixel($x, $y)
				if ($pixel.A -eq 0) { continue }
				$luma = [int](($pixel.R * 30 + $pixel.G * 59 + $pixel.B * 11) / 100)
				$normal = [Math]::Max(0.0, [Math]::Min(1.0, ($luma - $minimumLuma) / $lumaRange))
				$index = [Math]::Min($palette.Count - 1, [int][Math]::Floor($normal * $palette.Count))
				$output.SetPixel($x, $y, $palette[$index])
			}
		}
		Save-Bitmap $output $outputPath
		$output = $null
	}
	finally {
		$source.Dispose()
		if ($null -ne $output) { $output.Dispose() }
	}
}

function Copy-LogicalSprite(
	[System.Drawing.Bitmap]$logical,
	[System.Drawing.Bitmap]$sheet,
	[int]$tileWidth,
	[int]$tileHeight,
	[int]$destinationX = 0,
	[int]$destinationY = 0
) {
	for ($y = 0; $y -lt $tileHeight * 16; $y++) {
		for ($x = 0; $x -lt $tileWidth * 16; $x++) {
			$sheetX = $destinationX + [int][Math]::Floor($x / 16.0) * 18 + ($x % 16)
			$sheetY = $destinationY + [int][Math]::Floor($y / 16.0) * 18 + ($y % 16)
			$sheet.SetPixel($sheetX, $sheetY, $logical.GetPixel($x, $y))
		}
	}
}

function New-Platform {
	$sheet = New-Bitmap 486 18
	for ($frame = 0; $frame -lt 27; $frame++) {
		$x = $frame * 18
		Fill $sheet $black ($x + 1) 7 16 8
		Fill $sheet $steel ($x + 1) 6 16 2
		Fill $sheet $mid ($x + 2) 8 14 4
		Fill $sheet $deep ($x + 2) 12 14 3
		if ($frame % 4 -eq 0) { Fill $sheet $orange ($x + 4) 8 3 1 }
	}
	Save-Bitmap $sheet 'Content/Tiles/KesslerPlatform.png'
}

function New-Chair {
	$sheet = New-Bitmap 36 40
	for ($style = 0; $style -lt 2; $style++) {
		$logical = New-Bitmap 16 32
		$mirror = $style -eq 1
		$back = if ($mirror) { 10 } else { 3 }
		$seat = if ($mirror) { 3 } else { 5 }
		Fill $logical $black ($back - 1) 2 5 23
		Fill $logical $mid $back 3 3 19
		Fill $logical $rust $back 7 3 9
		Fill $logical $black ($seat - 1) 18 10 6
		Fill $logical $steel $seat 19 8 2
		Fill $logical $black $seat 23 2 9
		Fill $logical $black ($seat + 6) 23 2 9
		Copy-LogicalSprite $logical $sheet 1 2 ($style * 18) 0
		$logical.Dispose()
	}
	Save-Bitmap $sheet 'Content/Tiles/KesslerChair.png'
}

function New-Table {
	$logical = New-Bitmap 48 32
	Fill $logical $black 1 13 46 8
	Fill $logical $steel 2 13 44 2
	Fill $logical $mid 3 15 42 4
	Fill $logical $black 4 20 6 12
	Fill $logical $black 38 20 6 12
	Fill $logical $rust 15 17 18 2
	$sheet = New-Bitmap 54 36
	Copy-LogicalSprite $logical $sheet 3 2
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/KesslerTable.png'
}

function New-Workbench {
	$logical = New-Bitmap 32 16
	Fill $logical $black 1 4 30 12
	Fill $logical $steel 2 4 28 2
	Fill $logical $mid 3 6 26 6
	Fill $logical $rust 5 8 9 3
	Fill $logical $orange 20 8 5 2
	Fill $logical $black 4 12 4 4
	Fill $logical $black 24 12 4 4
	$sheet = New-Bitmap 36 20
	Copy-LogicalSprite $logical $sheet 2 1
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/KesslerWorkbench.png'
}

function New-Light {
	$sheet = New-Bitmap 18 18
	Fill $sheet $black 3 5 12 8
	Fill $sheet $steel 4 4 10 2
	Fill $sheet $rust 5 7 8 6
	Fill $sheet $orange 6 8 6 4
	Fill $sheet $amber 8 8 2 3
	Save-Bitmap $sheet 'Content/Tiles/KesslerLight.png'
}

function New-Console {
	$logical = New-Bitmap 48 32
	Fill $logical $black 1 4 46 28
	Fill $logical $mid 3 6 42 11
	Fill $logical $deep 5 8 38 7
	Fill $logical $rust 7 9 18 3
	Fill $logical $orange 8 10 8 1
	Fill $logical $red 29 9 8 3
	Fill $logical $steel 4 18 40 8
	for ($x = 7; $x -le 35; $x += 7) { Fill $logical $orange $x 20 3 2 }
	Fill $logical $black 5 26 5 6
	Fill $logical $black 38 26 5 6
	$sheet = New-Bitmap 54 36
	Copy-LogicalSprite $logical $sheet 3 2
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/KesslerConsole.png'
}

function New-Locker {
	$logical = New-Bitmap 32 48
	Fill $logical $black 1 1 30 47
	Fill $logical $mid 3 3 26 43
	Fill $logical $deep 5 5 10 39
	Fill $logical $deep 17 5 10 39
	Fill $logical $steel 6 6 8 2
	Fill $logical $steel 18 6 8 2
	for ($y = 12; $y -le 18; $y += 3) { Fill $logical $black 8 $y 4 1; Fill $logical $black 20 $y 4 1 }
	Fill $logical $orange 12 25 2 4
	Fill $logical $orange 18 25 2 4
	Fill $logical $rust 5 39 10 4
	Fill $logical $rust 17 39 10 4
	$sheet = New-Bitmap 36 54
	Copy-LogicalSprite $logical $sheet 2 3
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/KesslerLocker.png'
}

function New-PowerArmorRack {
	$sheet = New-Bitmap 54 288
	for ($frame = 0; $frame -lt 4; $frame++) {
		$logical = New-Bitmap 48 64
		Fill $logical $black 1 1 46 63
		Fill $logical $mid 4 3 40 58
		Fill $logical $deep 7 6 34 52
		Fill $logical $steel 8 7 32 3
		Fill $logical $steel 8 54 32 4
		Fill $logical $rust 6 14 4 35
		Fill $logical $rust 38 14 4 35
		# Helmet, shoulders, torso, gauntlets, and split armoured legs.
		Fill $logical $black 19 11 10 9
		Fill $logical $steel 20 12 8 7
		Fill $logical $red (21 + $frame) 15 5 2
		Fill $logical $black 13 21 22 9
		Fill $logical $mid 15 22 18 7
		Fill $logical $black 18 28 12 17
		Fill $logical $steel 20 29 8 14
		Fill $logical $rust 22 31 4 10
		Fill $logical $black 10 27 7 20
		Fill $logical $black 31 27 7 20
		Fill $logical $mid 11 29 5 16
		Fill $logical $mid 32 29 5 16
		Fill $logical $black 17 43 6 13
		Fill $logical $black 25 43 6 13
		Fill $logical $steel 18 44 4 10
		Fill $logical $steel 26 44 4 10
		Fill $logical $orange (8 + $frame * 8) 59 5 2
		Copy-LogicalSprite $logical $sheet 3 4 0 ($frame * 72)
		$logical.Dispose()
	}
	Save-Bitmap $sheet 'Content/Tiles/KesslerPowerArmorRack.png'
}

function New-WarBanner {
	$sheet = New-Bitmap 72 288
	$wave = @(0, 2, 4, 2)
	for ($frame = 0; $frame -lt 4; $frame++) {
		$logical = New-Bitmap 64 64
		# Pole and weighted military base.
		Fill $logical $black 4 0 6 60
		Fill $logical $steel 5 1 3 57
		Fill $logical $highlight 6 2 1 54
		Fill $logical $black 0 57 16 7
		Fill $logical $mid 2 57 12 4
		# Four hard-edged cloth bands create a restrained waving silhouette.
		$shift = $wave[$frame]
		for ($band = 0; $band -lt 4; $band++) {
			$y = 5 + $band * 10
			$bandShift = if ($band % 2 -eq 0) { $shift } else { [int]($shift / 2) }
			$width = 49 - $bandShift - $band
			Fill $logical $clothDark 9 $y $width 11
			Fill $logical $cloth 11 ($y + 2) ($width - 3) 7
		}
		# Kessler mark: a pale shield split by three signal chevrons.
		Fill $logical $highlight 27 12 15 22
		Fill $logical $black 29 14 11 18
		for ($chevron = 0; $chevron -lt 3; $chevron++) {
			$cy = 16 + $chevron * 5
			Fill $logical $orange 31 $cy 7 2
			Put $logical 30 ($cy + 1) $orange
			Put $logical 38 ($cy + 1) $orange
		}
		Copy-LogicalSprite $logical $sheet 4 4 0 ($frame * 72)
		$logical.Dispose()
	}
	Save-Bitmap $sheet 'Content/Tiles/KesslerWarBanner.png'
}

Convert-Atlas $tileReference 'Content/Tiles/KesslerBlock.png' $palettes.Block
Convert-Atlas $tileReference 'Content/Tiles/KesslerTrim.png' $palettes.Trim
Convert-Atlas $tileReference 'Content/Tiles/KesslerFloor.png' $palettes.Floor
Convert-Atlas $tileReference 'Content/Tiles/KesslerGlass.png' $palettes.Glass
Convert-Atlas $tileReference 'Content/Tiles/KesslerBeam.png' $palettes.Beam
Convert-Atlas $tileReference 'Content/Tiles/KesslerPlating.png' $palettes.Block
Convert-Atlas $tileReference 'Content/Tiles/KesslerRuinBlock.png' $palettes.Beam
Convert-Atlas $wallReference 'Content/Walls/KesslerBulkheadWall.png' $palettes.BulkheadWall
Convert-Atlas $wallReference 'Content/Walls/KesslerWindowWall.png' $palettes.WindowWall

New-Platform
New-Chair
New-Table
New-Workbench
New-Light
New-Console
New-Locker
New-PowerArmorRack
New-WarBanner

Write-Host 'Generated the Kessler native construction, furniture, lighting, rack, and animated banner set.'
