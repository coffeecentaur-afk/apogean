param(
    [string]$Capture = 'Art/Validation/WastesLandscapeV1/Live/Horizon-ground-2560x1440.png',
    [int]$X = 1855, [int]$Y = 515, [int]$Width = 128, [int]$Height = 34
)
# Narrow image regression: daylight orange truck upper-body span, not a UI scale guess.
# Coordinates select the visibly unobscured truck in a recorded live fixture.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
# Use the loaded drawing API directly. Compiling against the .NET Framework
# System.Drawing facade fails under pwsh's System.Drawing.Common type forwarding.
function Measure-TruckSpan([string]$Path, [int]$Left, [int]$Top, [int]$W, [int]$H) {
    $bitmap = [Drawing.Bitmap]::new($Path)
    try {
        if ($Left -lt 0 -or $Top -lt 0 -or $W -le 0 -or $H -le 0 -or $Left -gt $bitmap.Width-$W -or $Top -gt $bitmap.Height-$H) {
            throw 'Fixture region must fit inside the capture.'
        }
        $minimum = [int]::MaxValue; $maximum = -1
        for ($a=$Left; $a -lt $Left+$W; $a++) {
            for ($b=$Top; $b -lt $Top+$H; $b++) {
                $p = $bitmap.GetPixel($a,$b)
                if ($p.A -gt 0 -and $p.R -gt 70 -and $p.R -gt $p.G*1.20 -and $p.R -gt $p.B*1.25) {
                    $minimum = [math]::Min($minimum,$a); $maximum = [math]::Max($maximum,$a)
                }
            }
        }
        if ($maximum -lt $minimum) { throw 'No truck pigment in selected fixture region' }
        return $maximum-$minimum+1
    }
    finally { $bitmap.Dispose() }
}
$root=Split-Path -Parent $PSScriptRoot
$source=Measure-TruckSpan (Join-Path $root 'Content/Backgrounds/Candidates/WastesV1/Mid.png') 460 193 125 35
$actual=Measure-TruckSpan (Join-Path $root $Capture) $X $Y $Width $Height
$scale=$actual/[double]$source
Write-Host "Truck body: source=$source px; live=$actual px; measured scale=$([math]::Round($scale,3))"
if ([math]::Abs($actual-$source) -gt 3) { throw 'FAIL: the live landmark is enlarged despite Draw(scale:1).' }
Write-Host 'PASS: daylight truck pigment span is source-sized within 3 pixels. This is not a full renderer-quality test.'
