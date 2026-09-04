param(
    [Parameter(Mandatory = $true)][string]$Far,
    [Parameter(Mandatory = $true)][string]$Mid,
    [Parameter(Mandatory = $true)][string]$Close,
    [int]$MinimumWidth = 1920,
    [int]$MinimumHeight = 720,
    [int]$MaximumAxis = 4096,
    [int]$MinimumSampledColors = 96,
    [int64]$MaximumRawBytes = 33554432,
    [switch]$RequireMatchingHorizontalEdges
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()
$rawBytes = [int64]0

foreach ($entry in @(@('Far', $Far), @('Mid', $Mid), @('Close', $Close))) {
    $label = [string]$entry[0]
    $path = (Resolve-Path -LiteralPath ([string]$entry[1])).Path
    $bitmap = [Drawing.Bitmap]::new($path)
    try {
        if ($bitmap.Width -lt $MinimumWidth -or $bitmap.Height -lt $MinimumHeight) {
            $failures.Add("$label is $($bitmap.Width)x$($bitmap.Height); minimum is ${MinimumWidth}x${MinimumHeight}")
        }
        if ($bitmap.Width -gt $MaximumAxis -or $bitmap.Height -gt $MaximumAxis) {
            $failures.Add("$label exceeds the ${MaximumAxis}px conservative texture-axis limit")
        }
        $rawBytes += [int64]$bitmap.Width * $bitmap.Height * 4
        $colors = [Collections.Generic.HashSet[int]]::new()
        $softAlpha = 0
        for ($y = 0; $y -lt $bitmap.Height; $y += 4) {
            for ($x = 0; $x -lt $bitmap.Width; $x += 4) {
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $softAlpha++ }
                if ($pixel.A -eq 255) { [void]$colors.Add($pixel.ToArgb()) }
            }
        }
        if ($softAlpha -gt 0) { $failures.Add("$label has sampled soft-alpha pixels") }
        if ($colors.Count -lt $MinimumSampledColors) { $failures.Add("$label has only $($colors.Count) sampled opaque colors") }
        if ($RequireMatchingHorizontalEdges) {
            for ($y = 0; $y -lt $bitmap.Height; $y++) {
                if ($bitmap.GetPixel(0, $y).ToArgb() -ne $bitmap.GetPixel($bitmap.Width - 1, $y).ToArgb()) {
                    $failures.Add("$label horizontal edges differ at row $y")
                    break
                }
            }
        }
    }
    finally { $bitmap.Dispose() }
}

if ($rawBytes -gt $MaximumRawBytes) {
    $failures.Add("Set consumes $([Math]::Round($rawBytes / 1MB, 2)) MiB raw RGBA; maximum is $([Math]::Round($MaximumRawBytes / 1MB, 2)) MiB")
}
if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host "PASS: three background layers meet the requested asset and memory contracts ($([Math]::Round($rawBytes / 1MB, 2)) MiB). Camera and routing validation remain required." -ForegroundColor Green
