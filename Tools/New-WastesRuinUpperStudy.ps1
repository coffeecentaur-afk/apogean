param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$family=Join-Path $root 'Art/Candidates/WastesRuinUpperStyle-v1'
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
$station=Join-Path $root 'Art/Candidates/WastesStationBridge-v1/Study/Station-Upper.png'
$garage=Join-Path $root 'Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1/MotorDepot-Upper.png'
if((Get-FileHash -LiteralPath $station).Hash -ne '81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861' -or (Get-FileHash -LiteralPath $garage).Hash -ne 'AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890'){throw 'APPROVED_REFERENCE_CHANGED'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
public static class RuinUpperStudy {
    public static void Fit(string source,string output,int n,int d,int left,int top) {
        using(var a=new Bitmap(source))using(var b=new Bitmap(512,460,PixelFormat.Format32bppArgb)) {
            for(int y=0;y<460;y++)for(int x=0;x<512;x++) {
                int sx=(int)Math.Floor((x-left+0.5)*d/n),sy=(int)Math.Floor((y-top+0.5)*d/n);
                if(sx<0||sx>=a.Width||sy<0||sy>=a.Height)continue;
                Color c=a.GetPixel(sx,sy);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOFT_ALPHA");
                if(c.A==255)b.SetPixel(x,y,c);
            }
            b.Save(output,ImageFormat.Png);
        }
    }
    public static void Board(string station,string garage,string folder,bool dark) {
        using(var b=new Bitmap(1024,1000,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b))
        using(var font=new Font("Consolas",11))using(var ink=new SolidBrush(dark?Color.Gainsboro:Color.FromArgb(35,32,37))) {
            g.Clear(dark?Color.FromArgb(43,39,44):Color.FromArgb(151,185,201));
            g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
            string[] files={station,garage,Path.Combine(folder,"BrokenShell-Upper.png"),Path.Combine(folder,"Checkpoint-Upper.png")};
            string[] labels={"Approved gas station - UNCHANGED","Approved garage - UNCHANGED","Ruined building - NEW TOP candidate","Checkpoint - NEW TOP candidate"};
            for(int i=0;i<4;i++) {
                int x=(i%2)*512,y=(i/2)*480+34;
                g.DrawString(labels[i],font,ink,x+12,y);
                using(var a=new Bitmap(files[i]))g.DrawImageUnscaled(a,x,y+20);
            }
            g.DrawString("OFFLINE 1:1 asset pixels; soil y340 in each panel. Not a game screenshot.",font,ink,12,8);
            g.DrawString("Top style review only. Buildings retain their relative sizes; foundation work is paused.",font,ink,12,978);
            b.Save(Path.Combine(folder,dark?"Comparison-Dark.png":"Comparison-Light.png"),ImageFormat.Png);
        }
    }
}
'@
New-Item -ItemType Directory -Path $output | Out-Null
$entries=@(
    @{name='BrokenShell';n=3;d=8;left=8;top=-35;soilSource=1000},
    @{name='Checkpoint';n=1;d=3;left=32;top=5;soilSource=1005}
)
$records=@(foreach($entry in $entries){
    $source=Join-Path $family "Selected/$($entry.name)-Transparent.png"
    $target=Join-Path $output "$($entry.name)-Upper.png"
    [RuinUpperStudy]::Fit($source,$target,$entry.n,$entry.d,$entry.left,$entry.top)
    [ordered]@{name=$entry.name;sourceSHA256=(Get-FileHash -LiteralPath $source).Hash;candidateSHA256=(Get-FileHash -LiteralPath $target).Hash;numerator=$entry.n;denominator=$entry.d;offsetX=$entry.left;offsetY=$entry.top;sourceSoilRow=$entry.soilSource}
})
[RuinUpperStudy]::Board($station,$garage,$output,$true)
[RuinUpperStudy]::Board($station,$garage,$output,$false)
[ordered]@{recipe='RuinUpperStudy-v1';width=512;height=460;soilRow=340;assets=$records;stationSHA256=(Get-FileHash -LiteralPath $station).Hash;garageSHA256=(Get-FileHash -LiteralPath $garage).Hash;scope='Offline upper-only comparison. Explicit nearest-center reduction and registration; no enlargement or claim of pixel-exact geometry preservation. No runtime import or art approval.'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $output 'study.json')
Write-Output "Upper comparison: $output"
