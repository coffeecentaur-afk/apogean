param([Parameter(Mandatory)][string]$OutputDirectory,
    [int[]]$RemoveComponents=@(), [switch]$Finalize,
    [ValidateSet('v1','v2')][string]$Version='v1', [switch]$GridReviewOnly)
# Source-specific proposal -> reviewed mask -> existing exact exporter -> study.
# Not a runtime exporter. Never writes Content or replaces accepted art.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$spec=if($Version -eq 'v1'){
    @{hash='6CBDBE740DBD73ACF85D50672F3ED45C63887AD010A505BEB90FF73861BCB034';width=1317;height=1194;top=38}
}else{
    @{hash='C47B826925B5AF405CF51BDEFA315991F10D07B87ECBE6234086ECCA0E10C10B';width=1316;height=1195;top=58}
}
$source=Join-Path $root "Art/Candidates/WastesMidRuins-v2/PixelStyle-$Version/MotorDepot-original.png"
$station=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3/Station-Upper.png'
$old=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3/MotorDepot-Upper.png'
$oldHash='B40D71FDE94AA329A77144E9114FEB4A57A9E83DA0365DBB9992DE97A365D293'
$oldLabel='Previous depot (too fine-grained)'
if($Version -eq 'v2'){
    $old=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/Selected/MotorDepot-Upper.png'
    $oldHash='CA51A9C972F61DCD5E598AB523FD00A50416D5B672A7064CD786A3777D66045B'
    $oldLabel='Previous depot (too clean)'
}
foreach($pin in @(@($source,$spec.hash),
    @($station,'83C8EDE6580E6CEB2CEECD2C0F24078EFCADE3EB10680C8619BC3B1D9207C59B'),
    @($old,$oldHash))){
    if((Get-FileHash -LiteralPath $pin[0]).Hash -ne $pin[1]){throw 'SOURCE_CHANGED'}
}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidStyleStudy {
    public static void Propose(string path,string folder,int[] selected,bool finalize,string hash,int width,int height){
        using(var src=new Bitmap(path)){
            int w=src.Width,h=src.Height;
            if(w!=width||h!=height)throw new InvalidDataException("SOURCE_DIMENSIONS");
            var labels=new int[w*h];var bright=new bool[w*h];int next=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                Color c=src.GetPixel(x,y);
                if(c.A!=255)throw new InvalidDataException("UNEXPECTED_SOURCE_ALPHA");
                int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                bright[y*w+x]=lo>=225&&hi-lo<=12;
            }
            var regions=new List<object>();
            for(int p=0;p<labels.Length;p++){
                if(!bright[p]||labels[p]!=0)continue;
                int id=++next,area=0,minX=w,minY=h,maxX=0,maxY=0;bool border=false;
                var q=new Queue<int>();q.Enqueue(p);labels[p]=id;
                while(q.Count>0){int at=q.Dequeue(),x=at%w,y=at/w;area++;
                    minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);
                    border|=x==0||x==w-1||y==0||y==h-1;
                    foreach(int n in new[]{x>0?at-1:-1,x<w-1?at+1:-1,y>0?at-w:-1,y<h-1?at+w:-1})
                        if(n>=0&&bright[n]&&labels[n]==0){labels[n]=id;q.Enqueue(n);}
                }
                regions.Add(new{id,area,minX,minY,maxX,maxY,border});
            }
            var remove=new HashSet<int>(selected);
            if(finalize){
                if(remove.Count==0||remove.Count!=selected.Length)throw new InvalidDataException("INVALID_SELECTION");
                foreach(int id in selected)if(id<1||id>next)throw new InvalidDataException("INVALID_SELECTION");
            }
            var changes=new List<int[]>();
            using(var prepared=new Bitmap(src)){
                if(finalize)for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                    if(remove.Contains(labels[y*w+x]))continue;
                    Color c=src.GetPixel(x,y);
                    int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                    if(lo<85||hi-lo>35)continue;
                    bool edge=false;
                    for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){
                        int nx=x+dx,ny=y+dy;
                        if(nx>=0&&ny>=0&&nx<w&&ny<h&&remove.Contains(labels[ny*w+nx]))edge=true;
                    }
                    if(!edge)continue;
                    int best=-1,bestDistance=1000;
                    for(int dy=-6;dy<=6;dy++)for(int dx=-6;dx<=6;dx++){
                        int nx=x+dx,ny=y+dy,dist=dx*dx+dy*dy;
                        if(dist==0||dist>36||dist>=bestDistance||nx<1||ny<1||nx>=w-1||ny>=h-1)continue;
                        if(remove.Contains(labels[ny*w+nx]))continue;
                        Color d=src.GetPixel(nx,ny);
                        int dlo=Math.Min(d.R,Math.Min(d.G,d.B)),dhi=Math.Max(d.R,Math.Max(d.G,d.B));
                        if(dlo>=85&&dhi-dlo<=35)continue;
                        if((299*c.R+587*c.G+114*c.B)-(299*d.R+587*d.G+114*d.B)<25000)continue;
                        int support=0;
                        for(int ay=-1;ay<=1;ay++)for(int ax=-1;ax<=1;ax++)
                            if((ax!=0||ay!=0)&&!remove.Contains(labels[(ny+ay)*w+nx+ax]))support++;
                        if(support<2)continue;
                        best=ny*w+nx;bestDistance=dist;
                    }
                    if(best<0)continue;
                    prepared.SetPixel(x,y,src.GetPixel(best%w,best/w));
                    changes.Add(new[]{x,y,best%w,best/w});
                }
            Directory.CreateDirectory(folder);
            if(finalize){
                prepared.Save(Path.Combine(folder,"MotorDepot-EdgeSource.png"),ImageFormat.Png);
                File.WriteAllText(Path.Combine(folder,"edge-changes.json"),JsonSerializer.Serialize(changes));
            }
            using(var mask=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var view=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                    bool empty=finalize?remove.Contains(labels[y*w+x]):bright[y*w+x];
                    mask.SetPixel(x,y,empty?Color.Black:Color.White);
                    view.SetPixel(x,y,empty?Color.FromArgb(255,40,37,42):prepared.GetPixel(x,y));
                }
                mask.Save(Path.Combine(folder,"MotorDepot-Mask.png"),ImageFormat.Png);
                view.Save(Path.Combine(folder,"Mask-Preview.png"),ImageFormat.Png);
            }
            }
            File.WriteAllText(Path.Combine(folder,"mask-record.json"),JsonSerializer.Serialize(new{
                sourceSHA256=hash,
                width=w,height=h,status=finalize?"agent-reviewed component selection":"UNREVIEWED proposal",
                criterion="this source only: min RGB >=225 and chroma <=12; no erosion; separate recorded RGB edge donors",selected,edgeColorChanges=changes.Count,regions
            },new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    public static void Fit(string source,string station,string old,string output,int top,string oldLabel,int step){
        // Measured generated landmarks: ground y~904, vent y~488, facade x185..1160.
        // Uniform 1/3 maps them to y339, y201, x94..418 at previous game size.
        // Generation did NOT preserve requested size or exact geometry: this is
        // an explicitly registered ART REVIEW, never a lossless repair claim.
        using(var src=new Bitmap(source))using(var result=new Bitmap(512,432,PixelFormat.Format32bppArgb)){
            for(int y=0;y<432;y++)for(int x=0;x<512;x++){
                int sx=3*((x/step)*step-32)+(3*step)/2,sy=3*((y/step)*step-top)+(3*step)/2;
                if(sx<0||sy<0||sx>=src.Width||sy>=src.Height)continue;
                Color c=src.GetPixel(sx,sy);result.SetPixel(x,y,c.A==0?Color.FromArgb(0,0,0,0):c);
            }
            result.Save(Path.Combine(output,"MotorDepot-Upper.png"),ImageFormat.Png);
        }
        foreach(bool dark in new[]{true,false})using(var b=new Bitmap(1536,530,PixelFormat.Format32bppArgb))
        using(var g=Graphics.FromImage(b))using(var font=new Font("Consolas",12))
        using(var ink=new SolidBrush(dark?Color.Gainsboro:Color.FromArgb(30,30,35))){
            g.Clear(dark?Color.FromArgb(40,37,42):Color.FromArgb(151,185,201));
            g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString("OFFLINE STYLE COMPARISON - 1:1 preview pixels, not an in-game capture",font,ink,16,8);
            string[] paths={station,old,Path.Combine(output,"MotorDepot-Upper.png")};
            string[] names={"Accepted Station (unchanged)",oldLabel,step==1?"Revised depot - review candidate":"Same gritty redraw - 2px grid test"};
            for(int i=0;i<3;i++)using(var art=new Bitmap(paths[i])){
                g.DrawString(names[i],font,ink,i*512+16,34);
                g.DrawImage(art,new Rectangle(i*512,62,512,432),new Rectangle(0,0,512,432),GraphicsUnit.Pixel);
            }
            g.DrawString(step==1?"Upper-only crops. Depot redraw fitted at 1/3, offset (32,"+top+"); approximate soil datum y340. Deep foundations NOT complete.":"OFFLINE resampling test: 6-source-pixel sampling -> 2x2 display blocks. NOT additional detail or exact silhouette preservation.",font,ink,16,503);
            b.Save(Path.Combine(output,dark?"Comparison-Dark.png":"Comparison-Light.png"),ImageFormat.Png);
        }
    }
}
'@
if($GridReviewOnly){
    if($Version -ne 'v2'){throw 'GRID_REVIEW_V2_ONLY'}
    $rgba=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/PixelStyle-v2/Selected/MotorDepot-Transparent.png'
    if((Get-FileHash -LiteralPath $rgba).Hash -ne 'F682C085F139FBAA475A9CF0BD70AF8955A315958B2E7E536AE27A94E43E391D'){throw 'RGBA_CHANGED'}
    [IO.Directory]::CreateDirectory($output) | Out-Null
    [MidStyleStudy]::Fit($rgba,$station,$old,$output,$spec.top,$oldLabel,2)
    Write-Output "Offline resampling experiment, not new pixel-authored detail: $output"
    exit 0
}
[MidStyleStudy]::Propose($source,$output,$RemoveComponents,[bool]$Finalize,$spec.hash,$spec.width,$spec.height)
if($Finalize){
    & (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $output 'MotorDepot-EdgeSource.png') -MaskPath (Join-Path $output 'MotorDepot-Mask.png') -OutputPath (Join-Path $output 'MotorDepot-Transparent.png')
    [MidStyleStudy]::Fit((Join-Path $output 'MotorDepot-Transparent.png'),$station,$old,$output,$spec.top,$oldLabel,1)
}
Write-Output "Offline study only: $output"
