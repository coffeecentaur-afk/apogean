param([Parameter(Mandatory)][string]$OutputDirectory, [string]$DecisionPath)
# Source-specific review proposals for three pinned concept originals, not a
# generic background remover or production atlas import. Originals are never
# edited; optional prepared-color derivatives retain exact edge donor records.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$sources = @(
    @{ name='BrokenShell'; path='Art/Candidates/WastesMidRuin-v1/Concept-original.png'; hash='7A088DFB2AE4C1A2AF3DCD531D00179A839B4C1375ED92BD5D6743786A9CF146' },
    @{ name='MotorDepot'; path='Art/Candidates/WastesMidRuins-v2/MotorDepot-original.png'; hash='456442A7E3D7454DB3BAB4794E34A57004BB58A784A93E2395E3CE39D071DB2F' },
    @{ name='Checkpoint'; path='Art/Candidates/WastesMidRuins-v2/Checkpoint-original.png'; hash='3702DD05277CD7630F9F47409356103A36FEB256A07339097AD3C22B7C25B044' }
)
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'USE_NEW_OUTPUT_DIRECTORY' }
foreach ($source in $sources) {
    $source.full = Join-Path $root $source.path
    if ((Get-FileHash -LiteralPath $source.full).Hash -ne $source.hash) { throw "SOURCE_CHANGED_$($source.name)" }
}
$decision = $null
if ($DecisionPath) {
    $decision = Get-Content -Raw -LiteralPath $DecisionPath | ConvertFrom-Json
    if ($decision.recipeVersion -ne 1 -or $decision.reviewState -ne 'agent-reviewed') { throw 'REVIEW_REQUIRED' }
    foreach ($source in $sources) {
        $entry = @($decision.sources | Where-Object name -eq $source.name)
        if ($entry.Count -ne 1 -or $entry[0].sourceSHA256 -ne $source.hash) { throw 'DECISION_SOURCE_MISMATCH' }
        $source.decision = $entry[0]
    }
}
Add-Type -AssemblyName System.Drawing
$refs = @([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidRuinMaskProposal {
    public static void Run(string path,string output,string name,string hash,int[] selected,bool finalized) {
        using(var art=new Bitmap(path)) {
            int w=art.Width,h=art.Height;
            if(w!=1024 || h!=1536) throw new InvalidDataException("SOURCE_DIMENSIONS_CHANGED");
            var labels=new int[w*h]; var pixels=new Color[w*h];
            var proposed=new bool[w*h];
            long transparent=0,partial=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                Color c=art.GetPixel(x,y); int p=y*w+x; pixels[p]=c;
                if(c.A==0)transparent++;else if(c.A!=255)partial++;
                int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                // Only these pinned sources: painted neutral-white matte is
                // distinct from their dark warm materials. Agent review remains required.
                proposed[p]=c.A==255 && lo>=200 && hi-lo<=18;
            }
            if(transparent!=0 || partial!=0)throw new InvalidDataException("EXPECTED_OPAQUE_CONCEPT");
            int next=0; var components=new List<object>();
            for(int p=0;p<labels.Length;p++) {
                if(!proposed[p] || labels[p]!=0)continue;
                int id=++next,area=0,minX=w,minY=h,maxX=0,maxY=0; bool border=false;
                var queue=new Queue<int>();queue.Enqueue(p);labels[p]=id;
                while(queue.Count>0) {
                    int at=queue.Dequeue(),x=at%w,y=at/w;area++;
                    minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);
                    border|=x==0||y==0||x==w-1||y==h-1;
                    foreach(int n in new[]{x>0?at-1:-1,x<w-1?at+1:-1,y>0?at-w:-1,y<h-1?at+w:-1})
                        if(n>=0&&proposed[n]&&labels[n]==0){labels[n]=id;queue.Enqueue(n);}
                }
                components.Add(new{id,area,minX,minY,maxX,maxY,border});
            }
            var removals=new HashSet<int>(selected);
            if(finalized) {
                if(removals.Count==0||removals.Count!=selected.Length)throw new InvalidDataException("INVALID_COMPONENTS");
                foreach(int id in selected)if(id<1||id>next)throw new InvalidDataException("INVALID_COMPONENTS");
                for(int p=0;p<labels.Length;p++)proposed[p]=removals.Contains(labels[p]);
            }
            var prepared=(Color[])pixels.Clone();var changes=new List<int[]>();
            if(finalized) {
                // Separately recorded source-color preparation, not silhouette erosion.
                // Replace neutral matte-contaminated edge RGB with nearby original
                // material RGB. Interior colors and the reviewed mask stay unchanged.
                for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                    int at=y*w+x;if(proposed[at])continue;Color c=pixels[at];
                    int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                    if(lo<85||hi-lo>35)continue;
                    bool near=false;
                    for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){
                        int nx=x+dx,ny=y+dy;if(nx>=0&&nx<w&&ny>=0&&ny<h&&proposed[ny*w+nx])near=true;
                    }
                    if(!near)continue;
                    int best=-1,bestDistance=1000;
                    for(int dy=-6;dy<=6;dy++)for(int dx=-6;dx<=6;dx++) {
                        int nx=x+dx,ny=y+dy,dist=dx*dx+dy*dy;
                        if(dist==0||dist>36||dist>=bestDistance||nx<1||nx>=w-1||ny<1||ny>=h-1)continue;
                        int donor=ny*w+nx;if(proposed[donor])continue;Color d=pixels[donor];
                        int dlo=Math.Min(d.R,Math.Min(d.G,d.B)),dhi=Math.Max(d.R,Math.Max(d.G,d.B));
                        if(dlo>=85&&dhi-dlo<=35)continue;
                        if((299*c.R+587*c.G+114*c.B)-(299*d.R+587*d.G+114*d.B)<25000)continue;
                        int support=0;
                        for(int ay=-1;ay<=1;ay++)for(int ax=-1;ax<=1;ax++)
                            if((ax!=0||ay!=0)&&!proposed[(ny+ay)*w+nx+ax])support++;
                        if(support<2)continue;
                        best=donor;bestDistance=dist;
                    }
                    if(best<0)continue;
                    prepared[at]=pixels[best];changes.Add(new[]{x,y,best%w,best/w});
                }
            }
            Directory.CreateDirectory(output);
            using(var colorSource=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                if(finalized){
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++)colorSource.SetPixel(x,y,prepared[y*w+x]);
                    colorSource.Save(Path.Combine(output,name+"-EdgeSource.png"),ImageFormat.Png);
                    File.WriteAllText(Path.Combine(output,name+"-edge-changes.json"),JsonSerializer.Serialize(changes));
                }
            }
            using(var mask=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var dark=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var light=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                int minX=w,minY=h,maxX=0,maxY=0;long removed=0;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                    int p=y*w+x;bool remove=proposed[p];if(remove)removed++;
                    else {minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
                    mask.SetPixel(x,y,remove?Color.Black:Color.White);
                    dark.SetPixel(x,y,remove?Color.FromArgb(255,30,29,33):prepared[p]);
                    light.SetPixel(x,y,remove?Color.FromArgb(255,151,185,201):prepared[p]);
                }
                mask.Save(Path.Combine(output,name+"-Mask.png"),ImageFormat.Png);
                dark.Save(Path.Combine(output,name+"-Dark.png"),ImageFormat.Png);
                light.Save(Path.Combine(output,name+"-Light.png"),ImageFormat.Png);
                File.WriteAllText(Path.Combine(output,name+"-proposal.json"),JsonSerializer.Serialize(new {
                    recipeVersion=1,sourceSHA256=hash,state=finalized?"agent-reviewed mask; edge-color candidate":"UNREVIEWED proposal",width=w,height=h,
                    selectedComponents=selected,edgeColorChanges=changes.Count,
                    sourceTransparentPixels=transparent,sourcePartialPixels=partial,proposedRemoved=removed,
                    keptBounds=new{minX,minY,maxX,maxY},components
                },new JsonSerializerOptions{WriteIndented=true}));
            }
        }
    }
}
'@
foreach ($source in $sources) {
    $selected = if ($decision) { [int[]]$source.decision.removeComponents } else { [int[]]@() }
    [MidRuinMaskProposal]::Run($source.full,$output,$source.name,$source.hash,$selected,[bool]$decision)
}
Write-Output "Masks and offline light/dark previews (not game screenshots): $output"
