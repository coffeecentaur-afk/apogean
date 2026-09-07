param([string]$CandidateDirectory='Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/Selected',
    [ValidateSet('None','MatteLeak','WrongSample','InteriorColor','AlphaErode')][string]$Fault='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=[IO.Path]::GetFullPath($CandidateDirectory)
$source=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/MotorDepot-original.png'
foreach($pin in @(
    @($source,'6CBDBE740DBD73ACF85D50672F3ED45C63887AD010A505BEB90FF73861BCB034'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png'),'C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/QA-Package-v1/Foreground-Deep.png'),'9D039C003929EC128F43C3DFEFB3F98C22DC15488EB0AA9B0F9151C00D726FE4'),
    @((Join-Path $root 'Art/Candidates/WastesFarCity-v1/Runtime-v1/Far.png'),'D61106D719B292675607B8D9275BDCA73BE7CE01D9405606CF04EB8D1C966FE6'))){
    if((Get-FileHash -LiteralPath $pin[0]).Hash -ne $pin[1]){throw 'PIN_CHANGED'}
}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidStyleAudit {
    public static int Run(string original,string folder,string fault){
        using(var src=new Bitmap(original))
        using(var prep=new Bitmap(Path.Combine(folder,"MotorDepot-EdgeSource.png")))
        using(var mask=new Bitmap(Path.Combine(folder,"MotorDepot-Mask.png")))
        using(var rgba=new Bitmap(Path.Combine(folder,"MotorDepot-Transparent.png")))
        using(var upper=new Bitmap(Path.Combine(folder,"MotorDepot-Upper.png"))){
            int w=src.Width,h=src.Height;
            if(w!=1317||h!=1194||prep.Width!=w||prep.Height!=h||mask.Width!=w||mask.Height!=h||rgba.Width!=w||rgba.Height!=h||upper.Width!=512||upper.Height!=432)
                throw new InvalidDataException("DIMENSIONS");
            // Defects live only in these disposable decoded bitmaps.
            if(fault=="MatteLeak")rgba.SetPixel(0,0,Color.White);
            if(fault=="WrongSample")upper.SetPixel(256,300,Color.Magenta);
            if(fault=="InteriorColor")prep.SetPixel(300,700,Color.Magenta);
            if(fault=="AlphaErode")mask.SetPixel(500,700,Color.Black);
            var changes=JsonSerializer.Deserialize<int[][]>(File.ReadAllText(Path.Combine(folder,"edge-changes.json")));
            var donors=new Dictionary<int,int>();
            foreach(var c in changes){
                if(c.Length!=4||c[0]<0||c[0]>=w||c[1]<0||c[1]>=h||c[2]<0||c[2]>=w||c[3]<0||c[3]>=h)
                    throw new InvalidDataException("DONOR_BOUNDS");
                int dx=c[2]-c[0],dy=c[3]-c[1];
                if(dx*dx+dy*dy>36||dx*dx+dy*dy==0)throw new InvalidDataException("DONOR_DISTANCE");
                if(mask.GetPixel(c[0],c[1]).ToArgb()!=Color.White.ToArgb()||mask.GetPixel(c[2],c[3]).ToArgb()!=Color.White.ToArgb())
                    throw new InvalidDataException("DONOR_OUTSIDE");
                bool edge=false;
                for(int ey=-2;ey<=2;ey++)for(int ex=-2;ex<=2;ex++){
                    int nx=c[0]+ex,ny=c[1]+ey;
                    if(nx>=0&&ny>=0&&nx<w&&ny<h&&mask.GetPixel(nx,ny).ToArgb()==Color.Black.ToArgb())edge=true;
                }
                if(!edge)throw new InvalidDataException("NON_EDGE_RECOLOR");
                donors.Add(c[1]*w+c[0],c[3]*w+c[2]);
            }
            long removed=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                Color c=src.GetPixel(x,y);
                int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
                // The reviewed source has seven bright components, all selected;
                // assert the original decision, not arbitrary supplied mask pixels.
                bool empty=lo>=225&&hi-lo<=12;
                if(mask.GetPixel(x,y).ToArgb()!=(empty?Color.Black:Color.White).ToArgb())throw new InvalidDataException("MASK_CHANGED");
                if(empty)removed++;
                int donor;
                Color expected=donors.TryGetValue(y*w+x,out donor)?src.GetPixel(donor%w,donor/w):c;
                if(prep.GetPixel(x,y).ToArgb()!=expected.ToArgb())throw new InvalidDataException("COLOR_PROVENANCE");
                int argb=empty?0:expected.ToArgb();
                if(rgba.GetPixel(x,y).ToArgb()!=argb)throw new InvalidDataException("RGBA_EXPORT");
            }
            if(removed!=942706)throw new InvalidDataException("MASK_COUNT");
            for(int y=0;y<432;y++)for(int x=0;x<512;x++){
                int sx=(x-32)*3+1,sy=(y-38)*3+1;
                int expected=sx<0||sy<0||sx>=w||sy>=h?0:rgba.GetPixel(sx,sy).ToArgb();
                if(upper.GetPixel(x,y).ToArgb()!=expected)throw new InvalidDataException("STUDY_SAMPLE");
            }
            return changes.Length;
        }
    }
}
'@
$colorChanges=[MidStyleAudit]::Run($source,$folder,$Fault)
Write-Output "PASS: 1572498 source/mask/export pixels; $colorChanges bounded RGB donors; 221184 exact fitted samples; accepted art pins unchanged."
Write-Output 'Offline technical audit only; art review, full-depth assembly and new native proof remain pending.'
