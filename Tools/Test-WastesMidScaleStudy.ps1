param([string]$CandidateDirectory='',
    [ValidateSet('None','MatteLeak','WrongSample','CliffChanged','Misaligned')][string]$Fault='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$family=Join-Path $root 'Art/Candidates/WastesMidRuins-v2'
if(!$CandidateDirectory){$CandidateDirectory=Join-Path $family 'ScaleStudy-v3'}
$candidate=(Resolve-Path -LiteralPath $CandidateDirectory).Path
$facing=Join-Path $family 'FacingStudy-v1/Selected-v1'
$entries=@(
    @{name='Station';source=(Join-Path $root 'Art/Candidates/WastesFarCity-v1/QA-Package-v1/Station.png');hash='C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C';ground=440;n=1;d=1},
    @{name='BrokenShell';source=(Join-Path $family 'Transparent-v1/BrokenShell.png');hash='E5E5B457785977BD90149BE4FE7958A27D4893CD5E8BBC401DCD96020BD4BBDE';ground=685;n=2;d=5},
    @{name='MotorDepot';source=(Join-Path $family 'Transparent-v1/MotorDepot.png');hash='4134F85D366AD85C3839696EFFFDE30D3CC99CFB1DF60B5E5041BE5DF2351461';ground=735;n=2;d=5},
    @{name='Checkpoint';source=(Join-Path $facing 'Checkpoint-Transparent.png');hash='4C4E6419ABCD1A6545AC3CB28AFE70A44627389627778ED67DAC344BC7F4699F';ground=740;n=2;d=5}
)
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.IO')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
public static class MidNativeProof {
    public static void Facing(string original,string generated,string mask,string source,string exported,string fault){
        using(var old=new Bitmap(original))using(var art=new Bitmap(generated))using(var m=new Bitmap(mask))
        using(var combined=new Bitmap(source))using(var output=new Bitmap(exported)) {
            foreach(var im in new[]{old,art,m,combined,output})
                if(im.Width!=1024||im.Height!=1536)throw new InvalidDataException("FACING_DIMENSIONS");
            for(int y=0;y<1536;y++)for(int x=0;x<1024;x++){
                int expectedColor=(y<740?art:old).GetPixel(x,y).ToArgb();
                if(combined.GetPixel(x,y).ToArgb()!=expectedColor)throw new InvalidDataException("COMPOSITE_SOURCE");
                int maskValue=m.GetPixel(x,y).ToArgb();
                if(maskValue!=Color.White.ToArgb()&&maskValue!=Color.Black.ToArgb())throw new InvalidDataException("MASK_INVALID");
                int expected=maskValue==Color.Black.ToArgb()?0:expectedColor;
                int actual=output.GetPixel(x,y).ToArgb();
                if(fault=="MatteLeak"&&x==0&&y==0)actual=Color.Black.ToArgb();
                if(fault=="CliffChanged"&&x==512&&y==1000)actual^=1;
                if(y>=740&&actual!=old.GetPixel(x,y).ToArgb())throw new InvalidDataException("CLIFF_CHANGED");
                if(actual!=expected)throw new InvalidDataException("FACING_MASK_MISMATCH");
                int a=(actual>>24)&255;if(a!=0&&a!=255)throw new InvalidDataException("SOFT_ALPHA");
            }
        }
    }
    public static void Scale(string source,string image,int sourceGround,int n,int d,string fault){
        using(var art=new Bitmap(source))using(var output=new Bitmap(image)){
            if(output.Width!=512||output.Height!=460)throw new InvalidDataException("SCALE_DIMENSIONS");
            int left=(512-art.Width*n/d)/2,top=340-sourceGround*n/d;
            if(fault=="Misaligned")top++;
            bool injected=false;
            for(int y=0;y<460;y++)for(int x=0;x<512;x++){
                int rx=x-left,ry=y-top,expected=0;
                if(rx>=0&&ry>=0){
                    int sx=(2*rx+1)*d/(2*n),sy=(2*ry+1)*d/(2*n);
                    if(sx<art.Width&&sy<art.Height){
                        Color c=art.GetPixel(sx,sy);expected=c.A==0?0:c.ToArgb();
                    }
                }
                int actual=output.GetPixel(x,y).ToArgb();
                if(fault=="WrongSample"&&!injected&&expected!=0){actual^=1;injected=true;}
                if(actual!=expected)throw new InvalidDataException("SCALE_SAMPLE_MISMATCH");
            }
        }
    }
}
'@
$generated=Join-Path $family 'FacingStudy-v1/Checkpoint-opposite-original.png'
if((Get-FileHash $generated).Hash -ne '49335F9D35A53D86AAEB71840A170516F3D9DFCDBB2BBE5BA06F73D1616E23AB'){throw 'GENERATED_CHANGED'}
$old=Join-Path $family 'Transparent-v1/Checkpoint.png'
if((Get-FileHash $old).Hash -ne 'E29115A5686EF2805AD69E5166278CEF5DDF030E3488C47569DF826E0CE4D8B0'){throw 'ORIGINAL_CHANGED'}
[MidNativeProof]::Facing($old,$generated,(Join-Path $facing 'Checkpoint-Mask.png'),(Join-Path $facing 'Checkpoint-Source.png'),(Join-Path $facing 'Checkpoint-Transparent.png'),$Fault)
Write-Output 'PASS facing composite: selected upper mask, exact generated upper RGB, all796 lower rows unchanged, hard alpha.'
foreach($entry in $entries){
    if((Get-FileHash $entry.source).Hash -ne $entry.hash){throw "SOURCE_CHANGED_$($entry.name)"}
    [MidNativeProof]::Scale($entry.source,(Join-Path $candidate "$($entry.name)-Upper.png"),$entry.ground,$entry.n,$entry.d,$Fault)
    Write-Output "PASS $($entry.name): exact sampled pixels at the explicit ratio and common study datum."
}
if($Fault -ne 'None'){throw 'FAULT_WAS_NOT_REJECTED'}
Write-Output 'Scope: offline color/alpha/scale proof only; no native game or deep-coverage acceptance.'
