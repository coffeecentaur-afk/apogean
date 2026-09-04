param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Destination,
    [Parameter(Mandatory = $true)][ValidateCount(2, 32)][string[]]$Palette,
    [string[]]$TransparentColors = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Hex([string]$value) {
    $value = $value.Trim().TrimStart('#')
    if ($value -notmatch '^[0-9A-Fa-f]{6}$') { throw "Invalid RRGGBB color: $value" }
    [Drawing.ColorTranslator]::FromHtml("#$value")
}

$sourceBitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Source).Path)
$output = [Drawing.Bitmap]::new($sourceBitmap.Width, $sourceBitmap.Height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
$colors = @($Palette | ForEach-Object { Hex $_ })
$transparent = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($key in $TransparentColors) { [void]$transparent.Add($key.Trim().TrimStart('#')) }
try {
    for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
        for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
            $pixel = $sourceBitmap.GetPixel($x, $y)
            $key = '{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
            if ($pixel.A -eq 0 -or $transparent.Contains($key)) {
                $output.SetPixel($x, $y, [Drawing.Color]::Transparent)
                continue
            }
            if ($pixel.A -ne 255) { throw "Soft alpha $($pixel.A) at $x,$y" }
            $luma = 0.299 * $pixel.R + 0.587 * $pixel.G + 0.114 * $pixel.B
            $index = [Math]::Min($colors.Count - 1, [int][Math]::Floor(($luma / 256.0) * $colors.Count))
            $mapped = $colors[$index]
            $output.SetPixel($x, $y, [Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
        }
    }
    $path = if ([IO.Path]::IsPathRooted($Destination)) { $Destination } else { Join-Path (Get-Location) $Destination }
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    $output.Save($path, [Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Wrote $path"
}
finally {
    $sourceBitmap.Dispose()
    $output.Dispose()
}
