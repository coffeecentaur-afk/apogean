param([Parameter(Mandatory)][string]$OutputDirectory,
    [int[]]$RemoveComponents=@(), [switch]$Finalize)
# Pinned, source-specific dark-matte proposal. Selected components, not a
# global dark-color key, own final removal. The lower original is preserved.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$edited=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/FacingStudy-v1/Checkpoint-opposite-original.png'
$original=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/Transparent-v1/Checkpoint.png'
if((Get-FileHash -LiteralPath $edited).Hash -ne '49335F9D35A53D86AAEB71840A170516F3D9DFCDBB2BBE5BA06F73D1616E23AB'){throw 'EDIT_CHANGED'}
if((Get-FileHash -LiteralPath $original).Hash -ne 'E29115A5686EF2805AD69E5166278CEF5DDF030E3488C47569DF826E0CE4D8B0'){throw 'ORIGINAL_CHANGED'}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class CheckpointFacing {
    public static void Run(string edited,string original,string output,int[] selections,bool finalize){
        using(var upper=new Bitmap(edited))using(var lower=new Bitmap(original)) {
            const int w=1024,h=1536,cut=740;
            if(upper.Width!=w||upper.Height!=h||lower.Width!=w||lower.Height!=h)throw new InvalidDataException("DIMENSIONS");
            var proposed=new bool[w*cut];var labels=new int[w*cut];int next=0;
            for(int y=0;y<cut;y++)for(int x=0;x<w;x++){
                Color c=upper.GetPixel(x,y);
                int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                proposed[y*w+x]=hi<=20&&hi-lo<=10;
            }
            var components=new List<object>();
            for(int p=0;p<labels.Length;p++) {
                if(!proposed[p]||labels[p]!=0)continue;
                int id=++next,area=0,minX=w,minY=cut,maxX=0,maxY=0;bool border=false;
                var queue=new Queue<int>();labels[p]=id;queue.Enqueue(p);
                while(queue.Count>0){
                    int at=queue.Dequeue(),x=at%w,y=at/w;area++;
                    minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);
                    border|=x==0||x==w-1||y==0;
                    foreach(int n in new[]{x>0?at-1:-1,x<w-1?at+1:-1,y>0?at-w:-1,y<cut-1?at+w:-1})
                        if(n>=0&&proposed[n]&&labels[n]==0){labels[n]=id;queue.Enqueue(n);}
                }
                components.Add(new{id,area,minX,minY,maxX,maxY,border});
            }
            var remove=new HashSet<int>(selections);
            if(finalize){
                if(remove.Count==0||remove.Count!=selections.Length)throw new InvalidDataException("INVALID_SELECTION");
                foreach(int id in selections)if(id<1||id>next)throw new InvalidDataException("INVALID_SELECTION");
            }
            Directory.CreateDirectory(output);
            using(var mask=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var source=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var dark=new Bitmap(w,h,PixelFormat.Format32bppArgb))
            using(var light=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                long removed=0;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                    Color c=y<cut?upper.GetPixel(x,y):lower.GetPixel(x,y);
                    bool empty=y<cut?(finalize?remove.Contains(labels[y*w+x]):proposed[y*w+x]):c.A==0;
                    if(empty)removed++;
                    mask.SetPixel(x,y,empty?Color.Black:Color.White);
                    source.SetPixel(x,y,c);
                    dark.SetPixel(x,y,empty?Color.FromArgb(255,30,29,33):c);
                    light.SetPixel(x,y,empty?Color.FromArgb(255,151,185,201):c);
                }
                mask.Save(Path.Combine(output,"Checkpoint-Mask.png"),ImageFormat.Png);
                source.Save(Path.Combine(output,"Checkpoint-Source.png"),ImageFormat.Png);
                dark.Save(Path.Combine(output,"Checkpoint-Dark.png"),ImageFormat.Png);
                light.Save(Path.Combine(output,"Checkpoint-Light.png"),ImageFormat.Png);
                File.WriteAllText(Path.Combine(output,"mask-record.json"),JsonSerializer.Serialize(new {
                    recipeVersion=1,sourceWidth=w,sourceHeight=h,upperRows=cut,removed,
                    sourceEditSHA256="49335F9D35A53D86AAEB71840A170516F3D9DFCDBB2BBE5BA06F73D1616E23AB",
                    preservedOriginalSHA256="E29115A5686EF2805AD69E5166278CEF5DDF030E3488C47569DF826E0CE4D8B0",
                    status=finalize?"agent-selected mask; offline candidate":"UNREVIEWED components",
                    selectedComponents=selections,components
                },new JsonSerializerOptions{WriteIndented=true}));
            }
        }
    }
}
'@
[CheckpointFacing]::Run($edited,$original,$output,$RemoveComponents,[bool]$Finalize)
Write-Output "Source-specific offline mask output: $output"
