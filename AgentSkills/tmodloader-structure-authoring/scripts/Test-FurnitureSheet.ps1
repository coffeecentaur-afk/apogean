param(
    [Parameter(Mandatory = $true)][string]$Sheet,
    [Parameter(Mandatory = $true)][ValidateRange(1, 32)][int]$ObjectWidthTiles,
    [Parameter(Mandatory = $true)][ValidateRange(1, 32)][int]$ObjectHeightTiles,
    [int[]]$CoordinateHeights = @(),
    [ValidateRange(0, 8)][int]$Padding = 2,
    [ValidateRange(1, 128)][int]$Styles = 1,
    [ValidateRange(1, 128)][int]$AnimationFrames = 1,
    [ValidateSet('Horizontal', 'Vertical')][string]$StyleLayout = 'Horizontal',
    [ValidateRange(0, 64)][int]$ExtraFrameWidth = 0,
    [ValidateRange(0, 64)][int]$ExtraFrameHeight = 0,
    [int]$MaximumOpaqueColors = 32
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()
if ($CoordinateHeights.Count -eq 0) {
    $CoordinateHeights = for ($i = 0; $i -lt $ObjectHeightTiles; $i++) { 16 }
}
if ($CoordinateHeights.Count -ne $ObjectHeightTiles) {
    throw "CoordinateHeights count $($CoordinateHeights.Count) must equal ObjectHeightTiles $ObjectHeightTiles"
}
$frameWidth = $ObjectWidthTiles * (16 + $Padding) + $ExtraFrameWidth
$frameHeight = ($CoordinateHeights | Measure-Object -Sum).Sum + $ObjectHeightTiles * $Padding + $ExtraFrameHeight
$expectedWidth = if ($StyleLayout -eq 'Horizontal') { $frameWidth * $Styles } else { $frameWidth }
$expectedHeight = if ($StyleLayout -eq 'Vertical') { $frameHeight * $Styles * $AnimationFrames } else { $frameHeight * $AnimationFrames }
$bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Sheet).Path)
try {
    if ($bitmap.Width -ne $expectedWidth -or $bitmap.Height -ne $expectedHeight) {
        $failures.Add("sheet is $($bitmap.Width)x$($bitmap.Height); expected ${expectedWidth}x${expectedHeight}")
    }
    $colors = [Collections.Generic.HashSet[int]]::new()
    $soft = 0
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $soft++ }
            if ($pixel.A -eq 255) { [void]$colors.Add($pixel.ToArgb()) }
        }
    }
    if ($soft -gt 0) { $failures.Add("$soft soft-alpha pixels") }
    if ($colors.Count -gt $MaximumOpaqueColors) { $failures.Add("$($colors.Count) opaque colors; maximum is $MaximumOpaqueColors") }
}
finally { $bitmap.Dispose() }
if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host "PASS: furniture sheet matches ${ObjectWidthTiles}x${ObjectHeightTiles} TileObjectData, $Styles style(s), and $AnimationFrames animation frame(s)." -ForegroundColor Green
