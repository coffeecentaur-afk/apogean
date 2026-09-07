param([string]$StudyDirectory='Art/Candidates/WastesRuinUpperStyle-v1/Study',
    [ValidateSet('None','SoftAlpha','HiddenRGB','ChangedSample')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$family=Join-Path $root 'Art/Candidates/WastesRuinUpperStyle-v1'
$study=(Resolve-Path -LiteralPath $StudyDirectory).Path
$pins=@(
    @('Art/Candidates/WastesStationBridge-v1/Study/Station-Upper.png','81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861'),
    @('Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1/MotorDepot-Upper.png','AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890'),
    @('Art/Candidates/WastesRuinUpperStyle-v1/BrokenShell-original.png','5F9004C02792E2F4C386B177A793333C3AF156B9C4AE23D04FC1727A58750FB2'),
    @('Art/Candidates/WastesRuinUpperStyle-v1/Checkpoint-original.png','8D1175CF2B1E0732B10CCFE66B94C613DACDDE1D51AE905E35F376B994E3F2C2'))
foreach($p in $pins){if((Get-FileHash -LiteralPath (Join-Path $root $p[0])).Hash -ne $p[1]){throw 'PINNED_SOURCE_CHANGED'}}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json','System.Console')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class RuinUpperAudit {
    public static void Run(string root,string study,string name,int w,int h,int n,int d,int left,int top,string mutation) {
        string dir=Path.Combine(root,"Selected");
        using(var original=new Bitmap(Path.Combine(root,name+"-original.png")))
        using(var prepared=new Bitmap(Path.Combine(dir,name+"-EdgeSource.png")))
        using(var mask=new Bitmap(Path.Combine(dir,name+"-Mask.png")))
        using(var export=new Bitmap(Path.Combine(dir,name+"-Transparent.png")))
        using(var candidate=new Bitmap(Path.Combine(study,name+"-Upper.png"))) {
            foreach(var image in new[]{original,prepared,mask,export})if(image.Width!=w||image.Height!=h)throw new InvalidDataException("SOURCE_DIMENSIONS");
            if(candidate.Width!=512||candidate.Height!=460)throw new InvalidDataException("STUDY_DIMENSIONS");
            var donors=new Dictionary<int,int>();
            foreach(int[] a in JsonSerializer.Deserialize<int[][]>(File.ReadAllText(Path.Combine(dir,name+"-edge-changes.json")))) {
                if(a.Length!=4||a[0]<0||a[0]>=w||a[1]<0||a[1]>=h||a[2]<0||a[2]>=w||a[3]<0||a[3]>=h)throw new InvalidDataException("DONOR_BOUNDS");
                int dx=a[0]-a[2],dy=a[1]-a[3];
                if(dx*dx+dy*dy>36||donors.ContainsKey(a[1]*w+a[0]))throw new InvalidDataException("DONOR_CONTRACT");
                if(mask.GetPixel(a[2],a[3]).R!=255)throw new InvalidDataException("REMOVED_DONOR");
                bool edge=false;
                for(int yy=Math.Max(0,a[1]-2);yy<=Math.Min(h-1,a[1]+2);yy++)for(int xx=Math.Max(0,a[0]-2);xx<=Math.Min(w-1,a[0]+2);xx++)if(mask.GetPixel(xx,yy).R==0)edge=true;
                if(!edge)throw new InvalidDataException("NON_EDGE_EDIT");
                donors.Add(a[1]*w+a[0],a[3]*w+a[2]);
            }
            long kept=0,clear=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                Color m=mask.GetPixel(x,y);
                if(m.ToArgb()!=Color.White.ToArgb()&&m.ToArgb()!=Color.Black.ToArgb())throw new InvalidDataException("BINARY_MASK");
                int from=donors.TryGetValue(y*w+x,out int donor)?donor:y*w+x;
                if(prepared.GetPixel(x,y).ToArgb()!=original.GetPixel(from%w,from/w).ToArgb())throw new InvalidDataException("SOURCE_PROVENANCE");
                Color expected=m.R==255?prepared.GetPixel(x,y):Color.FromArgb(0,0,0,0);
                if(export.GetPixel(x,y).ToArgb()!=expected.ToArgb())throw new InvalidDataException("EXPORT_PROVENANCE");
                if(expected.A==255)kept++;else clear++;
            }
            if(kept==0||clear==0)throw new InvalidDataException("EMPTY_OR_OPAQUE_EXPORT");
            if(mutation=="SoftAlpha")candidate.SetPixel(0,0,Color.FromArgb(127,50,40,30));
            if(mutation=="HiddenRGB")candidate.SetPixel(0,0,Color.FromArgb(0,50,40,30));
            if(mutation=="ChangedSample")candidate.SetPixel(256,300,Color.Magenta);
            for(int y=0;y<460;y++)for(int x=0;x<512;x++) {
                Color c=candidate.GetPixel(x,y);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
                if(c.A==0&&(c.R!=0||c.G!=0||c.B!=0))throw new InvalidDataException("HIDDEN_RGB");
                // Independent signed integer-center expression of the recorded fit.
                int sx=(int)Math.Floor(((x-left)*2+1)*(double)d/(2*n));
                int sy=(int)Math.Floor(((y-top)*2+1)*(double)d/(2*n));
                Color expected=sx<0||sx>=w||sy<0||sy>=h?Color.FromArgb(0,0,0,0):export.GetPixel(sx,sy);
                if(c.ToArgb()!=expected.ToArgb())throw new InvalidDataException("SAMPLE_PROVENANCE");
            }
            Console.WriteLine("PASS "+name+": "+(w*h)+" source/export pixels; "+donors.Count+" recorded edge donors; 235520 study pixels. Not art or runtime approval.");
        }
    }
}
'@
[RuinUpperAudit]::Run($family,$study,'BrokenShell',1322,1190,3,8,8,-35,$Mutation)
[RuinUpperAudit]::Run($family,$study,'Checkpoint',1323,1189,1,3,32,5,$Mutation)
