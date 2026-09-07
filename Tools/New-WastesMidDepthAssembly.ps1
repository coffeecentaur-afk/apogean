param([Parameter(Mandatory)][string]$OutputDirectory)
# Scoped deterministic source assembly; does not generate new painted detail.
# Original architecture at study y<=340 is immutable. Only footing/cliff can change.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'USE_NEW_OUTPUT_DIRECTORY'}
$entries=@(
 @{name='Station';upper='Art/Candidates/WastesStationBridge-v1/Study/Station-Upper.png';hash='81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861';lowerHash='C18F4629C0503D44AE4DB4F9FBEDAFD15682879EB3654819F1154DAFD7229DF6';n=3;d=4;dx=-27;dy=226;seamStart=500;seamEnd=538;black=$false},
 @{name='MotorDepot';upper='Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1/MotorDepot-Upper.png';hash='AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890';lowerHash='F2C3491A7DC19C0543CF90285487F3BD288640DB6B95CCFD65A770DD2E637B36';n=2;d=3;dx=4;dy=243;seamStart=475;seamEnd=510;black=$false},
 @{name='BrokenShell';upper='Art/Candidates/WastesRuinUpperStyle-v1/Study/BrokenShell-Upper.png';hash='B6F3DA533BCE10F436B15A1CE25E368B660EC17CCD6A33E526D62FA7DD34A597';lowerHash='58D3815436DE81EA1E2DC2E02828D68601AE8A8A8022F1211E158B196690031A';n=2;d=3;dx=4;dy=187;seamStart=444;seamEnd=455;black=$true},
 @{name='Checkpoint';upper='Art/Candidates/WastesRuinUpperStyle-v1/Study/Checkpoint-Upper.png';hash='51EF9199EDD1B4C012729E29197825A86C2F250C0031B366E7C6BC5EC6B742CC';lowerHash='457CF61A820BC193C0C641857AE36EE040BEC029EF820E50CC45B66878637C46';n=2;d=3;dx=4;dy=83;seamStart=444;seamEnd=455;black=$false}
)
foreach($e in $entries){
 $e.upperFull=Join-Path $root $e.upper
 $e.lower="Art/Candidates/WastesMidBank-v1/Generated/$($e.name)-original.png"
 $e.lowerFull=Join-Path $root $e.lower
 if((Get-FileHash -LiteralPath $e.upperFull).Hash -ne $e.hash){throw 'APPROVED_UPPER_CHANGED'}
 if((Get-FileHash -LiteralPath $e.lowerFull).Hash -ne $e.lowerHash){throw 'LOWER_SOURCE_CHANGED'}
}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
public static class WastesDepthAssembly {
 const int W=512,H=1408;
 static bool Matte(Color c,bool black) => black ? Math.Max(c.R,Math.Max(c.G,c.B))<12 : Math.Min(c.R,Math.Min(c.G,c.B))>=195 && Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B))<=20;
 public static void Make(string upperPath,string lowerPath,string output,string name,int n,int d,int dx,int dy,int y0,int y1,bool black) {
  using(var upper=new Bitmap(upperPath))using(var source=new Bitmap(lowerPath)) {
   if(upper.Width!=W||upper.Height!=460||source.Width!=756||source.Height!=2079)throw new InvalidDataException("DIMENSIONS");
   var low=new Color[W*H];var original=new Color[W*H];
   // This proposal only masks the exterior of the lower cliff. No building/window
   // recognition: everything at or above the approved architecture is excluded.
   int sourceStart=(int)Math.Floor((441-dy+.5)*d/n);
   int[] left=new int[source.Height],right=new int[source.Height];
   for(int y=sourceStart;y<source.Height;y++){
    int l=0,r=source.Width-1;
    while(l<=r&&Matte(source.GetPixel(l,y),black))l++;
    while(r>=l&&Matte(source.GetPixel(r,y),black))r--;
    left[y]=l;right[y]=r;
   }
   using(var mask=new Bitmap(W,H,PixelFormat.Format32bppArgb))
   using(var color=new Bitmap(W,H,PixelFormat.Format32bppArgb))
   using(var selector=new Bitmap(W,H,PixelFormat.Format32bppArgb))
   using(var combined=new Bitmap(W,H,PixelFormat.Format32bppArgb)) {
    for(int y=0;y<H;y++)for(int x=0;x<W;x++){
     Color a=y>=100&&y<560?upper.GetPixel(x,y-100):Color.Transparent;
     original[y*W+x]=a.A==0?Color.FromArgb(0,0,0,0):a;
     int sx=(int)Math.Floor((x-dx+.5)*d/n),sy=(int)Math.Floor((y-dy+.5)*d/n);
     Color b=Color.FromArgb(0,0,0,0);
     if(y>440&&sx>=0&&sx<source.Width&&sy>=sourceStart&&sy<source.Height&&sx>=left[sy]&&sx<=right[sy])b=source.GetPixel(sx,sy);
     if(b.A!=0&&b.A!=255)throw new InvalidDataException("SOFT_SOURCE");
     low[y*W+x]=b;
    }
    // Separate color-only repair of the neutral baked-matte rim. Keep alpha and
    // original material donors, never globally key gray rock or darken the art.
    var rawLow=(Color[])low.Clone();var donors=new List<int[]>();
    for(int y=441;y<H;y++)for(int x=0;x<W;x++){
     int p=y*W+x;Color c=rawLow[p];
     int lo=Math.Min(c.R,Math.Min(c.G,c.B)),hi=Math.Max(c.R,Math.Max(c.G,c.B));
     if(c.A==0||lo<110||hi-lo>40)continue;
     bool edge=false;
     for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++){
      int nx=x+ox,ny=y+oy;
      if(nx<0||nx>=W||ny<441||ny>=H||rawLow[ny*W+nx].A==0)edge=true;
     }
     if(!edge)continue;
     int best=-1,distance=37;
     for(int oy=-6;oy<=6;oy++)for(int ox=-6;ox<=6;ox++){
      int nx=x+ox,ny=y+oy,dist=ox*ox+oy*oy;
      if(dist==0||dist>=distance||nx<0||nx>=W||ny<441||ny>=H)continue;
      Color a=rawLow[ny*W+nx];
      if(a.A!=255||a.R-a.B<24||c.R+c.G+c.B-a.R-a.G-a.B<60)continue;
      best=ny*W+nx;distance=dist;
     }
     if(best>=0){low[p]=rawLow[best];donors.Add(new[]{x,y,best%W,best/W});}
    }
    // Minimum-error continuous cut, only within the existing soil/footing.
    int rows=y1-y0+1;var cost=new double[W*rows];var parent=new int[W*rows];
    for(int x=0;x<W;x++)for(int j=0;j<rows;j++){
     int y=y0+j;Color a=original[y*W+x],b=low[y*W+x];
     double here=a.A!=b.A?90000:a.A==0?0:Math.Abs(a.R-b.R)+Math.Abs(a.G-b.G)+Math.Abs(a.B-b.B);
     here+=Math.Abs(j-rows*.5)*.15;
     double best=0;int prev=j;
     if(x>0){best=double.MaxValue;for(int k=Math.Max(0,j-2);k<=Math.Min(rows-1,j+2);k++){
      double v=cost[(x-1)*rows+k]+Math.Abs(j-k)*2;
      if(v<best){best=v;prev=k;}
     }}
     cost[x*rows+j]=best+here;parent[x*rows+j]=prev;
    }
    var seam=new int[W];int pick=0;
    for(int j=1;j<rows;j++)if(cost[(W-1)*rows+j]<cost[(W-1)*rows+pick])pick=j;
    for(int x=W-1;x>=0;x--){seam[x]=y0+pick;pick=parent[x*rows+pick];}
    for(int y=0;y<H;y++)for(int x=0;x<W;x++){
     bool useUpper=y<=440||y<seam[x];
     Color c=useUpper?original[y*W+x]:low[y*W+x];
     color.SetPixel(x,y,c.A==0?Color.Black:c);
     mask.SetPixel(x,y,c.A==0?Color.Black:Color.White);
     combined.SetPixel(x,y,c);
     selector.SetPixel(x,y,useUpper?Color.White:Color.Black);
    }
    Directory.CreateDirectory(output);
    color.Save(Path.Combine(output,name+"-Color.png"),ImageFormat.Png);
    mask.Save(Path.Combine(output,name+"-Mask.png"),ImageFormat.Png);
    selector.Save(Path.Combine(output,name+"-Selector.png"),ImageFormat.Png);
    using(var gmask=new Bitmap(W,H,PixelFormat.Format32bppArgb))
    using(var gcolor=new Bitmap(W,H,PixelFormat.Format32bppArgb)) {
     for(int y=0;y<H;y++)for(int x=0;x<W;x++){
      Color b=low[y*W+x];gmask.SetPixel(x,y,b.A==0?Color.Black:Color.White);gcolor.SetPixel(x,y,b.A==0?Color.Black:b);
     }
     gmask.Save(Path.Combine(output,name+"-LowerMask.png"),ImageFormat.Png);
     gcolor.Save(Path.Combine(output,name+"-LowerColor.png"),ImageFormat.Png);
    }
    // Preview only; final asset must go through the reviewed-mask exporter.
    foreach(bool dark in new[]{true,false})using(var board=new Bitmap(W,H,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(board)){
     g.Clear(dark?Color.FromArgb(40,36,41):Color.FromArgb(151,185,201));g.DrawImageUnscaled(combined,0,0);
     board.Save(Path.Combine(output,name+(dark?"-Dark.png":"-Light.png")),ImageFormat.Png);
    }
    File.WriteAllText(Path.Combine(output,name+"-seam.json"),JsonSerializer.Serialize(seam));
    File.WriteAllText(Path.Combine(output,name+"-edge-donors.json"),JsonSerializer.Serialize(donors));
   }
  }
 }
 public static void Board(string output) {
  using(var b=new Bitmap(2048,900,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b))using(var f=new Font("Consolas",12)){
   g.Clear(Color.FromArgb(40,36,41));string[] names={"Station","MotorDepot","BrokenShell","Checkpoint"};
   for(int i=0;i<4;i++)using(var a=new Bitmap(Path.Combine(output,names[i]+"-Dark.png"))){g.DrawImageUnscaled(a,i*512,-40);g.DrawString(names[i]+" — OFFLINE JOIN PROBE",f,Brushes.White,i*512+8,8);}
   b.Save(Path.Combine(output,"Join-Comparison.png"),ImageFormat.Png);
  }
 }
}
'@
$records=@()
foreach($e in $entries){
 [WastesDepthAssembly]::Make($e.upperFull,$e.lowerFull,$output,$e.name,$e.n,$e.d,$e.dx,$e.dy,$e.seamStart,$e.seamEnd,$e.black)
 $records+=[ordered]@{name=$e.name;upper=$e.upper;upperSHA256=$e.hash;lower=$e.lower;lowerSHA256=$e.lowerHash;n=$e.n;d=$e.d;dx=$e.dx;dy=$e.dy;seamStart=$e.seamStart;seamEnd=$e.seamEnd;blackMatte=$e.black}
}
[WastesDepthAssembly]::Board($output)
[ordered]@{recipe='WastesMidDepth-v2';width=512;height=1408;upperOffsetY=100;preservedThroughRow=440;soilRow=440;state='UNREVIEWED mask and footing proposal; not runtime';assets=$records}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $output 'assembly.json')
Write-Output "Offline depth proposals: $output"
