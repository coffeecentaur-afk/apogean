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

# Helix reads as an old clinical biotech campus: charcoal-green structural metal,
# aged bone ceramic, amber specimen labels, and green light only where a fixture
# actually emits it. None of the structural atlases use paper-white mortar.
$palettes = @{
	Block = @((C '#111715'), (C '#19231f'), (C '#26332d'), (C '#38473f'), (C '#526058'), (C '#747d70'), (C '#a0a18d'))
	Trim = @((C '#141710'), (C '#242719'), (C '#3a3b24'), (C '#555134'), (C '#796b3b'), (C '#a38a46'), (C '#cfb75f'))
	Floor = @((C '#101513'), (C '#171f1c'), (C '#222d28'), (C '#303d37'), (C '#435149'), (C '#5c675d'), (C '#7b8072'))
	Glass = @((C '#0d1614'), (C '#13221e'), (C '#1c302a'), (C '#294039'), (C '#3a574c'), (C '#527467'), (C '#719587'))
	Beam = @((C '#0d1210'), (C '#151d19'), (C '#202b25'), (C '#2e3a31'), (C '#424b3e'), (C '#5e6049'), (C '#847853'))
	Containment = @((C '#111815'), (C '#19251f'), (C '#26372d'), (C '#36503e'), (C '#477052'), (C '#629065'), (C '#87b178'))
	Ruin = @((C '#111412'), (C '#1b211d'), (C '#292f29'), (C '#393f36'), (C '#4c5043'), (C '#656554'), (C '#85806a'))
	MawResearch = @((C '#15120b'), (C '#261e0d'), (C '#3a2d10'), (C '#554015'), (C '#775b1b'), (C '#a27e23'), (C '#d0ad3c'))
	LaboratoryWall = @((C '#121714'), (C '#19201c'), (C '#222b26'), (C '#2e3731'), (C '#3c463e'), (C '#50594e'), (C '#697064'))
	ObservationWall = @((C '#101a17'), (C '#172722'), (C '#20352e'), (C '#2b473c'), (C '#3a5b4d'), (C '#4e725f'), (C '#6c8c76'))
}

$black = C '#0b100e'
$deep = C '#17211d'
$mid = C '#2d3b34'
$sage = C '#5d6d60'
$bone = C '#a9aa91'
$amberDark = C '#6d551d'
$amber = C '#c6a33f'
$greenDark = C '#1f6b42'
$green = C '#4fc77a'
$greenLight = C '#9cf0a7'

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
		Fill $sheet $black ($x + 1) 6 16 10
		Fill $sheet $sage ($x + 1) 6 16 2
		Fill $sheet $mid ($x + 2) 8 14 5
		Fill $sheet $deep ($x + 2) 13 14 3
		if ($frame % 5 -eq 0) { Fill $sheet $amber ($x + 7) 9 3 1 }
	}
	Save-Bitmap $sheet 'Content/Tiles/HelixPlatform.png'
}

function New-Chair {
	$sheet = New-Bitmap 36 40
	for ($style = 0; $style -lt 2; $style++) {
		$logical = New-Bitmap 16 32
		$mirror = $style -eq 1
		$back = if ($mirror) { 10 } else { 2 }
		$seat = if ($mirror) { 2 } else { 5 }
		Fill $logical $black $back 2 5 23
		Fill $logical $sage ($back + 1) 3 3 17
		Fill $logical $greenDark ($back + 1) 8 3 8
		Fill $logical $black ($seat - 1) 18 10 6
		Fill $logical $bone $seat 19 8 2
		Fill $logical $black $seat 23 2 9
		Fill $logical $black ($seat + 6) 23 2 9
		Copy-LogicalSprite $logical $sheet 1 2 ($style * 18) 0
		$logical.Dispose()
	}
	Save-Bitmap $sheet 'Content/Tiles/HelixChair.png'
}

function New-Table {
	$logical = New-Bitmap 48 32
	Fill $logical $black 1 12 46 8
	Fill $logical $bone 2 12 44 2
	Fill $logical $sage 3 14 42 4
	Fill $logical $greenDark 10 16 28 2
	Fill $logical $black 4 20 5 12
	Fill $logical $black 39 20 5 12
	$sheet = New-Bitmap 54 36
	Copy-LogicalSprite $logical $sheet 3 2
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/HelixTable.png'
}

function New-Workbench {
	$logical = New-Bitmap 32 16
	Fill $logical $black 1 3 30 13
	Fill $logical $bone 2 3 28 2
	Fill $logical $mid 3 5 26 7
	Fill $logical $greenDark 5 7 10 3
	Fill $logical $amberDark 20 7 6 3
	Fill $logical $black 4 12 4 4
	Fill $logical $black 24 12 4 4
	$sheet = New-Bitmap 36 20
	Copy-LogicalSprite $logical $sheet 2 1
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/HelixWorkbench.png'
}

function New-Light {
	$sheet = New-Bitmap 18 18
	Fill $sheet $black 3 4 12 10
	Fill $sheet $sage 4 4 10 2
	Fill $sheet $greenDark 5 7 8 6
	Fill $sheet $green 6 8 6 4
	Fill $sheet $greenLight 8 9 2 2
	Save-Bitmap $sheet 'Content/Tiles/HelixLight.png'
}

function New-Console {
	$logical = New-Bitmap 48 32
	Fill $logical $black 1 4 46 28
	Fill $logical $mid 3 6 42 11
	Fill $logical $deep 5 8 38 7
	Fill $logical $greenDark 7 9 18 4
	Fill $logical $green 9 10 12 2
	Fill $logical $amberDark 29 9 10 4
	Fill $logical $amber 31 10 6 2
	Fill $logical $sage 4 18 40 8
	foreach ($x in @(7, 14, 22, 30, 38)) { Fill $logical $green $x 20 3 2 }
	Fill $logical $black 5 26 5 6
	Fill $logical $black 38 26 5 6
	$sheet = New-Bitmap 54 36
	Copy-LogicalSprite $logical $sheet 3 2
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/HelixConsole.png'
}

function New-Locker {
	$logical = New-Bitmap 32 48
	Fill $logical $black 1 1 30 47
	Fill $logical $mid 3 3 26 43
	Fill $logical $deep 5 5 10 39
	Fill $logical $deep 17 5 10 39
	Fill $logical $bone 6 6 8 2
	Fill $logical $bone 18 6 8 2
	foreach ($y in @(11, 18, 32, 39)) { Fill $logical $black 8 $y 4 1; Fill $logical $black 20 $y 4 1 }
	Fill $logical $green 12 25 2 4
	Fill $logical $green 18 25 2 4
	$sheet = New-Bitmap 36 54
	Copy-LogicalSprite $logical $sheet 2 3
	$logical.Dispose()
	Save-Bitmap $sheet 'Content/Tiles/HelixLocker.png'
}

function New-SymbioteTank {
	$sheet = New-Bitmap 54 288
	$motion = @(0, 1, 0, -1)
	for ($frame = 0; $frame -lt 4; $frame++) {
		$logical = New-Bitmap 48 64
		Fill $logical $black 1 1 46 63
		Fill $logical $mid 3 3 42 59
		Fill $logical $bone 5 4 38 5
		Fill $logical $bone 5 55 38 5
		Fill $logical $deep 6 10 36 44
		Fill $logical $greenDark 8 12 32 40
		Fill $logical ([System.Drawing.Color]::FromArgb(255, 38, 91, 59)) 10 14 28 36
		# A readable engineered organism: central sac, paired tendrils, and a
		# pulsing amber implant. The four frames shift only internal tissue.
		$shift = $motion[$frame]
		Fill $logical $black (19 + $shift) 20 10 21
		Fill $logical $green (21 + $shift) 18 6 24
		Fill $logical $greenLight (23 + $shift) 22 2 13
		for ($y = 24; $y -le 43; $y += 6) {
			Put $logical (16 - $shift) $y $green
			Put $logical (31 + $shift) ($y + 2) $green
		}
		Fill $logical $amber (22 + $shift) 31 4 4
		Fill $logical $bone 8 57 8 1
		Fill $logical $bone 32 57 8 1
		Copy-LogicalSprite $logical $sheet 3 4 0 ($frame * 72)
		$logical.Dispose()
	}
	Save-Bitmap $sheet 'Content/Tiles/HelixSymbioteTank.png'
}

Convert-Atlas $tileReference 'Content/Tiles/HelixBlock.png' $palettes.Block
Convert-Atlas $tileReference 'Content/Tiles/HelixTrim.png' $palettes.Trim
Convert-Atlas $tileReference 'Content/Tiles/HelixFloor.png' $palettes.Floor
Convert-Atlas $tileReference 'Content/Tiles/HelixGlass.png' $palettes.Glass
Convert-Atlas $tileReference 'Content/Tiles/HelixBeam.png' $palettes.Beam
Convert-Atlas $tileReference 'Content/Tiles/HelixContainmentPanel.png' $palettes.Containment
Convert-Atlas $tileReference 'Content/Tiles/HelixRuinBlock.png' $palettes.Ruin
Convert-Atlas $tileReference 'Content/Tiles/MawResearchBlock.png' $palettes.MawResearch
Convert-Atlas $wallReference 'Content/Walls/HelixLaboratoryWall.png' $palettes.LaboratoryWall
Convert-Atlas $wallReference 'Content/Walls/HelixObservationWall.png' $palettes.ObservationWall

New-Platform
New-Chair
New-Table
New-Workbench
New-Light
New-Console
New-Locker
New-SymbioteTank

Write-Host 'Generated Helix native-topology construction, walls, furniture, lighting, and animated specimen tank.'
