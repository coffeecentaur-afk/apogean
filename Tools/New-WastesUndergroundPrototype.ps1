param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$outputDir = Join-Path $Root 'Content/Backgrounds/Diagnostics'
[System.IO.Directory]::CreateDirectory($outputDir) | Out-Null

function New-Color([string]$hex) {
	[System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Get-Hash([int]$x, [int]$y, [int]$seed) {
	$value = [uint32](($x * 374761393L + $y * 668265263L + $seed * 69069L) -band 0xFFFFFFFFL)
	$value = [uint32](($value -bxor ($value -shr 13)) * 1274126177L -band 0xFFFFFFFFL)
	return [double](($value -bxor ($value -shr 16)) -band 0xFFFF) / 65535.0
}

function Set-Block(
	[System.Drawing.Bitmap]$bitmap,
	[int]$left,
	[int]$top,
	[int]$width,
	[int]$height,
	[System.Drawing.Color]$color
) {
	for ($y = [Math]::Max(0, $top); $y -lt [Math]::Min($bitmap.Height, $top + $height); $y++) {
		for ($x = [Math]::Max(0, $left); $x -lt [Math]::Min(128, $left + $width); $x++) {
			$bitmap.SetPixel($x, $y, $color)
		}
	}
}

function Set-Line(
	[System.Drawing.Bitmap]$bitmap,
	[int]$x0,
	[int]$y0,
	[int]$x1,
	[int]$y1,
	[System.Drawing.Color]$color,
	[int]$width = 1
) {
	$dx = [Math]::Abs($x1 - $x0)
	$sx = if ($x0 -lt $x1) { 1 } else { -1 }
	$dy = -[Math]::Abs($y1 - $y0)
	$sy = if ($y0 -lt $y1) { 1 } else { -1 }
	$error = $dx + $dy
	while ($true) {
		Set-Block $bitmap $x0 $y0 $width $width $color
		if ($x0 -eq $x1 -and $y0 -eq $y1) { break }
		$twice = 2 * $error
		if ($twice -ge $dy) { $error += $dy; $x0 += $sx }
		if ($twice -le $dx) { $error += $dx; $y0 += $sy }
	}
}

function New-CaveField([bool]$deep) {
	$palette = if ($deep) {
		@('#1D2020', '#292A27', '#35332D', '#433D32', '#544735', '#68533A', '#80623F', '#9A7547') | ForEach-Object { New-Color $_ }
	} else {
		@('#2B2823', '#383229', '#473B2E', '#594634', '#6C5238', '#82613E', '#9C7446', '#B78A52') | ForEach-Object { New-Color $_ }
	}
	$bitmap = [System.Drawing.Bitmap]::new(128, 96, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

	# This is an opaque, repeating back-wall material. Distinct landmarks belong
	# to world furniture; placing a room here stamps it every 128 pixels.
	for ($y = 0; $y -lt 96; $y++) {
		for ($x = 0; $x -lt 128; $x++) {
			$wave = [Math]::Sin((2.0 * [Math]::PI * $x / 128.0) + ($y * 0.055))
			$band = [Math]::Sin(2.0 * [Math]::PI * $y / 24.0 + $wave * 0.8)
			$seed = if ($deep) { 97 } else { 41 }
			$grain = Get-Hash ([Math]::Floor($x / 4)) ([Math]::Floor($y / 4)) $seed
			$index = 2 + [int][Math]::Round($band * 0.65 + ($grain - 0.5) * 2.2)
			$index = [Math]::Clamp($index, 0, $palette.Count - 1)
			$color = $palette[$index]
			$bitmap.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $color.R, $color.G, $color.B))
		}
	}

	# Eroded strata and collapsed support fragments provide narrative texture
	# without creating a recognizable object that repeats across the cave.
	$shadow = $palette[0]
	$line = $palette[1]
	$edge = $palette[4]
	$highlight = $palette[6]
	foreach ($strataY in @(17, 46, 73)) {
		for ($x = 0; $x -lt 128; $x += 4) {
			$offset = [int][Math]::Round(2.0 * [Math]::Sin(2.0 * [Math]::PI * $x / 64.0 + $strataY))
			Set-Block $bitmap $x ($strataY + $offset) 4 2 $shadow
			if (($x / 4) % 3 -ne 1) { Set-Block $bitmap $x ($strataY + $offset - 1) 3 1 $edge }
		}
	}

	Set-Line $bitmap 19 5 29 38 $line 2
	Set-Line $bitmap 29 38 41 55 $shadow 2
	Set-Line $bitmap 98 48 108 77 $line 2
	Set-Line $bitmap 108 77 117 88 $shadow 2
	Set-Line $bitmap 53 61 73 57 $edge 2
	Set-Line $bitmap 73 57 88 62 $line 2
	Set-Block $bitmap 49 65 5 2 $highlight
	Set-Block $bitmap 82 30 3 3 $highlight
	Set-Block $bitmap 11 84 4 2 $edge

	# Explicit horizontal seam match; New-WrappedField supplies the extra 32px.
	for ($y = 0; $y -lt 96; $y++) {
		$bitmap.SetPixel(127, $y, $bitmap.GetPixel(0, $y))
	}
	return $bitmap
}

function New-WrappedField([System.Drawing.Bitmap]$core) {
	$field = [System.Drawing.Bitmap]::new(160, $core.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
	for ($y = 0; $y -lt $core.Height; $y++) {
		for ($x = 0; $x -lt 128; $x++) { $field.SetPixel($x, $y, $core.GetPixel($x, $y)) }
		for ($x = 0; $x -lt 32; $x++) { $field.SetPixel(128 + $x, $y, $core.GetPixel($x, $y)) }
	}
	return $field
}

function New-TransitionStrip([System.Drawing.Bitmap]$field, [int]$sourceY) {
	$core = [System.Drawing.Bitmap]::new(128, 16, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
	try {
		for ($y = 0; $y -lt 16; $y++) {
			for ($x = 0; $x -lt 128; $x++) {
				$sampleY = ($sourceY + $y) % $field.Height
				$core.SetPixel($x, $y, $field.GetPixel($x, $sampleY))
			}
		}
		return New-WrappedField $core
	}
	finally { $core.Dispose() }
}

$shallowCore = New-CaveField $false
$deepCore = New-CaveField $true
try {
	$fields = @(
		(New-TransitionStrip $shallowCore 8),
		(New-WrappedField $shallowCore),
		(New-TransitionStrip $deepCore 24),
		(New-WrappedField $deepCore)
	)
	try {
		for ($i = 0; $i -lt $fields.Count; $i++) {
			$path = Join-Path $outputDir "ForestUndergroundConceptV0_$i.png"
			$fields[$i].Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
			Write-Host "Wrote $path ($($fields[$i].Width)x$($fields[$i].Height))"
		}
	}
	finally { foreach ($field in $fields) { $field.Dispose() } }
}
finally {
	$shallowCore.Dispose()
	$deepCore.Dispose()
}
