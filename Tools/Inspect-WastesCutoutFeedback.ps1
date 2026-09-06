param([switch]$RequireClean)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$refs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Linq')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
public static class WastesCutoutFeedback {
    // Candidate-specific review probes, NOT a generic rule that forbids highlights.
    public static string[] Detached(string path,int x0,int y0,int width,int height) {
        using(var b=new Bitmap(path)) {
            bool[,] seen=new bool[width,height];var records=new List<string>();
            for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
                if(seen[x,y]||b.GetPixel(x0+x,y0+y).A==0)continue;
                var q=new Queue<Point>();q.Enqueue(new Point(x,y));seen[x,y]=true;
                int n=0,minX=x,maxX=x,minY=y,maxY=y;bool anchored=false;
                while(q.Count>0){var p=q.Dequeue();n++;minX=Math.Min(minX,p.X);maxX=Math.Max(maxX,p.X);minY=Math.Min(minY,p.Y);maxY=Math.Max(maxY,p.Y);anchored|=p.Y==height-1;
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                        int nx=p.X+dx,ny=p.Y+dy;
                        if(nx<0||ny<0||nx>=width||ny>=height||seen[nx,ny]||b.GetPixel(x0+nx,y0+ny).A==0)continue;
                        seen[nx,ny]=true;q.Enqueue(new Point(nx,ny));
                    }
                }
                if(!anchored&&n>=3)records.Add($"pixels={n}; bounds={x0+minX},{y0+minY},{maxX-minX+1},{maxY-minY+1}");
            }
            return records.ToArray();
        }
    }
    public static string[] PaleEdges(string path) {
        using(var b=new Bitmap(path)) {
            var records=new List<string>();
            // Only the upper silhouette. The opaque lower cliff is not in scope.
            for(int y=1;y<Math.Min(440,b.Height-1);y++)for(int x=1;x<b.Width-1;x++) {
                var c=b.GetPixel(x,y);int min=Math.Min(c.R,Math.Min(c.G,c.B)),max=Math.Max(c.R,Math.Max(c.G,c.B));
                if(c.A==0||min<120||max-min>65)continue;
                bool edge=false;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)edge|=b.GetPixel(x+dx,y+dy).A==0;
                if(edge)records.Add($"xy={x},{y}; rgb={c.R},{c.G},{c.B}; alpha={c.A}");
            }
            return records.ToArray();
        }
    }
}
'@
$repoRoot=Split-Path -Parent $PSScriptRoot
$runtime=Join-Path $repoRoot 'Content/Backgrounds/Candidates/WastesModules'
$left=@([WastesCutoutFeedback]::Detached((Join-Path $runtime 'Station.png'),30,365,56,75))
$right=@([WastesCutoutFeedback]::Detached((Join-Path $runtime 'Station.png'),410,350,60,91))
$pale=@([WastesCutoutFeedback]::PaleEdges((Join-Path $runtime 'Foreground-Deep.png')))
[pscustomobject]@{
    stationLeftDetachedComponents=$left
    stationRightDetachedComponents=$right
    foregroundPaleBoundaryCount=$pale.Count
    foregroundPaleBoundaryExamples=@($pale | Select-Object -First 16)
    scope='Read-only candidate-specific visual-review probes. Bright pixels require source/visual inspection; hard alpha is not edge-quality approval.'
} | ConvertTo-Json -Depth 5
if($RequireClean -and ($left.Count+$right.Count+$pale.Count -gt 0)){throw 'CUTOUT_REVIEW_REQUIRED: detached opaque branch islands / pale silhouette candidates remain in shipped QA PNGs'}
