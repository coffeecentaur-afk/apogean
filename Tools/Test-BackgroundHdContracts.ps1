Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$failures = [System.Collections.Generic.List[string]]::new()
$totalRawBytes = [int64]0
$biomes = @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption', 'Crimson', 'Hallow', 'Ocean', 'Mushroom')
$layers = foreach ($biome in $biomes) {
	foreach ($layer in @('Far', 'Mid', 'Close')) {
		@{
			Biome = $biome
			Name = $layer
			Path = "Content/Backgrounds/Diagnostics/HD/${biome}ConceptV0_${layer}.png"
			SourcePattern = "Art/Source/Backgrounds/$biome/V0-${layer}-extraction-v*.png"
		}
	}
}
$rawBytesByBiome = @{}

foreach ($layer in $layers) {
	$path = Join-Path $projectRoot $layer.Path
	if (-not (Test-Path -LiteralPath $path)) {
		$failures.Add("Missing HD $($layer.Name) layer: $path")
		continue
	}
	$source = Get-ChildItem -Path (Join-Path $projectRoot $layer.SourcePattern) |
		Sort-Object Name -Descending |
		Select-Object -First 1
	if ($null -eq $source) {
		$failures.Add("Missing authored source for $($layer.Biome) $($layer.Name).")
		continue
	}

	$bitmap = [System.Drawing.Bitmap]::new($path)
	$sourceBitmap = [System.Drawing.Bitmap]::new($source.FullName)
	try {
		if ($bitmap.Width -ne $sourceBitmap.Width -or $bitmap.Height -ne $sourceBitmap.Height) {
			$failures.Add("$($layer.Biome) $($layer.Name) is $($bitmap.Width)x$($bitmap.Height); source is $($sourceBitmap.Width)x$($sourceBitmap.Height).")
		}
		if ($bitmap.Width -lt 1600 -or $bitmap.Height -lt 700) {
			$failures.Add("$($layer.Biome) $($layer.Name) falls below the 1600x700 native-detail floor.")
		}
		if ($bitmap.Width -gt 4096 -or $bitmap.Height -gt 4096) {
			$failures.Add("$($layer.Biome) $($layer.Name) exceeds the conservative 4096px per-axis texture budget.")
		}

		$layerBytes = [int64]$bitmap.Width * $bitmap.Height * 4
		$totalRawBytes += $layerBytes
		if (-not $rawBytesByBiome.ContainsKey($layer.Biome)) { $rawBytesByBiome[$layer.Biome] = [int64]0 }
		$rawBytesByBiome[$layer.Biome] += $layerBytes
		$sampledColors = [System.Collections.Generic.HashSet[int]]::new()
		$softAlpha = $false
		for ($y = 0; $y -lt $bitmap.Height; $y += 4) {
			for ($x = 0; $x -lt $bitmap.Width; $x += 4) {
				$pixel = $bitmap.GetPixel($x, $y)
				if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $softAlpha = $true }
				if ($pixel.A -eq 255) { [void]$sampledColors.Add($pixel.ToArgb()) }
			}
		}
		if ($softAlpha) { $failures.Add("$($layer.Biome) $($layer.Name) contains soft-alpha edge pixels.") }
		if ($sampledColors.Count -lt 128) {
			$failures.Add("$($layer.Biome) $($layer.Name) retained only $($sampledColors.Count) sampled colours; the native-detail floor is 128.")
		}
		if ($bitmap.GetPixel([int]($bitmap.Width / 2), 0).A -ne 0) {
			$failures.Add("$($layer.Biome) $($layer.Name) paints over Terraria's sky at its top-center sample.")
		}
		if ($layer.Biome -eq 'Mushroom' -and $layer.Name -eq 'Far' -and
			$bitmap.GetPixel([Math]::Min(100, $bitmap.Width - 1), [Math]::Min(100, $bitmap.Height - 1)).A -ne 0) {
			$failures.Add('Mushroom Far lost its authored transparent sky region.')
		}
		for ($y = 0; $y -lt $bitmap.Height; $y++) {
			if ($bitmap.GetPixel(0, $y).ToArgb() -ne $bitmap.GetPixel($bitmap.Width - 1, $y).ToArgb()) {
				$failures.Add("$($layer.Biome) $($layer.Name) fails horizontal seam equality at row $y.")
				break
			}
		}
	}
	finally {
		$bitmap.Dispose()
		$sourceBitmap.Dispose()
	}
}

foreach ($biome in $biomes) {
	$rawBudget = 32MB
	if ($rawBytesByBiome[$biome] -gt $rawBudget) {
		$failures.Add("$biome HD layers require $([Math]::Round($rawBytesByBiome[$biome] / 1MB, 2)) MiB raw RGBA; per-style budget is 32 MiB.")
	}
}
if ($totalRawBytes -gt 256MB) {
	$failures.Add("All HD layers require $([Math]::Round($totalRawBytes / 1MB, 2)) MiB raw RGBA; complete-library budget is 256 MiB.")
}

$rendererSource = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs')
foreach ($contract in @('layerSets', 'HorizontalParallax', 'VerticalParallax', 'PositiveModulo', 'Main.screenPosition.X', 'Main.worldSurface', 'Main.ColorOfTheSkies', 'DrawUnderfill', 'GetReadableTint', 'TextureAssets.MagicPixel')) {
	if ($rendererSource -notmatch [regex]::Escape($contract)) {
		$failures.Add("HD renderer is missing source contract '$contract'.")
	}
}
foreach ($biome in $biomes) {
	if ($rendererSource -notmatch "RuinedBackgroundBiome\.$biome") {
		$failures.Add("HD renderer does not register $biome.")
	}
}
foreach ($forbiddenContract in @('AssetRequestMode.ImmediateLoad', 'CreateV0Layers')) {
	if ($rendererSource -match [regex]::Escape($forbiddenContract)) {
		$failures.Add("HD renderer still uses forbidden per-draw asset contract '$forbiddenContract'.")
	}
}
if ($rendererSource -notmatch 'HighDefinitionSurfaceBackgroundAssetSystem') {
	$failures.Add('HD renderer does not preload and release its texture assets through a ModSystem.')
}

if ($failures.Count -gt 0) {
	$failures | ForEach-Object { Write-Error $_ }
	exit 1
}

Write-Host "Surface HD background contracts passed: 9 biomes, 27 native-detail layers, hard alpha, exact seams, >=128 sampled colours, and $([Math]::Round($totalRawBytes / 1MB, 2)) MiB total raw RGBA."
