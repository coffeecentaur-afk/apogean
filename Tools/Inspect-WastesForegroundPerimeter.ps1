param(
    [string]$AssetPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1/Foreground-Deep.png'),
    [string]$ReportPath,
    [switch]$RequireNoReviewCandidates
)
# Read-only source inspection. Review candidates are not automatically bad pixels.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$refs = @([Drawing.Bitmap].Assembly.Location, [Drawing.Color].Assembly.Location,
    'System.Runtime', 'System.Collections', 'System.Linq', 'System.Text.Json')
$refs += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
public static class WastesPerimeterInspection {
    public static string Inspect(string path) {
        using(var b = new Bitmap(path)) {
            var points = new List<object>();
            long transparent = 0, partial = 0, transparentRgb = 0, boundary = 0;
            int paleNeutral = 0, belowOldProbe = 0;
            for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++) {
                var c=b.GetPixel(x,y);
                if(c.A==0) { transparent++; if(c.R+c.G+c.B>0)transparentRgb++; continue; }
                if(c.A!=255)partial++;
                bool edge=false;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    int nx=x+dx, ny=y+dy;
                    if(nx>=0&&ny>=0&&nx<b.Width&&ny<b.Height&&b.GetPixel(nx,ny).A==0)edge=true;
                }
                if(!edge)continue;
                boundary++;
                int min=Math.Min(c.R,Math.Min(c.G,c.B)),max=Math.Max(c.R,Math.Max(c.G,c.B));
                // A review aid for neutral fringes, not a prohibition on highlights.
                if(c.A==255&&min>=100&&max-min<=40) {
                    paleNeutral++; if(y>=440)belowOldProbe++;
                    if(y>=440&&points.Count<20)points.Add(new {x,y,r=c.R,g=c.G,b=c.B,a=c.A});
                }
            }
            return JsonSerializer.Serialize(new {width=b.Width,height=b.Height,
                transparent,partialAlpha=partial,transparentWithNonzeroRgb=transparentRgb,
                opaqueBoundaryPixels=boundary,paleNeutralBoundaryCandidates=paleNeutral,
                candidatesBelowOld440RowProbe=belowOldProbe,examplesBelowOldProbe=points,
                scope="Read-only full-perimeter inspection; no artwork, color, alpha or game state changed. Counts are review aids, not automatic deletion targets. Transparent RGB is not proof of runtime leakage without the loader/sampler path."},
                new JsonSerializerOptions{WriteIndented=true});
        }
    }
}
'@
$resolved = (Resolve-Path -LiteralPath $AssetPath).Path
$report = [WastesPerimeterInspection]::Inspect($resolved) | ConvertFrom-Json
$report | Add-Member -NotePropertyName sourceSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $resolved).Hash
$json = $report | ConvertTo-Json -Depth 5
if ($ReportPath) {
    $target = [IO.Path]::GetFullPath($ReportPath)
    if ($target -eq $resolved -or [IO.Path]::GetExtension($target) -ne '.json') { throw 'Report must be a separate JSON file.' }
    [IO.File]::WriteAllText($target,$json)
}
$json
if ($RequireNoReviewCandidates -and $report.paleNeutralBoundaryCandidates -gt 0) {
    throw 'PERIMETER_REVIEW_REQUIRED: source has opaque pale-neutral boundary pixels; inspect material before repairing.'
}
