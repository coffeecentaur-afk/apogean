param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
function Assert-SameVisiblePixel($actual, $expected, [string]$location) {
    if ($actual.A -ne $expected.A -or ($actual.A -ne 0 -and $actual.ToArgb() -ne $expected.ToArgb())) {
        throw "native/whole mismatch at $location"
    }
}
# Prove the pixel predicate rejects visible damage without modifying any asset.
foreach ($mutation in @('alpha', 'color')) {
    $expected = [System.Drawing.Color]::FromArgb(255, 53, 38, 31)
    $actual = if ($mutation -eq 'alpha') { [System.Drawing.Color]::FromArgb(0, 53, 38, 31) }
              else { [System.Drawing.Color]::FromArgb(255, 54, 38, 31) }
    $rejected = $false
    try { Assert-SameVisiblePixel $actual $expected "negative $mutation control" }
    catch { $rejected = $true }
    if (-not $rejected) { throw "Validator accepted the $mutation mutation" }
    Write-Host "PASS: rejects $mutation damage."
}
$cases = @(
    @{ Name='DeadTuft'; Width=32; Height=16; Styles=4 },
    @{ Name='WastesBristle'; Width=32; Height=48; Styles=3 },
    @{ Name='WastesRootShrub'; Width=48; Height=32; Styles=3 }
)
foreach ($case in $cases) {
    $atlas = [System.Drawing.Bitmap]::new((Join-Path $Root "Content/Tiles/$($case.Name).png"))
    $whole = [System.Drawing.Bitmap]::new((Join-Path $Root "Content/Tiles/$($case.Name)_Whole.png"))
    try {
        if ($whole.Width -ne $case.Width * $case.Styles -or $whole.Height -ne $case.Height) {
            throw "$($case.Name): unexpected whole-sprite dimensions"
        }
        $checked = 0
        for ($style=0; $style -lt $case.Styles; $style++) {
            for ($y=0; $y -lt $case.Height; $y++) {
                for ($x=0; $x -lt $case.Width; $x++) {
                    $ax = $style * ($case.Width / 16 * 18) + $x + 2 * [Math]::Floor($x / 16)
                    $ay = $y + 2 * [Math]::Floor($y / 16)
                    $a = $atlas.GetPixel($ax, $ay)
                    $b = $whole.GetPixel($style * $case.Width + $x, $y)
                    Assert-SameVisiblePixel $a $b "$($case.Name) style $style pixel $x,$y"
                    $checked++
                }
            }
        }
        Write-Host "PASS $($case.Name): $checked native atlas pixels exactly reproduce the approved whole sprite."
    }
    finally { $atlas.Dispose(); $whole.Dispose() }
}
