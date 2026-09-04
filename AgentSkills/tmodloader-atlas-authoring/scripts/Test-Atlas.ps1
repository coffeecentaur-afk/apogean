param(
    [Parameter(Mandatory = $true)][string]$Atlas,
    [string]$ReferenceAtlas,
    [int]$ExpectedWidth = 0,
    [int]$ExpectedHeight = 0,
    [int]$MaximumOpaqueColors = 16,
    [switch]$AllowOpaqueWhite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()
$bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Atlas).Path)
try {
    if ($ExpectedWidth -gt 0 -and $bitmap.Width -ne $ExpectedWidth) { $failures.Add("Wrong width: $($bitmap.Width)") }
    if ($ExpectedHeight -gt 0 -and $bitmap.Height -ne $ExpectedHeight) { $failures.Add("Wrong height: $($bitmap.Height)") }
    $colors = [Collections.Generic.HashSet[int]]::new()
    $white = 0
    $soft = 0
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            $p = $bitmap.GetPixel($x, $y)
            if ($p.A -gt 0 -and $p.A -lt 255) { $soft++ }
            if ($p.A -eq 255) {
                [void]$colors.Add($p.ToArgb())
                if ($p.R -eq 255 -and $p.G -eq 255 -and $p.B -eq 255) { $white++ }
            }
        }
    }
    if ($soft) { $failures.Add("$soft soft-alpha pixels") }
    if (-not $AllowOpaqueWhite -and $white) { $failures.Add("$white opaque-white pixels") }
    if ($colors.Count -gt $MaximumOpaqueColors) { $failures.Add("$($colors.Count) colors, maximum $MaximumOpaqueColors") }
    if ($ReferenceAtlas) {
        $reference = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $ReferenceAtlas).Path)
        try {
            if ($reference.Size -ne $bitmap.Size) { $failures.Add('Reference dimensions differ') }
            else {
                $mismatch = 0
                for ($y = 0; $y -lt $bitmap.Height; $y++) {
                    for ($x = 0; $x -lt $bitmap.Width; $x++) {
                        if (($reference.GetPixel($x, $y).A -gt 0) -ne ($bitmap.GetPixel($x, $y).A -gt 0)) { $mismatch++ }
                    }
                }
                if ($mismatch) { $failures.Add("$mismatch alpha-topology mismatches") }
            }
        }
        finally { $reference.Dispose() }
    }
    if ($failures.Count) {
        $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
        exit 1
    }
    Write-Host "PASS: $($bitmap.Width)x$($bitmap.Height), $($colors.Count) colors" -ForegroundColor Green
}
finally { $bitmap.Dispose() }
