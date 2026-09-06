param(
    [Parameter(Mandatory)][string]$ReferencePath,
    [Parameter(Mandatory)][string]$CandidatePath,
    [string[]]$AllowedRectangles = @(),
    [string]$ReportPath,
    [switch]$PreserveAlphaMask
)
# Read-only pre-install gate. This does not remove mattes, resize or approve art.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$refs = @([Drawing.Bitmap].Assembly.Location, [Drawing.Color].Assembly.Location,
    'System.Runtime', 'System.Collections')
$refs += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
public static class BackgroundReplacementProbe {
    public static long[] Inspect(string reference, string candidate, int[] boxes) {
        using(var a = new Bitmap(reference)) using(var b = new Bitmap(candidate)) {
            long empty=0, soft=0, changedOutside=0, changedInside=0, changedAlpha=0;
            bool sameSize=a.Width==b.Width && a.Height==b.Height;
            for(int y=0;y<b.Height;y++) for(int x=0;x<b.Width;x++) {
                Color pixel=b.GetPixel(x,y);
                if(pixel.A==0) empty++; else if(pixel.A!=255) soft++;
                if(!sameSize) continue;
                Color old=a.GetPixel(x,y);
                if(old.A!=pixel.A)changedAlpha++;
                if(old.A==pixel.A && (pixel.A==0 || old.ToArgb()==pixel.ToArgb())) continue;
                bool allowed=false;
                for(int i=0;i<boxes.Length;i+=4)
                    allowed |= x>=boxes[i] && y>=boxes[i+1] &&
                        x<(long)boxes[i]+boxes[i+2] && y<(long)boxes[i+1]+boxes[i+3];
                if(allowed) changedInside++; else changedOutside++;
            }
            return new long[]{a.Width,a.Height,b.Width,b.Height,empty,soft,changedOutside,changedInside,changedAlpha};
        }
    }
}
'@
$boxes = [Collections.Generic.List[int]]::new()
foreach ($rectangle in $AllowedRectangles) {
    if ($rectangle -notmatch '^\d+,\d+,[1-9]\d*,[1-9]\d*$') { throw "BAD_RECTANGLE: $rectangle (expected x,y,width,height)" }
    foreach ($number in $rectangle.Split(',')) { $boxes.Add([int]$number) }
}
$values = [BackgroundReplacementProbe]::Inspect(
    (Resolve-Path -LiteralPath $ReferencePath).Path,
    (Resolve-Path -LiteralPath $CandidatePath).Path, $boxes.ToArray())
$failures = [Collections.Generic.List[string]]::new()
if ($values[0] -ne $values[2] -or $values[1] -ne $values[3]) { $failures.Add('DIMENSIONS_CHANGED') }
if ($values[4] -eq 0) { $failures.Add('NO_TRANSPARENT_PIXELS') }
if ($values[5] -ne 0) { $failures.Add('SOFT_ALPHA') }
if ($values[6] -ne 0) { $failures.Add('PIXELS_CHANGED_OUTSIDE_APPROVED_REGIONS') }
if ($PreserveAlphaMask -and $values[8] -ne 0) { $failures.Add('ALPHA_MASK_CHANGED') }
$report = [ordered]@{
    referenceSize = "$($values[0])x$($values[1])"
    candidateSize = "$($values[2])x$($values[3])"
    transparentPixels = $values[4]
    partialAlphaPixels = $values[5]
    changedOutsideApprovedRegions = $(if ($failures.Contains('DIMENSIONS_CHANGED')) { $null } else { $values[6] })
    changedInsideApprovedRegions = $(if ($failures.Contains('DIMENSIONS_CHANGED')) { $null } else { $values[7] })
    changedAlphaPixels = $(if ($failures.Contains('DIMENSIONS_CHANGED')) { $null } else { $values[8] })
    alphaMaskPreservationRequired = [bool]$PreserveAlphaMask
    referenceSHA256 = (Get-FileHash -LiteralPath $ReferencePath -Algorithm SHA256).Hash
    candidateSHA256 = (Get-FileHash -LiteralPath $CandidatePath -Algorithm SHA256).Hash
    failures = $failures.ToArray()
    pass = $failures.Count -eq 0
    scope = 'Export invariants only. A pass does not approve silhouette, material, connected branches, scene routing or live rendering.'
} | ConvertTo-Json -Depth 4
if ($ReportPath) {
    $output = [IO.Path]::GetFullPath($ReportPath)
    if ($output -eq (Resolve-Path -LiteralPath $ReferencePath).Path -or $output -eq (Resolve-Path -LiteralPath $CandidatePath).Path) {
        throw 'REPORT_MUST_NOT_OVERWRITE_INPUT'
    }
    [IO.File]::WriteAllText($output, $report)
}
Write-Output $report
if ($failures.Count -gt 0) { exit 1 }
