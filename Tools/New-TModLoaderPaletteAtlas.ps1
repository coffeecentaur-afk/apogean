param(
    [Parameter(Mandatory = $true)]
    [string]$Source,
    [Parameter(Mandatory = $true)]
    [string]$Destination,
    [Parameter(Mandatory = $true)]
    [ValidateCount(2, 32)]
    [string[]]$Palette,
    [string[]]$TransparentColors = @(),
    [switch]$AllowSoftAlpha
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Resolve-PathForRead([string]$path) {
    (Resolve-Path -LiteralPath $path).Path
}

function Resolve-PathForWrite([string]$path) {
    if ([IO.Path]::IsPathRooted($path)) { return [IO.Path]::GetFullPath($path) }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $path))
}

function Convert-HexColor([string]$hex) {
    $normalized = $hex.Trim().TrimStart('#')
    if ($normalized -notmatch '^[0-9A-Fa-f]{6}$') {
        throw "Palette color '$hex' must be RRGGBB or #RRGGBB."
    }
    [Drawing.ColorTranslator]::FromHtml("#$normalized")
}

$sourcePath = Resolve-PathForRead $Source
$destinationPath = Resolve-PathForWrite $Destination
$colors = @($Palette | ForEach-Object { Convert-HexColor $_ })
$transparentKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($key in $TransparentColors) {
    [void]$transparentKeys.Add($key.Trim().TrimStart('#'))
}

$sourceBitmap = [Drawing.Bitmap]::new($sourcePath)
$output = [Drawing.Bitmap]::new($sourceBitmap.Width, $sourceBitmap.Height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
        for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
            $pixel = $sourceBitmap.GetPixel($x, $y)
            if ($pixel.A -eq 0) {
                $output.SetPixel($x, $y, [Drawing.Color]::Transparent)
                continue
            }
            if (-not $AllowSoftAlpha -and $pixel.A -ne 255) {
                throw "Source contains soft alpha $($pixel.A) at $x,$y. Normalize the source or pass -AllowSoftAlpha deliberately."
            }

            $sourceKey = '{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
            if ($transparentKeys.Contains($sourceKey)) {
                $output.SetPixel($x, $y, [Drawing.Color]::Transparent)
                continue
            }

            # Luminance ranks source roles while preserving the source atlas's exact
            # silhouettes, padding, slopes, and connection topology.
            $luminance = 0.299 * $pixel.R + 0.587 * $pixel.G + 0.114 * $pixel.B
            $index = [Math]::Min($colors.Count - 1, [int][Math]::Floor(($luminance / 256.0) * $colors.Count))
            $mapped = $colors[$index]
            $output.SetPixel($x, $y, [Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
        }
    }

    $directory = Split-Path -Parent $destinationPath
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $output.Save($destinationPath, [Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $sourceBitmap.Dispose()
    $output.Dispose()
}

Write-Host "Compiled native-topology atlas: $destinationPath" -ForegroundColor Green
