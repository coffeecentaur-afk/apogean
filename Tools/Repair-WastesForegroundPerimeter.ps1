param([switch]$SelfTest)
# User-approved exact-pixel color correction. Candidate output only.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$source=Join-Path $repoRoot 'Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1/Foreground-Deep.png'
$station=Join-Path $repoRoot 'Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1/Station.png'
$output=Join-Path $repoRoot 'Art/Candidates/WastesForegroundPerimeter-v2'
$sourceHash='4158255600690F0BFA67931C8D3AE0B5FF5E93B3BE5714D360DCB279DD3CC8D4'
$stationHash='C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C'
if((Get-FileHash -LiteralPath $source).Hash -ne $sourceHash){throw 'FOREGROUND_SOURCE_CHANGED'}
if((Get-FileHash -LiteralPath $station).Hash -ne $stationHash){throw 'ACCEPTED_STATION_CHANGED'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Linq','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
public static class WastesPerimeterRepair {
    static bool Inside(Bitmap b,int x,int y)=>x>=0&&y>=0&&x<b.Width&&y<b.Height;
    static bool Edge(Bitmap b,int x,int y) {
        if(b.GetPixel(x,y).A!=255)return false;
        for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            if(Inside(b,x+dx,y+dy)&&b.GetPixel(x+dx,y+dy).A==0)return true;
        return false;
    }
    static bool Body(Bitmap b,int x,int y) {
        for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
            if(!Inside(b,x+dx,y+dy)||b.GetPixel(x+dx,y+dy).A!=255)return false;
        return true;
    }
    static double Luma(Color c)=>.2126*c.R+.7152*c.G+.0722*c.B;
    static bool Pale(Color c) {
        int min=Math.Min(c.R,Math.Min(c.G,c.B)), max=Math.Max(c.R,Math.Max(c.G,c.B));
        return c.A==255 && ((min>=85&&max-min<=65)||(min>=145&&max-min<=100));
    }
    public static List<int[]> Repair(Bitmap a,Bitmap b) {
        var changes=new List<int[]>();
        for(int y=0;y<a.Height;y++)for(int x=0;x<a.Width;x++) {
            var c=a.GetPixel(x,y);
            if(!Pale(c)||!Edge(a,x,y))continue;
            var donors=new List<Point>();
            for(int dy=-7;dy<=7;dy++)for(int dx=-7;dx<=7;dx++) {
                int d=dx*dx+dy*dy;
                if(d==0||d>49||!Inside(a,x+dx,y+dy)||!Body(a,x+dx,y+dy))continue;
                donors.Add(new Point(x+dx,y+dy));
            }
            // Use the median of the five closest fully supported source colors.
            // This samples the adjacent material rather than imposing one gray,
            // brown or black outline. No repeated/propagating in-place sampling.
            var nearest=donors.OrderBy(p=>(p.X-x)*(p.X-x)+(p.Y-y)*(p.Y-y)).Take(5)
                .OrderBy(p=>Luma(a.GetPixel(p.X,p.Y))).ToArray();
            if(nearest.Length==0)continue;
            var donor=nearest[nearest.Length/2]; var dcolor=a.GetPixel(donor.X,donor.Y);
            if(Luma(c)-Luma(dcolor)<16)continue;
            b.SetPixel(x,y,dcolor);
            changes.Add(new[]{x,y,donor.X,donor.Y});
        }
        return changes;
    }
    public static void Validate(Bitmap a,Bitmap b,List<int[]> changes) {
        if(a.Width!=b.Width||a.Height!=b.Height)throw new Exception("DIMENSIONS_CHANGED");
        var map=changes.ToDictionary(p=>p[1]*a.Width+p[0]);
        int actual=0;
        for(int y=0;y<a.Height;y++)for(int x=0;x<a.Width;x++) {
            var old=a.GetPixel(x,y);var c=b.GetPixel(x,y);
            if(old.A!=c.A)throw new Exception("ALPHA_CHANGED");
            if(old.ToArgb()==c.ToArgb())continue;
            actual++;
            if(!Edge(a,x,y))throw new Exception("INTERIOR_CHANGED");
            if(!map.TryGetValue(y*a.Width+x,out var p))throw new Exception("UNRECORDED_CHANGE");
            if(!Inside(a,p[2],p[3])||(p[2]-x)*(p[2]-x)+(p[3]-y)*(p[3]-y)>49||!Body(a,p[2],p[3]))
                throw new Exception("INVALID_MATERIAL_DONOR");
            if(c.ToArgb()!=a.GetPixel(p[2],p[3]).ToArgb()||Luma(old)-Luma(c)<16)
                throw new Exception("COLOR_NOT_LOCAL_SOURCE");
        }
        if(actual!=changes.Count)throw new Exception("MANIFEST_COUNT");
    }
    public static void Run(string input,string folder) {
        using(var a=new Bitmap(input))using(var b=new Bitmap(a)) {
            var changes=Repair(a,b);Validate(a,b,changes);
            if(changes.Count==0)throw new Exception("NO_CORRECTION");
            b.Save(Path.Combine(folder,"Foreground-Deep.png"),ImageFormat.Png);
            File.WriteAllText(Path.Combine(folder,"pixel-changes.json"),JsonSerializer.Serialize(changes));
            File.WriteAllText(Path.Combine(folder,"repair-measurements.json"),JsonSerializer.Serialize(new {
                width=b.Width,height=b.Height,changedPixels=changes.Count,
                changedBelowOldProbe=changes.Count(p=>p[1]>=440),alphaChanges=0,interiorChanges=0,
                donorRadius=7,stationChanged=false,installed=false,liveApproved=false,
                selection="Pale exposed pixels at least16 luma brighter than the median of5 nearest opaque-body colors. Asset-specific candidate rule, not a universal cleanup."},
                new JsonSerializerOptions{WriteIndented=true}));
            Preview(a,b,Path.Combine(folder,"Edges-Dark-Before-After.png"),Color.FromArgb(44,45,51));
            Preview(a,b,Path.Combine(folder,"Edges-Warm-Before-After.png"),Color.FromArgb(70,57,43));
            Preview(a,b,Path.Combine(folder,"Edges-Light-Before-After.png"),Color.FromArgb(178,186,192));
        }
    }
    static void Preview(Bitmap before,Bitmap after,string path,Color backing) {
        var crops=new[]{new Rectangle(35,525,180,180),new Rectangle(1268,430,180,180),new Rectangle(160,220,180,180)};
        using(var sheet=new Bitmap(752,1216,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(sheet))
        using(var font=new Font("Consolas",12))using(var label=new SolidBrush(Color.FromArgb(230,228,222))) {
            g.Clear(Color.FromArgb(24,24,27));
            g.DrawString("BEFORE                    AFTER | 2x pixel inspection",font,label,12,10);
            for(int row=0;row<crops.Length;row++) {
                int y=48+row*388;
                g.DrawString($"Source crop {crops[row].X},{crops[row].Y} (not a game screenshot)",font,label,12,y);
                for(int col=0;col<2;col++) {
                    var src=col==0?before:after;
                    for(int py=0;py<180;py++)for(int px=0;px<180;px++) {
                        var c=src.GetPixel(crops[row].X+px,crops[row].Y+py);
                        if(c.A==0)c=backing;
                        int tx=12+col*376+px*2,ty=y+24+py*2;
                        sheet.SetPixel(tx,ty,c);sheet.SetPixel(tx+1,ty,c);
                        sheet.SetPixel(tx,ty+1,c);sheet.SetPixel(tx+1,ty+1,c);
                    }
                }
            }
            sheet.Save(path,ImageFormat.Png);
        }
    }
    public static void SelfTest() {
        using(var a=new Bitmap(20,500,PixelFormat.Format32bppArgb)) {
            for(int y=5;y<495;y++)for(int x=5;x<16;x++)a.SetPixel(x,y,Color.FromArgb(255,70,55,35));
            a.SetPixel(5,450,Color.FromArgb(255,180,171,159));
            a.SetPixel(10,100,Color.FromArgb(255,215,202,171)); // protected interior highlight
            using(var b=new Bitmap(a)) {
                var changes=Repair(a,b);Validate(a,b,changes);
                if(changes.Count!=1||changes[0][1]!=450)throw new Exception("LOWER_EDGE_REPAIR_MISSED");
                foreach(string mutation in new[]{"alpha","interior","unrecorded"})using(var bad=new Bitmap(b)) {
                    if(mutation=="alpha")bad.SetPixel(5,450,Color.FromArgb(0,0,0,0));
                    if(mutation=="interior")bad.SetPixel(10,100,Color.FromArgb(255,20,20,20));
                    if(mutation=="unrecorded")bad.SetPixel(5,200,Color.FromArgb(255,20,20,20));
                    bool failed=false;try{Validate(a,bad,changes);}catch(Exception e){
                        string expected=mutation=="alpha"?"ALPHA_CHANGED":mutation=="interior"?"INTERIOR_CHANGED":"UNRECORDED_CHANGE";
                        if(e.Message!=expected)throw;failed=true;}
                    if(!failed)throw new Exception("MUTATION_PASSED:"+mutation);
                }
            }
        }
    }
}
'@
if($SelfTest){[WastesPerimeterRepair]::SelfTest();Write-Output 'PASS lower-edge repair + protected interior highlight; alpha/interior/unrecorded mutations rejected.';exit 0}
New-Item -ItemType Directory -Path $output -Force | Out-Null
[WastesPerimeterRepair]::Run($source,$output)
& (Get-Process -Id $PID).Path -NoProfile -File (Join-Path $PSScriptRoot 'Test-BackgroundReplacement.ps1') -ReferencePath $source -CandidatePath (Join-Path $output 'Foreground-Deep.png') -AllowedRectangles '0,0,1448,1915' -PreserveAlphaMask -ReportPath (Join-Path $output 'Foreground-report.json')
if($LASTEXITCODE -ne 0){throw 'EXPORT_INVARIANT_FAILURE'}
Get-Content -LiteralPath (Join-Path $output 'repair-measurements.json')
