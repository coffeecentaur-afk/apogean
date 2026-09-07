param([Parameter(Mandatory)][string]$StudyDirectory,
    [ValidateSet('None','SoftAlpha','SampleChanged','HiddenRGB')][string]$Fault='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$art=Join-Path $root 'Art/Candidates/WastesStationBridge-v1'
$record=Get-Content -Raw -LiteralPath (Join-Path $StudyDirectory 'study.json') | ConvertFrom-Json
$file=Join-Path $StudyDirectory 'Station-Upper.png'
if($record.recipe -ne 'StationBridgeStudy-v1' -or (Get-FileHash -LiteralPath $file).Hash -ne $record.candidateSHA256){throw 'REPORT_MISMATCH'}
$pins=@(
    @('Station-original.png','E5E81D8B8B612ADF8DA814BA207B5868495239D18F631759E13EF47149357F75'),
    @('Selected/Station-EdgeSource.png','1898AB1E4485EA9D535E398BB83C9E44D42893BCBF9D88C2E0EAB05018EA30CC'),
    @('Selected/Station-Mask.png','BB840273D889736FB2A3A73D04A7B03EBCCC520581E2638C1BCD4B87A5232955'),
    @('Selected/Station-Transparent.png','E1E6FF649F83A4997CAEF37A4065A576E115A15B4E959235362C7EAFAF597C30'))
foreach($p in $pins){if((Get-FileHash -LiteralPath (Join-Path $art $p[0])).Hash -ne $p[1]){throw 'SOURCE_CHANGED'}}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class StationBridgeAudit {
    public static void Test(string art,string file,string fault) {
        using(var original=new Bitmap(Path.Combine(art,"Station-original.png")))
        using(var prepared=new Bitmap(Path.Combine(art,"Selected/Station-EdgeSource.png")))
        using(var mask=new Bitmap(Path.Combine(art,"Selected/Station-Mask.png")))
        using(var rgba=new Bitmap(Path.Combine(art,"Selected/Station-Transparent.png")))
        using(var fit=new Bitmap(file)) {
            int[][] changes=JsonSerializer.Deserialize<int[][]>(File.ReadAllText(Path.Combine(art,"Selected/Station-edge-changes.json")));
            var donors=new Dictionary<int,int>();
            foreach(int[] c in changes) {
                if(c.Length!=4||c[0]<0||c[0]>=1323||c[1]<0||c[1]>=1189||c[2]<0||c[2]>=1323||c[3]<0||c[3]>=1189)
                    throw new InvalidDataException("DONOR_BOUNDS");
                int dx=c[0]-c[2],dy=c[1]-c[3];
                if(dx*dx+dy*dy>36||mask.GetPixel(c[0],c[1]).R!=255||mask.GetPixel(c[2],c[3]).R!=255)
                    throw new InvalidDataException("DONOR_MASK_OR_DISTANCE");
                donors.Add(c[1]*1323+c[0],c[3]*1323+c[2]);
            }
            if(donors.Count!=5690)throw new InvalidDataException("DONOR_COUNT");
            for(int y=0;y<1189;y++)for(int x=0;x<1323;x++) {
                int at=y*1323+x,d;bool changed=donors.TryGetValue(at,out d);
                Color expected=changed?original.GetPixel(d%1323,d/1323):original.GetPixel(x,y);
                if(prepared.GetPixel(x,y).ToArgb()!=expected.ToArgb())throw new InvalidDataException("COLOR_PROVENANCE");
                Color m=mask.GetPixel(x,y);
                if(m.ToArgb()!=Color.Black.ToArgb()&&m.ToArgb()!=Color.White.ToArgb())throw new InvalidDataException("MASK");
                if(rgba.GetPixel(x,y).ToArgb()!=(m.R==255?expected.ToArgb():0))throw new InvalidDataException("RGBA_PROVENANCE");
            }
            if(fit.Width!=512||fit.Height!=460)throw new InvalidDataException("FIT_DIMENSIONS");
            if(fault=="SoftAlpha")fit.SetPixel(0,0,Color.FromArgb(128,120,80,40));
            if(fault=="SampleChanged")fit.SetPixel(200,350,Color.Magenta);
            if(fault=="HiddenRGB")fit.SetPixel(0,0,Color.FromArgb(0,255,255,255));
            for(int y=0;y<460;y++)for(int x=0;x<512;x++) {
                Color c=fit.GetPixel(x,y);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
                if(c.A==0&&c.ToArgb()!=0)throw new InvalidDataException("HIDDEN_RGB");
                // Independent integer form of the documented 3/8 nearest-center sampling.
                int sx=(8*x+4)/3,sy=(int)Math.Floor((8*y-316)/3.0);
                int expected=sx<1323&&sy>=0&&sy<1189?rgba.GetPixel(sx,sy).ToArgb():0;
                if(c.ToArgb()!=expected)throw new InvalidDataException("SAMPLE_CHANGED");
            }
        }
    }
}
'@
[StationBridgeAudit]::Test($art,$file,$Fault)
Write-Output 'PASS: 1573047 source pixels, 5690 recorded edge donors and 235520 fit samples audited; binary alpha and empty RGB. Static provenance only, not art or runtime approval.'
