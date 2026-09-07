param([Parameter(Mandatory)][string]$OutputDirectory)
# REJECTED art probe: visible joins/shoulders. See Art/Candidates/WastesMidBank-v1/README.md.
# Retained for provenance, not a runtime importer or an accepted foundation recipe.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class WastesMidBankAssembly {
    public static void Make(string upperPath,string lowerPath,string output,int dx) {
        using(var upper=new Bitmap(upperPath))using(var lower=new Bitmap(lowerPath))
        using(var result=new Bitmap(512,1408,PixelFormat.Format32bppArgb)) {
            if(upper.Width!=512||upper.Height!=460||lower.Height!=1408)throw new InvalidDataException("DIMENSIONS");
            // A translation, not a resize: gallery soil340 becomes module soil440.
            // Reuse the existing authored deep cliff at its native pixel scale.
            // Only add underneath the last original ground pixel in each column.
            int[] foot=new int[512];int first=512,last=-1;
            for(int x=0;x<512;x++) {
                foot[x]=-1;
                for(int y=340;y<460;y++)if(upper.GetPixel(x,y).A==255)foot[x]=y+100;
                if(foot[x]>=0){first=Math.Min(first,x);last=Math.Max(last,x);}
            }
            if(last<first)throw new InvalidDataException("NO_GROUND");
            for(int y=440;y<1408;y++)for(int x=0;x<512;x++) {
                int sx=x-dx;
                if(sx<0||sx>=lower.Width)continue;
                // Natural lower contours may broaden only below the source upper.
                // No full-width fill, reflected rows, scaling, smoothing or color-key.
                if(y<560 && (foot[x]<0||y<=foot[x]))continue;
                Color c=lower.GetPixel(sx,y);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("LOWER_SOFT_ALPHA");
                if(c.A==255)result.SetPixel(x,y,c);
            }
            for(int y=0;y<460;y++)for(int x=0;x<512;x++) {
                Color c=upper.GetPixel(x,y);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("UPPER_SOFT_ALPHA");
                if(c.A==255)result.SetPixel(x,y+100,c);
            }
            result.Save(output,ImageFormat.Png);
        }
    }
    public static void Board(string path) {
        using(var b=new Bitmap(2048,1408,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b)) {
            g.Clear(Color.FromArgb(43,39,44));int i=0;
            foreach(string name in new[]{"Station","MotorDepot","BrokenShell","Checkpoint"})
                using(var t=new Bitmap(Path.Combine(path,name+".png")))g.DrawImageUnscaled(t,512*i++,0);
            b.Save(Path.Combine(path,"Assembly-Dark.png"),ImageFormat.Png);
        }
    }
}
'@
$entries=@(
    @{name='Station';upper='Art/Candidates/WastesStationBridge-v1/Study/Station-Upper.png';upperHash='81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861';lower='Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png';dx=12},
    @{name='MotorDepot';upper='Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1/MotorDepot-Upper.png';upperHash='AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890';lower='Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png';dx=12},
    @{name='BrokenShell';upper='Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3/BrokenShell-Upper.png';upperHash='371989108EEF09CE9DAB06326E7BDCE34505DCC597775FD4EDDA828A7C9229DA';lower='Content/Backgrounds/Candidates/WastesModules/Highway.png';dx=-32},
    @{name='Checkpoint';upper='Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3/Checkpoint-Upper.png';upperHash='8512BBAE3F7FFEAE8BBD5CA3E7ECC4F1A0B434A7382FC266B7A005201643F0C5';lower='Content/Backgrounds/Candidates/WastesModules/Quiet.png';dx=60}
)
foreach($entry in $entries){if((Get-FileHash -LiteralPath (Join-Path $root $entry.upper)).Hash -ne $entry.upperHash){throw 'APPROVED_UPPER_CHANGED'}}
New-Item -ItemType Directory -Path $output | Out-Null
$records=@(foreach($entry in $entries){
    $upper=Join-Path $root $entry.upper;$lower=Join-Path $root $entry.lower
    $asset=Join-Path $output ($entry.name+'.png')
    [WastesMidBankAssembly]::Make($upper,$lower,$asset,$entry.dx)
    [ordered]@{name=$entry.name;upper=$entry.upper;upperSHA256=$entry.upperHash;lower=$entry.lower;lowerSHA256=(Get-FileHash -LiteralPath $lower).Hash;lowerOffsetX=$entry.dx;assetSHA256=(Get-FileHash -LiteralPath $asset).Hash}
})
[WastesMidBankAssembly]::Board($output)
[ordered]@{recipe='NativeMidBank-probe-v1';width=512;height=1408;upperOffsetY=100;soilRow=440;assets=$records;scope='Offline native-pixel reuse probe. Original visible upper pixels preserved; transparent space below ground may gain cliff. No installation, new painted detail or art approval.'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $output 'assembly.json')
Write-Output "Offline assembly probe: $output"
