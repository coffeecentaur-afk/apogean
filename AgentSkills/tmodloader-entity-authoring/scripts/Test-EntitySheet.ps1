param(
    [Parameter(Mandatory = $true)][string]$Sheet,
    [Parameter(Mandatory = $true)][ValidateRange(1, 64)][int]$Frames,
    [int]$MinimumFrameWidth = 16,
    [int]$MinimumFrameHeight = 16,
    [int]$MinimumOpaqueWidth = 8,
    [int]$MinimumOpaqueHeight = 8,
    [int]$MaximumOpaqueColors = 24
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()
$bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Sheet).Path)
try {
    if ($bitmap.Height % $Frames -ne 0) { $failures.Add("Height $($bitmap.Height) is not divisible by $Frames") }
    else {
        $frameHeight = [int]($bitmap.Height / $Frames)
        if ($bitmap.Width -lt $MinimumFrameWidth) { $failures.Add("Frame width $($bitmap.Width) is below $MinimumFrameWidth") }
        if ($frameHeight -lt $MinimumFrameHeight) { $failures.Add("Frame height $frameHeight is below $MinimumFrameHeight") }
        $colors = [Collections.Generic.HashSet[int]]::new()
        for ($frame = 0; $frame -lt $Frames; $frame++) {
            $minX = $bitmap.Width; $minY = $frameHeight; $maxX = -1; $maxY = -1
            for ($localY = 0; $localY -lt $frameHeight; $localY++) {
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    $p = $bitmap.GetPixel($x, $frame * $frameHeight + $localY)
                    if ($p.A -gt 0 -and $p.A -lt 255) { $failures.Add("Soft alpha in frame $frame at $x,$localY") }
                    if ($p.A -ne 255) { continue }
                    [void]$colors.Add($p.ToArgb())
                    $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $localY); $maxY = [Math]::Max($maxY, $localY)
                }
            }
            $width = if ($maxX -ge 0) { $maxX - $minX + 1 } else { 0 }
            $height = if ($maxY -ge 0) { $maxY - $minY + 1 } else { 0 }
            if ($width -lt $MinimumOpaqueWidth -or $height -lt $MinimumOpaqueHeight) {
                $failures.Add("Frame $frame occupies ${width}x${height}")
            }
        }
        if ($colors.Count -gt $MaximumOpaqueColors) { $failures.Add("$($colors.Count) colors, maximum $MaximumOpaqueColors") }
    }
    if ($failures.Count) {
        $failures | Select-Object -Unique | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
        exit 1
    }
    Write-Host "PASS: $($bitmap.Width)x$($bitmap.Height), $Frames frames" -ForegroundColor Green
}
finally { $bitmap.Dispose() }
