param(
    [Parameter(Mandatory = $true)]
    [string]$Sheet,
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, 100)]
    [int]$Frames,
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
$path = (Resolve-Path -LiteralPath $Sheet).Path
$bitmap = [Drawing.Bitmap]::new($path)
try {
    if ($bitmap.Height % $Frames -ne 0) {
        $failures.Add("Sheet height $($bitmap.Height) is not divisible by $Frames frames")
    }
    else {
        $frameHeight = [int]($bitmap.Height / $Frames)
        if ($bitmap.Width -lt $MinimumFrameWidth) { $failures.Add("Frame width $($bitmap.Width) is below $MinimumFrameWidth") }
        if ($frameHeight -lt $MinimumFrameHeight) { $failures.Add("Frame height $frameHeight is below $MinimumFrameHeight") }

        $colors = [Collections.Generic.HashSet[int]]::new()
        $soft = 0
        for ($frame = 0; $frame -lt $Frames; $frame++) {
            $minX = $bitmap.Width
            $minY = $frameHeight
            $maxX = -1
            $maxY = -1
            for ($localY = 0; $localY -lt $frameHeight; $localY++) {
                $y = $frame * $frameHeight + $localY
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    $pixel = $bitmap.GetPixel($x, $y)
                    if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $soft++ }
                    if ($pixel.A -ne 255) { continue }
                    [void]$colors.Add($pixel.ToArgb())
                    $minX = [Math]::Min($minX, $x)
                    $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $localY)
                    $maxY = [Math]::Max($maxY, $localY)
                }
            }
            $opaqueWidth = if ($maxX -ge 0) { $maxX - $minX + 1 } else { 0 }
            $opaqueHeight = if ($maxY -ge 0) { $maxY - $minY + 1 } else { 0 }
            if ($opaqueWidth -lt $MinimumOpaqueWidth -or $opaqueHeight -lt $MinimumOpaqueHeight) {
                $failures.Add("Frame $frame silhouette is ${opaqueWidth}x${opaqueHeight}; require at least ${MinimumOpaqueWidth}x${MinimumOpaqueHeight}")
            }
        }
        if ($soft -gt 0) { $failures.Add("Sheet contains $soft soft-alpha pixels") }
        if ($colors.Count -gt $MaximumOpaqueColors) { $failures.Add("Sheet uses $($colors.Count) opaque colors; maximum is $MaximumOpaqueColors") }
    }

    if ($failures.Count -gt 0) {
        Write-Host "TMODLOADER ENTITY SHEET: FAIL ($($failures.Count) problems)" -ForegroundColor Red
        foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
        exit 1
    }
    Write-Host "TMODLOADER ENTITY SHEET: PASS — $($bitmap.Width)x$($bitmap.Height), $Frames frames" -ForegroundColor Green
}
finally {
    $bitmap.Dispose()
}
