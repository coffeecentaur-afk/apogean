param(
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$DecisionPath
)
# Source-specific mask authoring aid, NOT the reusable exporter.
# Proposal colors never become a production key; reviewed component IDs own removal.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/WastesFarCity-v1/City-Concept.png'
$sourceHash='583BE8E5801C37887CFFC9A9E73FC1FD1A4B4AA1E2E3DA346FDFF0F1E3C40AF7'
if((Get-FileHash -LiteralPath $source).Hash -ne $sourceHash){throw 'CITY_SOURCE_CHANGED'}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_A_NEW_MASK_OUTPUT_DIRECTORY'}
$removed=[int[]]@()
if($DecisionPath){
    $decision=Get-Content -LiteralPath $DecisionPath -Raw | ConvertFrom-Json
    if($decision.sourceSHA256 -ne $sourceHash -or $decision.recipeVersion -ne 1){throw 'MASK_DECISION_WRONG_SOURCE_OR_RECIPE'}
    if($decision.reviewState -ne 'agent-reviewed'){throw 'MASK_REVIEW_REQUIRED'}
    $proposal=Join-Path $root 'Art/Candidates/WastesFarCity-v1/Mask-Proposal/City-Mask.png'
    if((Get-FileHash -LiteralPath $proposal).Hash -ne $decision.proposalMaskSHA256){throw 'PROPOSAL_MASK_CHANGED'}
    $removed=[int[]]$decision.removeComponents
}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json','System.Linq')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
public static class WastesCityMaskAuthor {
    public static void Run(string source,string output,int[] removals,bool finalized) {
        using(var art=new Bitmap(source)) {
            int w=art.Width,h=art.Height;
            if(w!=1586 || h!=992) throw new InvalidDataException("CITY_DIMENSIONS_CHANGED");
            int[] labels=new int[w*h]; bool[] proposed=new bool[w*h];
            // This known source's sky occupies the upper facade band.
            // Limit proposal to that band; preserve every lower-earth pixel.
            for(int y=0;y<Math.Min(570,h);y++) for(int x=0;x<w;x++) {
                Color c=art.GetPixel(x,y);
                int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                proposed[y*w+x]=lo>=200 && hi-lo<=18;
            }
            var components=new List<object>(); int next=0;
            for(int p=0;p<labels.Length;p++) {
                if(!proposed[p] || labels[p]!=0)continue;
                int id=++next,area=0,minX=w,maxX=0,minY=h,maxY=0;
                bool top=false; var queue=new Queue<int>();
                labels[p]=id;queue.Enqueue(p);
                while(queue.Count>0) {
                    int at=queue.Dequeue(),x=at%w,y=at/w; area++;
                    minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);top|=y==0;
                    foreach(int neighbor in new[]{x>0?at-1:-1,x<w-1?at+1:-1,y>0?at-w:-1,y<h-1?at+w:-1})
                        if(neighbor>=0 && proposed[neighbor] && labels[neighbor]==0){labels[neighbor]=id;queue.Enqueue(neighbor);}
                }
                components.Add(new{id,area,minX,minY,maxX,maxY,touchesTop=top});
            }
            var selected=new HashSet<int>(removals);
            if(finalized && (selected.Count==0 || selected.Any(id=>id<1 || id>next) || selected.Count!=removals.Length))
                throw new InvalidDataException("INVALID_COMPONENT_DECISION");
            Directory.CreateDirectory(output);
            bool[] removedPixels=new bool[w*h];
            for(int p=0;p<labels.Length;p++)removedPixels[p]=finalized?selected.Contains(labels[p]):labels[p]!=0;
            var changes=new List<int[]>();
            using(var prepared=new Bitmap(art)) {
            if(finalized) {
                // Color-only preparation is separate from mask export.
                // Only neutral pale pixels within two pixels of selected sky/openings.
                for(int y=0;y<570;y++)for(int x=0;x<w;x++){
                    if(removedPixels[y*w+x])continue;
                    Color c=art.GetPixel(x,y);
                    int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                    if(lo<85 || hi-lo>35)continue;
                    bool near=false;
                    for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){
                        int nx=x+dx,ny=y+dy;
                        if(nx>=0&&nx<w&&ny>=0&&ny<h&&removedPixels[ny*w+nx])near=true;
                    }
                    if(!near)continue;
                    var donors=new List<int[]>();
                    for(int dy=-6;dy<=6;dy++)for(int dx=-6;dx<=6;dx++){
                        int nx=x+dx,ny=y+dy,dist=dx*dx+dy*dy;
                        if(dist==0||dist>36||nx<1||nx>=w-1||ny<1||ny>=h-1||removedPixels[ny*w+nx])continue;
                        Color d=art.GetPixel(nx,ny);
                        int dlo=Math.Min(d.R,Math.Min(d.G,d.B)),dhi=Math.Max(d.R,Math.Max(d.G,d.B));
                        if(dlo>=85&&dhi-dlo<=35)continue; // Do not propagate other pale fringe.
                        int luminance=(d.R*299+d.G*587+d.B*114)/1000;
                        if((c.R*299+c.G*587+c.B*114)/1000-luminance<25)continue;
                        int supported=0;
                        for(int ay=-1;ay<=1;ay++)for(int ax=-1;ax<=1;ax++)
                            if((ax!=0||ay!=0)&&!removedPixels[(ny+ay)*w+nx+ax])supported++;
                        if(supported>=2)donors.Add(new[]{dist,luminance,nx,ny});
                    }
                    if(donors.Count==0)continue;
                    var nearest=donors.OrderBy(d=>d[0]).ThenBy(d=>d[3]).ThenBy(d=>d[2]).Take(5).OrderBy(d=>d[1]).ToArray();
                    int[] donor=nearest[nearest.Length/2];
                    prepared.SetPixel(x,y,art.GetPixel(donor[2],donor[3]));
                    changes.Add(new[]{x,y,donor[2],donor[3]});
                }
                prepared.Save(Path.Combine(output,"City-EdgeSource.png"),ImageFormat.Png);
                File.WriteAllText(Path.Combine(output,"edge-color-changes.json"),JsonSerializer.Serialize(changes));
            }
            using(var mask=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var overlay=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var dark=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var light=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                    Color c=prepared.GetPixel(x,y);
                    bool remove=removedPixels[y*w+x];
                    mask.SetPixel(x,y,remove?Color.Black:Color.White);
                    overlay.SetPixel(x,y,remove?Color.FromArgb(255,(c.R+200)/2,c.G/2,(c.B+190)/2):c);
                    dark.SetPixel(x,y,remove?Color.FromArgb(255,38,35,32):c);
                    light.SetPixel(x,y,remove?Color.FromArgb(255,160,184,199):c);
                }
                mask.Save(Path.Combine(output,"City-Mask.png"),ImageFormat.Png);
                overlay.Save(Path.Combine(output,"Mask-Overlay.png"),ImageFormat.Png);
                dark.Save(Path.Combine(output,"Preview-Dark.png"),ImageFormat.Png);
                light.Save(Path.Combine(output,"Preview-Light.png"),ImageFormat.Png);
                // Native crops, no interpolation/enlargement. Locate facade openings and thin antennas.
                foreach(var crop in new[]{new Rectangle(730,320,390,250),new Rectangle(270,375,290,200),new Rectangle(1140,390,310,190)})
                using(var a=dark.Clone(crop,PixelFormat.Format32bppArgb))
                using(var b=light.Clone(crop,PixelFormat.Format32bppArgb)){
                    string name=crop.X+"-"+crop.Y;
                    a.Save(Path.Combine(output,"Edge-Dark-"+name+".png"),ImageFormat.Png);
                    b.Save(Path.Combine(output,"Edge-Light-"+name+".png"),ImageFormat.Png);
                }
            }
            }
            File.WriteAllText(Path.Combine(output,"components.json"),JsonSerializer.Serialize(new{recipeVersion=1,width=w,height=h,
                sourceSHA256="583BE8E5801C37887CFFC9A9E73FC1FD1A4B4AA1E2E3DA346FDFF0F1E3C40AF7",
                state=finalized?"agent-reviewed candidate mask":"UNREVIEWED proposal",
                selectedComponents=removals,edgeColorChanges=changes.Count,components},new JsonSerializerOptions{WriteIndented=true}));
        }
    }
}
'@
[WastesCityMaskAuthor]::Run($source,$output,$removed,[bool]$DecisionPath)
if($DecisionPath){
    if((Get-FileHash -LiteralPath (Join-Path $output 'City-Mask.png')).Hash -ne $decision.expectedMaskSHA256){throw 'FINAL_MASK_CHANGED'}
}
Write-Output "Mask authoring output: $output. Previews are offline source pixels, not game screenshots."
