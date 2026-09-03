param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$CaptureRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$Hex) {
    [System.Drawing.ColorTranslator]::FromHtml($Hex)
}

function Convert-NativeSprite(
    [string]$SourceName,
    [string]$OutputRelativePath,
    [string[]]$PaletteHex
) {
    $sourcePath = Join-Path $CaptureRoot $SourceName
    $outputPath = Join-Path $Root $OutputRelativePath
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Missing renderer-exported Terraria sprite: $sourcePath"
    }

    $palette = @($PaletteHex | ForEach-Object { Convert-HexColor $_ })
    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $luminances = [System.Collections.Generic.List[double]]::new()
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -gt 0) {
                    $luminances.Add((0.299 * $pixel.R) + (0.587 * $pixel.G) + (0.114 * $pixel.B))
                }
            }
        }
        if ($luminances.Count -eq 0) {
            throw "Source sprite contains no opaque pixels: $sourcePath"
        }

        $minimum = ($luminances | Measure-Object -Minimum).Minimum
        $maximum = ($luminances | Measure-Object -Maximum).Maximum
        $range = [Math]::Max(1.0, $maximum - $minimum)

        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -eq 0) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $luminance = (0.299 * $pixel.R) + (0.587 * $pixel.G) + (0.114 * $pixel.B)
                $normalized = [Math]::Clamp(($luminance - $minimum) / $range, 0.0, 1.0)
                $index = [Math]::Min($palette.Count - 1, [int][Math]::Floor($normalized * $palette.Count))
                $chosen = $palette[$index]
                $output.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($pixel.A, $chosen.R, $chosen.G, $chosen.B))
            }
        }

        $directory = Split-Path -Parent $outputPath
        if (-not (Test-Path -LiteralPath $directory)) {
            New-Item -ItemType Directory -Path $directory | Out-Null
        }
        $temporaryPath = "$outputPath.generated.png"
        $output.Save($temporaryPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Move-Item -LiteralPath $temporaryPath -Destination $outputPath -Force
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

# Every asset keeps the exact silhouette and detail frequency of its Terraria
# counterpart. Only the indexed palette changes, preventing vector-like or
# over-detailed inventory icons from entering the production mod.
Convert-NativeSprite 'Vanilla-DirtBlock-Item.png'  'Content/Items/Placeable/WastesSoilBlock.png'  @('#241b16', '#3b2a20', '#59402b', '#795837', '#a67b4a', '#d0a76d')
Convert-NativeSprite 'Vanilla-StoneBlock-Item.png' 'Content/Items/Placeable/WastesStoneBlock.png' @('#28231f', '#413a34', '#5e554c', '#7d7062', '#a08d78', '#c9b49b')
Convert-NativeSprite 'Vanilla-SandBlock-Item.png'  'Content/Items/Placeable/WastesSandBlock.png'  @('#49351f', '#6e502b', '#987039', '#bc914f', '#dab66e', '#efd596')
Convert-NativeSprite 'Vanilla-IceBlock-Item.png'   'Content/Items/Placeable/WastesIceBlock.png'   @('#293638', '#3f5052', '#5e7172', '#819394', '#aab8b5', '#d4d8cd')
Convert-NativeSprite 'Vanilla-SnowBlock-Item.png'  'Content/Items/Placeable/WastesSnowBlock.png'  @('#514b42', '#70695c', '#918979', '#aea694', '#cec6ad', '#e5ddc5')
Convert-NativeSprite 'Vanilla-MudBlock-Item.png'   'Content/Items/Placeable/WastesMudBlock.png'   @('#211a14', '#36291c', '#4d3925', '#695034', '#886943', '#aa895b')
Convert-NativeSprite 'Vanilla-SandBall-Projectile.png' 'Content/Projectiles/WastesSandBallProjectile.png' @('#49351f', '#6e502b', '#987039', '#bc914f', '#dab66e', '#efd596')

Write-Host 'Generated Terraria-native Wastes terrain item and sand projectile sprites.' -ForegroundColor Green
