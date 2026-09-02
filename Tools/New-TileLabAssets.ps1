param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function New-NormalizedCopy([string]$sourceRelativePath, [string]$outputRelativePath, [bool]$usesMagentaKey) {
    $sourcePath = Join-Path $Root $sourceRelativePath
    $outputPath = Join-Path $Root $outputRelativePath
    $outputDirectory = Split-Path -Parent $outputPath
    if (-not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($usesMagentaKey -and $pixel.R -gt 220 -and $pixel.B -gt 220 -and $pixel.G -lt 160) {
                    $pixel = [System.Drawing.Color]::Transparent
                }
                $output.SetPixel($x, $y, $pixel)
            }
        }
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

# These are control assets, not final Apogean art. They preserve the official ExampleMod
# frame topology exactly so client rendering can be tested before any custom pixel work.
New-NormalizedCopy 'Tools/Templates/TerrariaTerrainFrameMask.png' 'Content/Tiles/Diagnostics/TileLabBlock.png' $true
New-NormalizedCopy 'Tools/Templates/TerrariaWallFrameMask.png' 'Content/Walls/Diagnostics/TileLabWall.png' $false

Write-Host 'Generated the Tile Lab control assets.' -ForegroundColor Green
