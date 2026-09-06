param()
# Candidate-only matte extraction/composition, not a tModLoader or production test.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$repoRoot = Split-Path -Parent $PSScriptRoot
$candidate = Join-Path $repoRoot 'Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1'
$references = @([System.Drawing.Bitmap].Assembly.Location, [System.Drawing.Color].Assembly.Location,
    'System.Runtime', 'System.Collections', 'System.Linq', 'System.Text.Json', 'System.Runtime.InteropServices')
$references += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
$projection = Get-Content -Raw (Join-Path $repoRoot 'Common/Backgrounds/WastesCameraProjection.cs')
Add-Type -TypeDefinition $projection
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;

public static class ModularMidPreview
{
    public const int Overlap = 256;
    // The generated continuation joins the valleys farther down. Reject that
    // portion; only these fresh rows remain in the candidate, never repeat them.
    public const int LowerRows = 640;
    static bool IsMatte(Color c) => c.R-c.G > 24 && c.B-c.G > 24;
    static Bitmap Key(Bitmap source)
    {
        var result = new Bitmap(source.Width,source.Height,PixelFormat.Format32bppArgb);
        for(int y=0;y<source.Height;y++) for(int x=0;x<source.Width;x++)
        {
            Color c=source.GetPixel(x,y);
            result.SetPixel(x,y,IsMatte(c)?Color.Transparent:Color.FromArgb(255,c.R,c.G,c.B));
        }
        return result;
    }
    public static string ExportForeground(string root)
    {
        using(var source=new Bitmap(Path.Combine(root,"Foreground-Cleanup-Source.png")))
        using(var output=new Bitmap(source.Width,source.Height,PixelFormat.Format32bppArgb))
        {
            if(source.Width!=1448||source.Height!=1086)throw new InvalidDataException("Changed foreground dimensions; refuse rescaling.");
            long transparent=0;
            for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++)
            {
                Color c=source.GetPixel(x,y);
                int min=Math.Min(c.R,Math.Min(c.G,c.B)),max=Math.Max(c.R,Math.Max(c.G,c.B));
                // This specific cleanup draft baked its neutral near-white grid.
                // The bank has no white material. Explicit candidate-only matte
                // removal, never apply this predicate to arbitrary game assets.
                bool empty=c.A==0||(min>185&&max-min<24);
                if(empty)transparent++;
                output.SetPixel(x,y,empty?Color.Transparent:Color.FromArgb(255,c.R,c.G,c.B));
            }
            // Remove the single light, low-chroma antialias fringe left by this
            // source's baked white matte. Only exposed boundary pixels qualify;
            // internal material pixels and ochre turf are not recolored.
            var fringe=new List<Point>();
            for(int y=0;y<output.Height;y++)for(int x=0;x<output.Width;x++)
            {
                Color c=output.GetPixel(x,y);
                int min=Math.Min(c.R,Math.Min(c.G,c.B)),max=Math.Max(c.R,Math.Max(c.G,c.B));
                if(c.A==0||min<=145||max-min>=56)continue;
                bool edge=false;
                for(int dy=-1;dy<=1&&!edge;dy++)for(int dx=-1;dx<=1;dx++)
                    if(x+dx>=0&&x+dx<output.Width&&y+dy>=0&&y+dy<output.Height&&output.GetPixel(x+dx,y+dy).A==0){edge=true;break;}
                if(edge)fringe.Add(new Point(x,y));
            }
            foreach(var p in fringe)output.SetPixel(p.X,p.Y,Color.Transparent);
            transparent+=fringe.Count;
            output.Save(Path.Combine(root,"Derived","Foreground-Bank.png"),ImageFormat.Png);
            string report=JsonSerializer.Serialize(new {width=output.Width,height=output.Height,transparent,
                opaque=output.Width*output.Height-transparent,partialAlpha=0,fringeRemoved=fringe.Count,
                nominalSoilRow=330,rawRgbaBytes=output.Width*output.Height*4,
                lowerContinuationRequired=true,installed=false,artApproved=false},new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText(Path.Combine(root,"Derived","foreground-measurements.json"),report);
            return report;
        }
    }
    static double Cost(Color a,Color b)
    {
        if(a.A!=b.A)return 1000000;
        if(a.A==0)return 0;
        return (a.R-b.R)*(a.R-b.R)+(a.G-b.G)*(a.G-b.G)+(a.B-b.B)*(a.B-b.B);
    }
    static int[] Seam(Bitmap upper,Bitmap lower)
    {
        var parents=new int[upper.Width,Overlap];
        var previous=new double[Overlap];
        for(int x=0;x<upper.Width;x++)
        {
            var next=new double[Overlap];
            for(int y=0;y<Overlap;y++)
            {
                int best=y;
                for(int p=Math.Max(0,y-1);p<=Math.Min(Overlap-1,y+1);p++)
                    if(previous[p]<previous[best])best=p;
                parents[x,y]=best;
                next[y]=previous[best]+Cost(upper.GetPixel(x,upper.Height-Overlap+y),lower.GetPixel(x,y))+Math.Abs(y-Overlap/2)*.01;
            }
            previous=next;
        }
        int end=0;
        for(int y=1;y<Overlap;y++)if(previous[y]<previous[end])end=y;
        var path=new int[upper.Width];
        for(int x=upper.Width-1;x>=0;x--){path[x]=end;end=parents[x,end];}
        return path;
    }
    static Rectangle[] Regions(Bitmap image)
    {
        var result=new List<Rectangle>();
        int start=-1;
        for(int x=0;x<=image.Width;x++)
        {
            bool occupied=false;
            if(x<image.Width)for(int y=0;y<image.Height;y++)
                if(image.GetPixel(x,y).A!=0){occupied=true;break;}
            if(occupied&&start<0)start=x;
            if(!occupied&&start>=0){result.Add(new Rectangle(start,0,x-start,image.Height));start=-1;}
        }
        return result.ToArray();
    }
    public static string Export(string root)
    {
        string output=Path.Combine(root,"Derived");Directory.CreateDirectory(output);
        using(var sourceA=new Bitmap(Path.Combine(root,"Upper-Cliffs-Matte.png")))
        using(var sourceB=new Bitmap(Path.Combine(root,"Lower-Cliffs-Matte.png")))
        {
            if(sourceA.Width!=1536||sourceA.Height!=1024||sourceB.Size!=sourceA.Size)
                throw new InvalidDataException("Source dimensions changed; refuse rescaling.");
            using(var a=Key(sourceA)) using(var b=Key(sourceB))
            using(var joined=new Bitmap(a.Width,a.Height-Overlap+LowerRows,PixelFormat.Format32bppArgb))
            {
                int[] seam=Seam(a,b);
                for(int x=0;x<joined.Width;x++) for(int y=0;y<joined.Height;y++)
                    joined.SetPixel(x,y,y<a.Height-Overlap+seam[x]?a.GetPixel(x,y):b.GetPixel(x,y-(a.Height-Overlap)));
                var regions=Regions(joined);
                if(regions.Length!=3||regions.Any(r=>r.Width<100))
                    throw new InvalidDataException("Expected three separated cliff groups; source matte or valleys failed.");
                long transparent=0,opaque=0;
                for(int y=0;y<joined.Height;y++)for(int x=0;x<joined.Width;x++)
                {
                    Color c=joined.GetPixel(x,y);
                    if(c.A==0)transparent++;else if(c.A==255&&!IsMatte(c))opaque++;
                    else throw new InvalidDataException("Invalid alpha or remaining matte.");
                }
                joined.Save(Path.Combine(output,"Joined.png"),ImageFormat.Png);
                string[] ids={"Highway","Quiet","Station"};
                var records=new List<object>();
                int layoutX=0;
                for(int i=0;i<3;i++)
                {
                    using(var module=joined.Clone(regions[i],PixelFormat.Format32bppArgb))
                        module.Save(Path.Combine(output,ids[i]+".png"),ImageFormat.Png);
                    records.Add(new {id=ids[i],x=regions[i].X,width=regions[i].Width,height=joined.Height,layoutX});
                    // Open valley + quiet hill + open valley separate major ruins.
                    layoutX+=regions[i].Width+(i==2?480:360);
                }
                // Exact pieces reconstruct their own group without a pixel shift.
                int socketY=a.Height-Overlap;
                using(var top=joined.Clone(new Rectangle(0,0,joined.Width,socketY),PixelFormat.Format32bppArgb))
                    top.Save(Path.Combine(output,"Joined-Top.png"),ImageFormat.Png);
                using(var bottom=joined.Clone(new Rectangle(0,socketY,joined.Width,joined.Height-socketY),PixelFormat.Format32bppArgb))
                    bottom.Save(Path.Combine(output,"Joined-Continuation.png"),ImageFormat.Png);
                string json=JsonSerializer.Serialize(new {sourceWidth=a.Width,sourceHeight=a.Height,exportHeight=joined.Height,
                    overlap=Overlap,lowerRows=LowerRows,socketY,seamMin=seam.Min(),seamMax=seam.Max(),
                    transparent,opaque,partialAlpha=0,layoutPeriod=layoutX,groups=records,
                    rescaled=false,installed=false,artApproved=false},new JsonSerializerOptions{WriteIndented=true});
                File.WriteAllText(Path.Combine(output,"measurements.json"),json);
                return json;
            }
        }
    }
    public static void Render(string root,string farPath,string closePath,string name,int width,int height,
        float farTop,float midTop,float closeTop,float midOpacity,bool nearest,float pan,bool modularForeground)
    {
        string output=Path.Combine(root,"Derived");
        using(var far=new Bitmap(farPath)) using(var close=new Bitmap(modularForeground?Path.Combine(output,"Foreground-Bank.png"):closePath))
        using(var highway=new Bitmap(Path.Combine(output,"Highway.png")))
        using(var quiet=new Bitmap(Path.Combine(output,"Quiet.png")))
        using(var station=new Bitmap(Path.Combine(output,"Station.png")))
        using(var canvas=new Bitmap(width,height,PixelFormat.Format32bppArgb))
        using(var g=Graphics.FromImage(canvas)) using(var font=new Font("Consolas",14))
        {
            g.Clear(Color.FromArgb(166,204,221));
            g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
            g.CompositingMode=CompositingMode.SourceOver;
            for(int x=-200;x<width;x+=far.Width)g.DrawImageUnscaled(far,x,(int)Math.Floor(farTop));
            // This is only the unchanged Far reference's existing coverage guard.
            // It never fills transparent Mid valleys with Mid art.
            for(int y=(int)Math.Floor(farTop)+far.Height;y<height;y+=512)
                for(int x=-200;x<width;x+=far.Width)
                    g.DrawImage(far,new Rectangle(x,y,far.Width,512),new Rectangle(0,far.Height-512,far.Width,512),GraphicsUnit.Pixel);
            var modules=new[]{highway,quiet,station};
            int period=modules.Sum(m=>m.Width)+1200;
            using(var attributes=new ImageAttributes())
            {
                var matrix=new ColorMatrix();matrix.Matrix33=midOpacity;attributes.SetColorMatrix(matrix);
                int phase=(int)(pan%period);if(phase<0)phase+=period;
                for(int start=-phase;start<width;start+=period)
                {
                    int x=start;
                    foreach(var module in modules)
                    {
                        g.DrawImage(module,new Rectangle(x,(int)Math.Floor(midTop),module.Width,module.Height),0,0,module.Width,module.Height,GraphicsUnit.Pixel,attributes);
                        x+=module.Width+360;
                    }
                }
            }
            if(nearest&&modularForeground)
            {
                // Long bank, offset from the Mid groups. Different horizontal
                // rates naturally permit some overlap. Never chase a landmark.
                int closePeriod=close.Width+820;
                int phase=(int)(pan*.30/.14-620)%closePeriod;if(phase<0)phase+=closePeriod;
                int top=(int)Math.Floor(closeTop+488-330);
                if(top<height&&top+close.Height<height)
                    throw new InvalidDataException("Foreground bottom exposed: author a lower extension, not a fill.");
                for(int x=-phase;x<width;x+=closePeriod)g.DrawImageUnscaled(close,x,top);
            }
            if(nearest&&!modularForeground)for(int x=-110;x<width;x+=close.Width)
            {
                int baseY=(int)Math.Floor(closeTop)+close.Height;
                g.DrawImageUnscaled(close,x,(int)Math.Floor(closeTop));
                // Match the CURRENT QA renderer's explicitly provisional Close
                // guard. Omitting it would invent a new cutoff in this preview.
                for(int index=Math.Max(0,(int)Math.Floor(-baseY/512.0));baseY+index*512<height;index++)
                    using(var strip=close.Clone(new Rectangle(0,close.Height-512,close.Width,512),PixelFormat.Format32bppArgb))
                    {
                        if(index%2==0)strip.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        g.DrawImageUnscaled(strip,x,baseY+index*512);
                    }
            }
            g.FillRectangle(Brushes.Black,0,0,width,58);
            g.DrawString("OFFLINE ART CANDIDATE | "+name+" | "+width+"x"+height+" | source pixels 1:1",font,Brushes.White,12,6);
            g.DrawString(modularForeground?"NOT in-game. New long Close + modular Mid; old Far reference. Approval/depth checks pending.":
                "NOT an in-game capture. Existing Far/Close reference; new Mid only. No production approval.",font,Brushes.LightGray,12,31);
            canvas.Save(Path.Combine(output,name+"-"+height+".png"),ImageFormat.Png);
        }
    }
}
'@
$json = [ModularMidPreview]::Export($candidate)
Write-Output $json
Write-Output ([ModularMidPreview]::ExportForeground($candidate))
$far = Join-Path $repoRoot 'Content/Backgrounds/Candidates/WastesV1/Far.png'
$close = Join-Path $repoRoot 'Content/Backgrounds/Candidates/WastesV1/Close.png'
# Reuse the current fixed QA datum and production projection arithmetic. New
# artwork requires a 220px alignment offset; this remains proposed, not installed.
$surface = 649.0
$ground = ($surface - 50) * 16
$span = $ground - $surface * 16 * .35
foreach($viewport in @(@{Width=1920;Height=1080;Zoom=1.0},@{Width=2560;Height=1440;Zoom=4.0/3.0})) {
    foreach($sample in @(@{Name='Ground';Lift=0;Fade=$true},@{Name='Elevated';Lift=$span/3;Fade=$true},
        @{Name='Shallow';Lift=-400;Fade=$true},@{Name='High-NoFade-Stress';Lift=$span*.8;Fade=$false})) {
        $cameraY=$ground-$viewport.Height*.5-$sample.Lift
        $farTop=[apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$cameraY,$viewport.Height,1280,0,$viewport.Zoom)
        $midTop=220+[apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$cameraY,$viewport.Height,1408,1,$viewport.Zoom)
        $closeTop=[apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$cameraY,$viewport.Height,1280,2,$viewport.Zoom)
        $altitude=[apogean.Common.Backgrounds.WastesCameraProjection]::Altitude($surface,$cameraY+$viewport.Height*.5)
        $opacity=if($sample.Fade){[apogean.Common.Backgrounds.WastesCameraProjection]::MiddleOpacity($altitude)}else{1.0}
        if($midTop+1408 -lt $viewport.Height){throw 'Candidate underside exposed; author more depth instead of hiding it.'}
        [ModularMidPreview]::Render($candidate,$far,$close,$sample.Name,$viewport.Width,$viewport.Height,$farTop,$midTop,$closeTop,$opacity,$true,80,$false)
        if($viewport.Height -eq 1080 -and $sample.Name -in 'Ground','Elevated') {
            [ModularMidPreview]::Render($candidate,$far,$close,('Layered-'+$sample.Name),$viewport.Width,$viewport.Height,$farTop,$midTop,$closeTop,$opacity,$true,80,$true)
        }
    }
}
Write-Host 'Candidate exported with original material pixels, authored Mid continuation, and ten offline viewport studies. No game assets changed.'
