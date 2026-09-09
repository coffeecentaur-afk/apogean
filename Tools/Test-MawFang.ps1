param([string]$CandidateDirectory = '')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$geometry = Join-Path $root 'Common/Geometry/MawFangShape.cs'
if (-not (Test-Path -LiteralPath $geometry)) { throw 'MISSING_FANG_CONTRACT_IMPLEMENTATION' }
Add-Type -TypeDefinition (Get-Content -LiteralPath $geometry -Raw)
$type = [apogean.Common.Geometry.MawFangShape]
foreach ($o in 0..3) {
    $count = 0
    foreach ($y in 0..2) { foreach ($x in 0..2) {
        $s = $type::Slope($x,$y,$o)
        if ($s -lt 0) { continue }
        $count++
        $fx = $x*18; $fy = ($o*3+$y)*18
        $decoded = $type::Decode($fx,$fy)
        if ($decoded -ne ($o*9+$y*3+$x)) { throw 'FRAME_ROUNDTRIP' }
    }}
    if ($count -ne 5) { throw 'OCCUPANCY_COUNT' }
}
if ($type::Decode(1,0) -ne -1 -or $type::Decode(0,216) -ne -1 -or $type::Decode(36,0) -ne -1) { throw 'BAD_FRAME_ACCEPTED' }
# Independent truth table, not a second call to the implementation as oracle.
foreach ($s in 0..4) {
    foreach ($y in 0..15) { foreach ($x in 0..15) {
        $expected = switch ($s) { 0 { $true }; 1 { $y -ge $x }; 2 { $y -ge (15-$x) }; 3 { $y -le (15-$x) }; 4 { $y -le $x } }
        if ($type::PixelSolid($x,$y,$s) -ne $expected) { throw "PIXEL_GEOMETRY $s,$x,$y" }
    }}
}
if ($type::Touches(10,1,11,2,1,0) -or -not $type::Touches(1,10,2,11,1,0)) { throw 'SLOPE_HURT_GEOMETRY' }
if ($type::Touches(17,17,18,18,0,0) -or $type::Touches(-3,-3,-2,-2,0,0)) { throw 'REMOTE_CONTACT' }
Write-Host 'PASS: four rotations, 20 frame roundtrips, invalid frames rejected, 1280 independent pixel geometry cases, sloped contact.'
if ($CandidateDirectory) {
    Add-Type -AssemblyName System.Drawing
    $asset = Join-Path (Resolve-Path -LiteralPath $CandidateDirectory).Path 'MawFangTile.png'
    if (-not (Test-Path -LiteralPath $asset)) { throw 'MISSING_FANG_ATLAS' }
    $bitmap = [Drawing.Bitmap]::new($asset)
    try {
        if ($bitmap.Width -ne 54 -or $bitmap.Height -ne 216) { throw 'FANG_ATLAS_DIMENSIONS' }
        $colors = [Collections.Generic.HashSet[int]]::new()
        foreach ($y in 0..215) { foreach ($x in 0..53) {
            $o = [int][Math]::Floor($y/54); $cy = [int][Math]::Floor(($y%54)/18); $cx = [int][Math]::Floor($x/18)
            $px = $x%18; $py = $y%18; $s=$type::Slope($cx,$cy,$o)
            $solid=$s -ge 0 -and $px -lt 16 -and $py -lt 16 -and $type::PixelSolid($px,$py,$s)
            $c=$bitmap.GetPixel($x,$y)
            if (($solid -and $c.A -ne 255) -or (-not $solid -and $c.ToArgb() -ne 0)) { throw "FANG_ALPHA_OCCUPANCY $x,$y" }
            if ($solid) { $null=$colors.Add($c.ToArgb()) }
        }}
        if ($colors.Count -lt 3 -or $colors.Count -gt 8) { throw 'FANG_PALETTE' }
        Write-Host "PASS: exact 54x216 occupancy, hard alpha, zero-RGB padding, $($colors.Count) opaque colors. Art and native physics still require review."
    } finally { $bitmap.Dispose() }
}
