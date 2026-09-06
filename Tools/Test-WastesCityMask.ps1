param([switch]$TestDefects)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root 'Art/Candidates/WastesFarCity-v1/Transparent-v1'
$original=Join-Path $root 'Art/Candidates/WastesFarCity-v1/City-Concept.png'
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Console','System.Collections','System.Text.Json','System.Linq')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class CityMaskProof {
    public static int Verify(Bitmap original,Bitmap prepared,Bitmap mask,Bitmap output,int[][] changes) {
        int w=original.Width,h=original.Height;
        foreach(var b in new[]{prepared,mask,output})if(b.Width!=w||b.Height!=h)throw new InvalidDataException("DIMENSIONS_CHANGED");
        var recorded=new Dictionary<int,int[]>();
        foreach(int[] p in changes) {
            if(p.Length!=4||p[0]<0||p[0]>=w||p[1]<0||p[1]>=h||p[2]<0||p[2]>=w||p[3]<0||p[3]>=h||!recorded.TryAdd(p[1]*w+p[0],p))
                throw new InvalidDataException("INVALID_CHANGE_RECORD");
        }
        int count=0;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            Color a=original.GetPixel(x,y),b=prepared.GetPixel(x,y),m=mask.GetPixel(x,y),c=output.GetPixel(x,y);
            if(m.A!=255||m.R!=m.G||m.G!=m.B||(m.R!=0&&m.R!=255))throw new InvalidDataException("BAD_MASK");
            bool changed=a.ToArgb()!=b.ToArgb();
            if(changed) {
                count++;
                if(a.A!=b.A||y>=570||m.R!=255)throw new InvalidDataException("PREPARATION_OUTSIDE_ALLOWED_AREA");
                bool edge=false;
                for(int yy=Math.Max(0,y-2);yy<=Math.Min(h-1,y+2);yy++)
                    for(int xx=Math.Max(0,x-2);xx<=Math.Min(w-1,x+2);xx++)
                        if(mask.GetPixel(xx,yy).R==0)edge=true;
                if(!edge)throw new InvalidDataException("INTERIOR_CHANGED");
                if(!recorded.TryGetValue(y*w+x,out var p))throw new InvalidDataException("UNRECORDED_CHANGE");
                int dx=x-p[2],dy=y-p[3];
                if(dx*dx+dy*dy>36||mask.GetPixel(p[2],p[3]).R!=255||b.ToArgb()!=original.GetPixel(p[2],p[3]).ToArgb())
                    throw new InvalidDataException("DONOR_MISMATCH");
            } else if(recorded.ContainsKey(y*w+x))throw new InvalidDataException("UNCHANGED_PIXEL_RECORDED");
            if(c.ToArgb()!=(m.R==255?b.ToArgb():0))throw new InvalidDataException("EXPORT_PIXEL_MISMATCH");
        }
        if(count!=recorded.Count)throw new InvalidDataException("CHANGE_COUNT_MISMATCH");
        return count;
    }
    public static void Run(string originalPath,string folder,bool testDefects) {
        using(var original=new Bitmap(originalPath))
        using(var prepared=new Bitmap(Path.Combine(folder,"City-EdgeSource.png")))
        using(var mask=new Bitmap(Path.Combine(folder,"City-Mask.png")))
        using(var output=new Bitmap(Path.Combine(folder,"City-Transparent.png"))) {
            int[][] changes=JsonSerializer.Deserialize<int[][]>(File.ReadAllText(Path.Combine(folder,"edge-color-changes.json")));
            int count=Verify(original,prepared,mask,output,changes);
            Console.WriteLine("PASS actual city: "+count+" recorded edge-color changes; zero interior/lower-earth changes; exact masked export.");
            if(!testDefects)return;
            foreach(string defect in new[]{"interior","alpha","sky"}) {
                using(var p=new Bitmap(prepared))using(var o=new Bitmap(output)){
                    if(defect=="interior")p.SetPixel(800,800,Color.Magenta);
                    if(defect=="alpha")p.SetPixel(800,800,Color.FromArgb(0,0,0,0));
                    if(defect=="sky")o.SetPixel(0,0,Color.White);
                    string expected=defect=="sky"?"EXPORT_PIXEL_MISMATCH":"PREPARATION_OUTSIDE_ALLOWED_AREA";
                    bool rejected=false;
                    try{Verify(original,p,mask,o,changes);}
                    catch(InvalidDataException e){if(e.Message!=expected)throw;rejected=true;}
                    if(!rejected)throw new InvalidDataException("FALSE_PASS_"+defect);
                    Console.WriteLine("PASS city validator rejects in-memory "+defect+" mutation.");
                }
            }
        }
    }
}
'@
[CityMaskProof]::Run($original,$folder,[bool]$TestDefects)
