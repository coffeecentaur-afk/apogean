param([string]$CandidateDirectory='',
    [ValidateSet('None','MatteLeak','WrongSample','InteriorColor','AlphaErode','WrongGrid')][string]$Fault='None',
    [ValidateSet('v1','v2')][string]$Version='v1', [string]$GridReviewDirectory='')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$spec=if($Version -eq 'v1'){
    @{hash='6CBDBE740DBD73ACF85D50672F3ED45C63887AD010A505BEB90FF73861BCB034';width=1317;height=1194;removed=942706;top=38}
}else{
    @{hash='C47B826925B5AF405CF51BDEFA315991F10D07B87ECBE6234086ECCA0E10C10B';width=1316;height=1195;removed=835637;top=58}
}
if(!$CandidateDirectory){$CandidateDirectory=Join-Path $root "Art/Candidates/WastesMidRuins-v2/PixelStyle-$Version/Selected"}
$folder=[IO.Path]::GetFullPath($CandidateDirectory)
$source=Join-Path $root "Art/Candidates/WastesMidRuins-v2/PixelStyle-$Version/MotorDepot-original.png"
if($Fault -eq 'WrongGrid' -and !$GridReviewDirectory){throw 'GRID_REQUIRED_FOR_DEFECT'}
foreach($pin in @(
    @($source,$spec.hash),
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
    public static int Run(string original,string folder,string fault,int width,int height,int expectedRemoved,int top,string gridPath){
        using(var src=new Bitmap(original))
        using(var prep=new Bitmap(Path.Combine(folder,"MotorDepot-EdgeSource.png")))
        using(var mask=new Bitmap(Path.Combine(folder,"MotorDepot-Mask.png")))
        using(var rgba=new Bitmap(Path.Combine(folder,"MotorDepot-Transparent.png")))
        using(var upper=new Bitmap(Path.Combine(folder,"MotorDepot-Upper.png"))){
            int w=src.Width,h=src.Height;
            if(w!=width||h!=height||prep.Width!=w||prep.Height!=h||mask.Width!=w||mask.Height!=h||rgba.Width!=w||rgba.Height!=h||upper.Width!=512||upper.Height!=432)
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
                // Reviewed v1/v2 sources have seven/sixteen bright components, all selected;
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
            if(removed!=expectedRemoved)throw new InvalidDataException("MASK_COUNT");
            for(int y=0;y<432;y++)for(int x=0;x<512;x++){
                int sx=(x-32)*3+1,sy=(y-top)*3+1;
                int expected=sx<0||sy<0||sx>=w||sy>=h?0:rgba.GetPixel(sx,sy).ToArgb();
                if(upper.GetPixel(x,y).ToArgb()!=expected)throw new InvalidDataException("STUDY_SAMPLE");
            }
            if(!string.IsNullOrEmpty(gridPath))using(var grid=new Bitmap(Path.Combine(gridPath,"MotorDepot-Upper.png"))){
                if(grid.Width!=512||grid.Height!=432)throw new InvalidDataException("GRID_DIMENSIONS");
                if(fault=="WrongGrid")grid.SetPixel(256,300,Color.Magenta);
                for(int y=0;y<432;y++)for(int x=0;x<512;x++){
                    int sx=6*(x/2)-96+3,sy=6*(y/2)-3*top+3;
                    int expected=sx<0||sy<0||sx>=w||sy>=h?0:rgba.GetPixel(sx,sy).ToArgb();
                    if(grid.GetPixel(x,y).ToArgb()!=expected)throw new InvalidDataException("GRID_SAMPLE");
                }
            }
            return changes.Length;
        }
    }
}
'@
$colorChanges=[MidStyleAudit]::Run($source,$folder,$Fault,$spec.width,$spec.height,$spec.removed,$spec.top,$GridReviewDirectory)
Write-Output "PASS: $($spec.width*$spec.height) source/mask/export pixels; $colorChanges bounded RGB donors; 221184 exact fitted samples; accepted art pins unchanged."
if($GridReviewDirectory){Write-Output 'PASS: 221184 exact grid samples. Resampling is not a hand-authored style or silhouette acceptance test.'}
Write-Output 'Offline technical audit only; art review, full-depth assembly and new native proof remain pending.'
