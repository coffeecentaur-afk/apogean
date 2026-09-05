param(
    [Parameter(Mandatory = $true)][string]$Path,
    [int]$MinimumWidth = 2048,
    [int]$MinimumHeight = 720,
    [int]$MaximumAxis = 4096,
    [int]$SkyRows = 2,
    [int]$GroundRows = 8,
    [switch]$RequireMatchingHorizontalEdges
)

# Read-only pre-import gate. Generated checkerboards are RGB artwork, not alpha.
# Pixel count, palette size and matching outer columns do not prove good art.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($MinimumWidth -lt 1 -or $MinimumHeight -lt 1 -or $MaximumAxis -lt 1 -or
    $SkyRows -lt 1 -or $GroundRows -lt 1) { throw 'Dimensions and inspection bands must be positive.' }
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public sealed class SurfaceLayerInspection
{
    public int Width, Height;
    public long TransparentPixels, OpaquePixels, SoftPixels;
    public long NontransparentSkyPixels, NonopaqueGroundPixels, DifferingEdgeRows;
    public string PixelFormat;

    public static SurfaceLayerInspection Read(string path, int skyRows, int groundRows)
    {
        using (var source = new Bitmap(path))
        using (var bitmap = source.Clone(new Rectangle(0, 0, source.Width, source.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        {
            var result = new SurfaceLayerInspection { Width = source.Width, Height = source.Height,
                PixelFormat = source.PixelFormat.ToString() };
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                var row = new byte[bitmap.Width * 4];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int alpha = row[x * 4 + 3];
                        if (alpha == 0) result.TransparentPixels++;
                        else if (alpha == 255) result.OpaquePixels++;
                        else result.SoftPixels++;
                        if (y < skyRows && alpha != 0) result.NontransparentSkyPixels++;
                        if (y >= bitmap.Height - groundRows && alpha != 255) result.NonopaqueGroundPixels++;
                    }
                    int last = (bitmap.Width - 1) * 4;
                    bool equal = row[3] == row[last + 3];
                    // Hidden RGB values beneath fully transparent pixels do not affect joins.
                    if (equal && row[3] != 0)
                        equal = row[0] == row[last] && row[1] == row[last + 1] && row[2] == row[last + 2];
                    if (!equal) result.DifferingEdgeRows++;
                }
            }
            finally { bitmap.UnlockBits(data); }
            return result;
        }
    }
}
'@

$resolved = (Resolve-Path -LiteralPath $Path).Path
$result = [SurfaceLayerInspection]::Read($resolved, $SkyRows, $GroundRows)
$failures = [Collections.Generic.List[string]]::new()
if ($result.Width -lt $MinimumWidth -or $result.Height -lt $MinimumHeight) {
    $failures.Add("Actual $($result.Width)x$($result.Height); required at least ${MinimumWidth}x${MinimumHeight}. No upscale credit.")
}
if ($result.Width -gt $MaximumAxis -or $result.Height -gt $MaximumAxis) { $failures.Add('Texture exceeds configured axis budget.') }
if ($SkyRows + $GroundRows -gt $result.Height) { $failures.Add('Inspection bands overlap; invalid asset contract.') }
if ($result.TransparentPixels -eq 0) { $failures.Add('No transparent pixels: a painted checkerboard or matte is not transparency.') }
if ($result.OpaquePixels -eq 0) { $failures.Add('No opaque landscape pixels.') }
if ($result.SoftPixels -ne 0) { $failures.Add("$($result.SoftPixels) soft-alpha pixels; this pixel-art contract requires hard alpha.") }
if ($result.NontransparentSkyPixels -ne 0) { $failures.Add('Top sky band paints over the engine sky.') }
if ($result.NonopaqueGroundPixels -ne 0) { $failures.Add('Bottom ground band has holes; renderer coverage is not established.') }
if ($RequireMatchingHorizontalEdges -and $result.DifferingEdgeRows -ne 0) { $failures.Add('Horizontal edge columns differ.') }

[pscustomobject]@{
    File = $resolved
    SHA256 = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
    Inspection = $result
    RawRgbaMiB = [Math]::Round([int64]$result.Width * $result.Height * 4 / 1MB, 3)
    Passed = $failures.Count -eq 0
    Failures = @($failures.ToArray())
    Scope = 'Static export only. Does not prove source detail, internal matte removal, seamless composition, parallax, lighting, routing or live coverage.'
} | ConvertTo-Json -Depth 4
if ($failures.Count -ne 0) { exit 1 }
