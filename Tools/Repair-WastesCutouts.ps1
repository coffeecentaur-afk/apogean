param()
# Explicitly approved native-pixel repair. Candidate output only; never game files.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$baseline=Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/Derived'
$master=Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/Upper-Cliffs-Matte.png'
$output=Join-Path $repoRoot 'Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1'
$pins=@{
    (Join-Path $baseline 'Station.png')='85AF2EE81EBE380438DFBD4A3AFA2BB0D04501108CFB414E2F68C07FD53FCD51'
    (Join-Path $baseline 'Foreground-Deep.png')='9E240C6ABEBD7CA5A80CEEA796C867DA4F6B1FAC92256E25DDEFF780FE8CFF2B'
    $master='5EA3371525B2E86CA513A4EE65ADECEC4695139DC549ADE07B639BA8DA9BA57E'
}
foreach($path in $pins.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $pins[$path]){throw "SOURCE_CHANGED: $path"}}
New-Item -ItemType Directory -Path $output -Force | Out-Null
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
public static class WastesExactRepair {
    // These source-specific selections are NOT a matte key for other biomes.
    public static int Station(string input,string master,string output) {
        using(var old=new Bitmap(input)) using(var source=new Bitmap(master))
        using(var b=new Bitmap(old)) {
            var regions=new[]{new Rectangle(30,365,56,75),new Rectangle(410,350,60,91)};
            int count=0;
            foreach(var r in regions)for(int y=r.Top;y<r.Bottom;y++)for(int x=r.Left;x<r.Right;x++) {
                var a=old.GetPixel(x,y); var s=source.GetPixel(x+1048,y);
                // Recover the dark source silhouette excluded by the old broad
                // R-G/B-G key. Pure/saturated magenta stays transparent.
                bool wood=s.R<170 && s.B<150 && s.G<100;
                Color c=a;
                if(a.A==0 && wood)c=Color.FromArgb(255,Math.Min(s.R,(int)(s.G*1.65+12)),s.G,Math.Min(s.B,(int)(s.G*.85)));
                else if(a.A!=0 && wood && s.B>s.G+8 && s.R>s.G+15)
                    c=Color.FromArgb(255,Math.Min(s.R,(int)(s.G*1.65+12)),s.G,Math.Min(s.B,(int)(s.G*.85)));
                if(c.ToArgb()!=a.ToArgb()){b.SetPixel(x,y,c);count++;}
            }
            b.Save(output,ImageFormat.Png);return count;
        }
    }
    public static int Foreground(string input,string output) {
        using(var old=new Bitmap(input)) using(var b=new Bitmap(old)) {
            // Only reviewed exposed timber, not the highlighted rocks or turf.
            var regions=new[]{new Rectangle(310,195,190,90),new Rectangle(640,225,85,80),new Rectangle(1120,230,125,75)};
            int count=0;
            foreach(var r in regions)for(int y=r.Top;y<r.Bottom;y++)for(int x=r.Left;x<r.Right;x++) {
                var c=old.GetPixel(x,y);int min=Math.Min(c.R,Math.Min(c.G,c.B)),max=Math.Max(c.R,Math.Max(c.G,c.B));
                if(c.A==0||min<120||max-min>65)continue;
                bool edge=false;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)edge|=old.GetPixel(x+dx,y+dy).A==0;
                if(!edge)continue;
                Color best=c;int distance=999;
                for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++) {
                    var p=old.GetPixel(x+dx,y+dy);int d=dx*dx+dy*dy;
                    // Sample existing darker brown wood. Do not introduce a
                    // fixed gray border or enlarge/erode the alpha silhouette.
                    if(p.A==255&&p.R>=p.G&&p.G>=p.B&&p.G<115&&p.R-p.B>=12&&d<distance){best=p;distance=d;}
                }
                if(best.ToArgb()!=c.ToArgb()){b.SetPixel(x,y,best);count++;}
            }
            b.Save(output,ImageFormat.Png);return count;
        }
    }
}
'@
$station=[WastesExactRepair]::Station((Join-Path $baseline 'Station.png'),$master,(Join-Path $output 'Station.png'))
$foreground=[WastesExactRepair]::Foreground((Join-Path $baseline 'Foreground-Deep.png'),(Join-Path $output 'Foreground-Deep.png'))
[ordered]@{stationChangedPixels=$station;foregroundChangedPixels=$foreground;installed=$false;rescaled=$false;foregroundAlphaChanged=$false;scope='Candidate-specific source-aware repair, not a general-purpose image key or live rendering proof'} | ConvertTo-Json
