param(
    [string]$Directory = (Join-Path $PSScriptRoot '../Art/Candidates/ArrivalPod-v1/Native-v1'),
    [ValidateSet(1,2)][int]$PixelClusterSize = 1
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$nativePath = Join-Path $Directory 'ArrivalPod.png'
$sheetPath = Join-Path $Directory 'ArrivalPod_Tile.png'
if (!(Test-Path -LiteralPath $nativePath) -or !(Test-Path -LiteralPath $sheetPath)) { throw 'NATIVE_ART_MISSING' }
$art = [Drawing.Bitmap]::new((Resolve-Path $nativePath).Path)
$sheet = [Drawing.Bitmap]::new((Resolve-Path $sheetPath).Path)
try {
    if ($art.Width -ne 80 -or $art.Height -ne 96) { throw 'NATIVE_DIMENSIONS' }
    if ($sheet.Width -ne 90 -or $sheet.Height -ne 108) { throw 'SHEET_DIMENSIONS' }
    $colors = [Collections.Generic.HashSet[int]]::new()
    $points = [Collections.Generic.HashSet[int]]::new()
    $floor = 0
    $transparent = 0
    for ($y=0; $y -lt 96; $y++) {
        for ($x=0; $x -lt 80; $x++) {
            $p = $art.GetPixel($x,$y)
            if ($p.A -ne 0 -and $p.A -ne 255) { throw 'SOFT_ALPHA' }
            if ($p.A -eq 0) {
                $transparent++
                if ($p.R -ne 0 -or $p.G -ne 0 -or $p.B -ne 0) { throw 'HIDDEN_MATTE_RGB' }
            } else {
                if (($p.R -eq 255 -and $p.G -eq 255 -and $p.B -eq 255) -or
                    ($p.R -eq 255 -and $p.G -eq 0 -and $p.B -eq 255)) { throw 'EXPORTER_KEY_COLOR' }
                [void]$colors.Add($p.ToArgb())
                [void]$points.Add($y*80+$x)
                if ($y -eq 95) { $floor++ }
            }
            $sx = [int][Math]::Floor($x/16)*18 + ($x%16)
            $sy = [int][Math]::Floor($y/16)*18 + ($y%16)
            if ($sheet.GetPixel($sx,$sy).ToArgb() -ne $p.ToArgb()) { throw 'FRAME_ROUNDTRIP' }
        }
    }
    if ($points.Count -lt 3000 -or $transparent -lt 500) { throw 'SILHOUETTE_OCCUPANCY' }
    if ($colors.Count -gt 20) { throw 'PALETTE_BUDGET' }
    if ($floor -lt 12) { throw 'FLOATING_FOOT' }
    for ($y=0; $y -lt 108; $y++) {
        for ($x=0; $x -lt 90; $x++) {
            if (($x%18 -ge 16 -or $y%18 -ge 16) -and $sheet.GetPixel($x,$y).ToArgb() -ne 0) { throw 'NONEMPTY_PADDING' }
        }
    }
    # Eight-connected pixel clusters allow diagonal stair steps but not a detached hatch/aerial.
    $remaining = [Collections.Generic.HashSet[int]]::new($points)
    $queue = [Collections.Generic.Queue[int]]::new()
    $seed = @($remaining)[0]
    [void]$remaining.Remove($seed)
    $queue.Enqueue($seed)
    while ($queue.Count -gt 0) {
        $v=$queue.Dequeue(); $x=$v%80; $y=[int][Math]::Floor($v/80)
        for ($dy=-1; $dy -le 1; $dy++) {
            for ($dx=-1; $dx -le 1; $dx++) {
                $nx=$x+$dx; $ny=$y+$dy
                if ($nx -lt 0 -or $nx -ge 80 -or $ny -lt 0 -or $ny -ge 96) { continue }
                $n=$ny*80+$nx
                if ($remaining.Remove($n)) { $queue.Enqueue($n) }
            }
        }
    }
    if ($remaining.Count -ne 0) { throw 'DETACHED_PIXELS' }
    # Coarse candidates must use an actual shared logical pixel grid, not merely
    # fewer colors in the same dense texture. Includes transparent edge cells.
    for ($y=0; $y -lt 96; $y++) {
        for ($x=0; $x -lt 80; $x++) {
            $cx = $x - ($x % $PixelClusterSize)
            $cy = $y - ($y % $PixelClusterSize)
            if ($art.GetPixel($x,$y).ToArgb() -ne $art.GetPixel($cx,$cy).ToArgb()) {
                throw "COARSE_PIXEL_GRID at $x,$y"
            }
        }
    }
    Write-Output "PASS: 80x96 art; 90x108 sheet; 30 cells round-trip exactly; $($colors.Count) colors; $floor grounded pixels; one connected silhouette; $PixelClusterSize-pixel grid. Static only, not native renderer proof."
} finally { $art.Dispose(); $sheet.Dispose() }
