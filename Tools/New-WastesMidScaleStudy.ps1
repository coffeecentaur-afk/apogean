param([Parameter(Mandatory)][string]$OutputDirectory,
    [string]$CheckpointCandidatePath = '', [string]$CheckpointSHA256 = '')
# Explicit 2/5 concept reduction into an OFFLINE native-size upper-module study.
# Not a runtime package: deep cliff continuation and live coverage are separate.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root 'Art/Candidates/WastesMidRuins-v2'
$entries=@(
    @{name='Station';path='Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png';hash='C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C';ground=440;denominator=1;numerator=1},
    @{name='BrokenShell';path='Art/Candidates/WastesMidRuins-v2/Transparent-v1/BrokenShell.png';hash='E5E5B457785977BD90149BE4FE7958A27D4893CD5E8BBC401DCD96020BD4BBDE';ground=685;denominator=5;numerator=2},
    @{name='MotorDepot';path='Art/Candidates/WastesMidRuins-v2/Transparent-v1/MotorDepot.png';hash='4134F85D366AD85C3839696EFFFDE30D3CC99CFB1DF60B5E5041BE5DF2351461';ground=735;denominator=5;numerator=2},
    @{name='Checkpoint';path='Art/Candidates/WastesMidRuins-v2/Transparent-v1/Checkpoint.png';hash='E29115A5686EF2805AD69E5166278CEF5DDF030E3488C47569DF826E0CE4D8B0';ground=740;denominator=5;numerator=2}
)
if($CheckpointCandidatePath){
    if($CheckpointSHA256 -notmatch '^[A-Fa-f0-9]{64}$'){throw 'CHECKPOINT_HASH_REQUIRED'}
    $entries[3].path=[IO.Path]::GetFullPath($CheckpointCandidatePath)
    $entries[3].hash=$CheckpointSHA256
}
foreach($entry in $entries){
    $entry.full=if([IO.Path]::IsPathRooted($entry.path)){$entry.path}else{Join-Path $root $entry.path}
    if((Get-FileHash -LiteralPath $entry.full).Hash -ne $entry.hash){throw "SOURCE_CHANGED_$($entry.name)"}
}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidScaleStudy {
    const int width=512,height=460,ground=340;
    public static void Make(string source,string output,string name,string hash,int sourceGround,int numerator,int denominator) {
        using(var art=new Bitmap(source))using(var result=new Bitmap(width,height,PixelFormat.Format32bppArgb)) {
            int scaledWidth=art.Width*numerator/denominator;
            if(scaledWidth>width)throw new InvalidDataException("STUDY_TOO_WIDE");
            int left=(width-scaledWidth)/2,top=ground-sourceGround*numerator/denominator;
            long kept=0;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
                int rx=x-left,ry=y-top;
                if(rx<0||ry<0)continue;
                // Centre-sampled nearest neighbor; no smooth alpha or recolor.
                int sx=(2*rx+1)*denominator/(2*numerator),sy=(2*ry+1)*denominator/(2*numerator);
                if(sx>=art.Width||sy>=art.Height)continue;
                Color c=art.GetPixel(sx,sy);
                if(c.A!=0&&c.A!=255)throw new InvalidDataException("SOURCE_SOFT_ALPHA");
                if(c.A==0)c=Color.FromArgb(0,0,0,0);else kept++;
                result.SetPixel(x,y,c);
            }
            Directory.CreateDirectory(output);
            result.Save(Path.Combine(output,name+"-Upper.png"),ImageFormat.Png);
            File.WriteAllText(Path.Combine(output,name+"-scale.json"),JsonSerializer.Serialize(new {
                sourceSHA256=hash,sourceWidth=art.Width,sourceHeight=art.Height,sourceGround,
                width,height,ground,numerator,denominator,left,top,kept,
                scope="Offline 1x upper-module study only. Explicit reduction, not added detail or runtime coverage."
            },new JsonSerializerOptions{WriteIndented=true}));
        }
    }
    public static void Board(string folder) {
        string[] names={"Station","BrokenShell","MotorDepot","Checkpoint"};
        foreach(bool dark in new[]{true,false})using(var board=new Bitmap(2048,600,PixelFormat.Format32bppArgb))
        using(var g=Graphics.FromImage(board))using(var font=new Font("Consolas",15))
        using(var small=new Font("Consolas",11))using(var brush=new SolidBrush(dark?Color.Gainsboro:Color.FromArgb(30,30,35))) {
            g.Clear(dark?Color.FromArgb(30,29,33):Color.FromArgb(151,185,201));
            g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString("OFFLINE NATIVE-SIZE STUDY — not an in-game screenshot",font,brush,24,14);
            g.DrawString("Station 1:1 | ruins explicitly reduced to 40% | common soil datum | lower continuation NOT shown",small,brush,24,42);
            int[] xs={0,512,1024,1536};int[] ys={92,92,92,92};
            for(int i=0;i<names.Length;i++) {
                using(var image=new Bitmap(Path.Combine(folder,names[i]+"-Upper.png")))
                    g.DrawImageUnscaled(image,xs[i],ys[i]);
                g.DrawString(names[i],font,brush,xs[i]+30,ys[i]-25);
            }
            g.DrawString("All four drawn at 1:1 preview pixels. Compare door/storey size and intact small features. Lower edges are study crops, NOT final foundations.",small,brush,24,573);
            board.Save(Path.Combine(folder,dark?"Scale-Comparison-Dark.png":"Scale-Comparison-Light.png"),ImageFormat.Png);
        }
    }
}
'@
foreach($entry in $entries){[MidScaleStudy]::Make($entry.full,$output,$entry.name,$entry.hash,$entry.ground,$entry.numerator,$entry.denominator)}
[MidScaleStudy]::Board($output)
Write-Output "Offline native-size upper-module study: $output"
