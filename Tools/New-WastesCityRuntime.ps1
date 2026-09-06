param([Parameter(Mandatory)][string]$OutputDirectory)
# Bounded native-pixel fitting: minimum-error joins, no scale or mirrored art.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$inputPath=Join-Path $root 'Art/Candidates/WastesFarCity-v1/Transparent-v1/City-Transparent.png'
$expected='71C14F299210D278E1EB61B31EEBC9E341F2EF986E0C4D4159D0B3C2A41B785B'
if((Get-FileHash -LiteralPath $inputPath).Hash -ne $expected){throw 'CITY_SOURCE_CHANGED'}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_CANDIDATE_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.IO','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
public static class NativeCityFit {
    static int Cost(int a,int b) {
        return Math.Abs((a>>24&255)-(b>>24&255))*4+Math.Abs((a>>16&255)-(b>>16&255))
            +Math.Abs((a>>8&255)-(b>>8&255))+Math.Abs((a&255)-(b&255));
    }
    // A connected minimum-error cut. Fixed endpoints keep repeated strips aligned.
    static int[] Cut(int[,] costs) {
        int steps=costs.GetLength(0),span=costs.GetLength(1),mid=span/2;
        long[,] distance=new long[steps,span];int[,] parent=new int[steps,span];
        for(int p=0;p<span;p++)distance[0,p]=p==mid?costs[0,p]:long.MaxValue/4;
        for(int step=1;step<steps;step++)for(int p=1;p<span-1;p++){
            int best=p;
            if(p>1&&distance[step-1,p-1]<distance[step-1,best])best=p-1;
            if(p<span-2&&distance[step-1,p+1]<distance[step-1,best])best=p+1;
            distance[step,p]=distance[step-1,best]+costs[step,p];parent[step,p]=best;
        }
        int[] cut=new int[steps];cut[steps-1]=mid;
        for(int s=steps-1;s>0;s--)cut[s-1]=parent[s,cut[s]];
        return cut;
    }
    public static void Run(string input,string output) {
        using(var source=new Bitmap(input)){
            const int originalWidth=1586,originalHeight=992,overlap=128,w=1458,h=1792;
            if(source.Width!=originalWidth||source.Height!=originalHeight)throw new InvalidDataException("SOURCE_DIMENSIONS");
            int[,] art=new int[originalHeight,originalWidth];
            for(int y=0;y<originalHeight;y++)for(int x=0;x<originalWidth;x++)art[y,x]=source.GetPixel(x,y).ToArgb();
            int[,] costs=new int[originalHeight,overlap];
            for(int y=0;y<originalHeight;y++)for(int x=0;x<overlap;x++)costs[y,x]=Cost(art[y,x],art[y,x+w]);
            int[] seam=Cut(costs);
            int[,] canvas=new int[h,w],loop=new int[originalHeight,w];
            for(int y=0;y<originalHeight;y++)for(int x=0;x<w;x++){
                int c=x<seam[y]?art[y,x+w]:art[y,x];
                canvas[y,x]=loop[y,x]=c;
            }
            // Source is all opaque ground from row688 down. Reuse that texture
            // only below row928; source skyline and mid-earth remain untouched.
            const int groundTop=688,band=304,join=64;
            int[] shifts={337,811,119,997};
            int filled=originalHeight,iteration=0;
            while(filled<h){
                int start=filled-join,shift=shifts[iteration++%shifts.Length];
                int[,] joinCost=new int[w,join];
                for(int x=0;x<w;x++)for(int y=0;y<join;y++)
                    joinCost[x,y]=Cost(canvas[start+y,x],loop[groundTop+y,(x+shift)%w]);
                int[] cut=Cut(joinCost);
                for(int y=0;y<band&&start+y<h;y++)for(int x=0;x<w;x++){
                    if(y<cut[x])continue;
                    int c=loop[groundTop+y,(x+shift)%w];
                    if((c>>24&255)!=255)throw new InvalidDataException("TRANSPARENT_GROUND_DONOR");
                    canvas[start+y,x]=c;
                }
                filled=start+band;
            }
            Directory.CreateDirectory(output);
            using(var png=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)png.SetPixel(x,y,Color.FromArgb(canvas[y,x]));
                png.Save(Path.Combine(output,"Far.png"),ImageFormat.Png);
                // Native 1:1 join/depth crops, not game screenshots.
                using(var joinView=new Bitmap(512,1050,PixelFormat.Format32bppArgb)){
                    for(int y=0;y<1050;y++)for(int x=0;x<512;x++)
                        joinView.SetPixel(x,y,Color.FromArgb(canvas[y+250,(w-256+x)%w]));
                    joinView.Save(Path.Combine(output,"Horizontal-Join.png"),ImageFormat.Png);
                }
                using(var depth=png.Clone(new Rectangle(430,820,640,700),PixelFormat.Format32bppArgb))
                    depth.Save(Path.Combine(output,"Ground-Continuation.png"),ImageFormat.Png);
            }
            File.WriteAllText(Path.Combine(output,"fit.json"),JsonSerializer.Serialize(new{
                sourceSHA256="71C14F299210D278E1EB61B31EEBC9E341F2EF986E0C4D4159D0B3C2A41B785B",
                width=w,height=h,sourceWidth=originalWidth,sourceHeight=originalHeight,
                horizontalOverlap=overlap,unchangedRectangle=new[]{128,0,1330,928},
                groundDonorRows=new[]{688,991},groundContinuationStart=928,
                sourceScale=1,rawBytes=w*h*4,scope="Offline native-pixel fitting candidate; repeated texture quilting is not new painted detail or live art approval."
            },new JsonSerializerOptions{WriteIndented=true}));
        }
    }
}
'@
[NativeCityFit]::Run($inputPath,$output)
Get-FileHash -LiteralPath (Join-Path $output 'Far.png')
