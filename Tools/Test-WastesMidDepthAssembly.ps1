param([string]$CandidateDirectory='', [ValidateSet('None','Architecture','SoftAlpha','Support','Sample')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(!$CandidateDirectory){$CandidateDirectory=Join-Path $root 'Art/Candidates/WastesMidDepth-v2/EdgeReview'}
$dir=(Resolve-Path -LiteralPath $CandidateDirectory).Path
$recipe=Get-Content -Raw -LiteralPath (Join-Path $dir 'assembly.json')|ConvertFrom-Json
if($recipe.recipe -ne 'WastesMidDepth-v2' -or $recipe.width -ne 512 -or $recipe.height -ne 1408 -or $recipe.preservedThroughRow -ne 440){throw 'RECIPE'}
$upperHashes=@{Station='81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861';MotorDepot='AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890';BrokenShell='B6F3DA533BCE10F436B15A1CE25E368B660EC17CCD6A33E526D62FA7DD34A597';Checkpoint='51EF9199EDD1B4C012729E29197825A86C2F250C0031B366E7C6BC5EC6B742CC'}
$lowerHashes=@{Station='C18F4629C0503D44AE4DB4F9FBEDAFD15682879EB3654819F1154DAFD7229DF6';MotorDepot='F2C3491A7DC19C0543CF90285487F3BD288640DB6B95CCFD65A770DD2E637B36';BrokenShell='58D3815436DE81EA1E2DC2E02828D68601AE8A8A8022F1211E158B196690031A';Checkpoint='457CF61A820BC193C0C641857AE36EE040BEC029EF820E50CC45B66878637C46'}
if(@($recipe.assets).Count -ne 4 -or @($recipe.assets.name|Select-Object -Unique).Count -ne 4){throw 'ASSET_SET'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json','System.Console','System.Memory')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidDepthAudit {
 static void Fail(string s){throw new InvalidDataException(s);}
 static int Source(int dest,int offset,int n,int d)=>(int)Math.Floor(((long)(dest-offset)*2+1)*d/(2.0*n));
 static void Alpha(Color c){if(c.A!=0&&c.A!=255)Fail("SOFT_ALPHA");if(c.A==0&&(c.R!=0||c.G!=0||c.B!=0))Fail("HIDDEN_RGB");}
 public static void Check(string root,string dir,string specJson,string mutation){
  using(var json=JsonDocument.Parse(specJson)){
   var e=json.RootElement;string name=e.GetProperty("name").GetString();
   int n=e.GetProperty("n").GetInt32(),d=e.GetProperty("d").GetInt32(),dx=e.GetProperty("dx").GetInt32(),dy=e.GetProperty("dy").GetInt32();
   int y0=e.GetProperty("seamStart").GetInt32(),y1=e.GetProperty("seamEnd").GetInt32();
   if(n<1||n>d||d>4||y0<441||y1>559||y1<y0)Fail("FIT_CONTRACT");
   using(var upper=new Bitmap(Path.Combine(root,e.GetProperty("upper").GetString())))
   using(var lower=new Bitmap(Path.Combine(root,e.GetProperty("lower").GetString())))
   using(var mask=new Bitmap(Path.Combine(dir,name+"-Mask.png")))
   using(var color=new Bitmap(Path.Combine(dir,name+"-Color.png")))
   using(var lowerMask=new Bitmap(Path.Combine(dir,name+"-LowerMask.png")))
   using(var lowerColor=new Bitmap(Path.Combine(dir,name+"-LowerColor.png")))
   using(var selector=new Bitmap(Path.Combine(dir,name+"-Selector.png")))
   using(var asset=new Bitmap(Path.Combine(dir,name+".png"))){
    foreach(var b in new[]{mask,color,lowerMask,lowerColor,selector,asset})if(b.Width!=512||b.Height!=1408)Fail("DIMENSIONS");
    if(upper.Width!=512||upper.Height!=460||lower.Width!=756||lower.Height!=2079)Fail("SOURCE_DIMENSIONS");
    var seam=JsonSerializer.Deserialize<int[]>(File.ReadAllText(Path.Combine(dir,name+"-seam.json")));
    if(seam.Length!=512)Fail("SEAM_LENGTH");
    for(int x=0;x<512;x++)if(seam[x]<y0||seam[x]>y1||(x>0&&Math.Abs(seam[x]-seam[x-1])>2))Fail("SEAM_BOUNDS");
    var donors=JsonSerializer.Deserialize<int[][]>(File.ReadAllText(Path.Combine(dir,name+"-edge-donors.json")));
    var donorMap=new Dictionary<int,int>();
    foreach(var a in donors){
     if(a.Length!=4||a[0]<0||a[0]>=512||a[1]<441||a[1]>=1408||a[2]<0||a[2]>=512||a[3]<441||a[3]>=1408)Fail("DONOR_BOUNDS");
     int dist=(a[0]-a[2])*(a[0]-a[2])+(a[1]-a[3])*(a[1]-a[3]);if(dist<1||dist>36)Fail("DONOR_RADIUS");
     int p=a[1]*512+a[0];if(donorMap.ContainsKey(p))Fail("DUPLICATE_DONOR");donorMap.Add(p,a[3]*512+a[2]);
     if(lowerMask.GetPixel(a[0],a[1]).ToArgb()!=Color.White.ToArgb()||lowerMask.GetPixel(a[2],a[3]).ToArgb()!=Color.White.ToArgb())Fail("DONOR_ALPHA");
     bool edge=false;for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++){
      int x=a[0]+ox,y=a[1]+oy;if(x<0||x>=512||y<441||y>=1408||lowerMask.GetPixel(x,y).ToArgb()==Color.Black.ToArgb())edge=true;
     }if(!edge)Fail("DONOR_INTERIOR");
    }
    if(mutation=="Architecture")asset.SetPixel(200,350,Color.Magenta);
    if(mutation=="SoftAlpha")asset.SetPixel(256,800,Color.FromArgb(128,40,30,20));
    if(mutation=="Support")for(int x=0;x<512;x++)asset.SetPixel(x,1000,Color.FromArgb(0,0,0,0));
    if(mutation=="Sample")asset.SetPixel(256,850,Color.Magenta);
    long preserved=0,lowerPixels=0;
    for(int y=0;y<1408;y++){
     int support=0;
     for(int x=0;x<512;x++){
      Color actual=asset.GetPixel(x,y);Alpha(actual);if(actual.A==255)support++;
      if(y<=440){
       Color a=y>=100?upper.GetPixel(x,y-100):Color.FromArgb(0,0,0,0);
       if(actual.ToArgb()!=a.ToArgb())Fail("ARCHITECTURE_CHANGED");preserved++;
      }
      Color m=mask.GetPixel(x,y),lm=lowerMask.GetPixel(x,y),s=selector.GetPixel(x,y);
      foreach(Color c in new[]{m,lm,s})if(c.ToArgb()!=Color.White.ToArgb()&&c.ToArgb()!=Color.Black.ToArgb())Fail("MASK_VALUE");
      bool chooseUpper=y<=440||y<seam[x];
      if((s.ToArgb()==Color.White.ToArgb())!=chooseUpper)Fail("SELECTOR");
      Color expectedLow=Color.FromArgb(0,0,0,0);
      if(lm.ToArgb()==Color.White.ToArgb()){
       if(y<=440)Fail("LOWER_IN_ARCHITECTURE");
       int p=y*512+x;int donor=donorMap.TryGetValue(p,out int at)?at:p;
       int sx=Source(donor%512,dx,n,d),sy=Source(donor/512,dy,n,d);
       if(sx<0||sx>=lower.Width||sy<0||sy>=lower.Height)Fail("SAMPLE_BOUNDS");
       expectedLow=lower.GetPixel(sx,sy);lowerPixels++;
       if(lowerColor.GetPixel(x,y).ToArgb()!=expectedLow.ToArgb())Fail("LOWER_PROVENANCE");
      }
      Color expected=chooseUpper?(y>=100&&y<560?upper.GetPixel(x,y-100):Color.FromArgb(0,0,0,0)):expectedLow;
      if(expected.A==0)expected=Color.FromArgb(0,0,0,0);
      if((m.ToArgb()==Color.White.ToArgb())!=(expected.A==255))Fail("MASK_PROVENANCE");
      if(expected.A==255&&color.GetPixel(x,y).ToArgb()!=expected.ToArgb())Fail("COLOR_PROVENANCE");
      if(actual.ToArgb()!=expected.ToArgb())Fail(mutation=="Support"?"SUPPORT_CHANGED":"PIXEL_PROVENANCE");
     }
     if(y>=560&&support<90)Fail("MISSING_CLIFF_SUPPORT");
    }
    Console.WriteLine($"PASS {name}: {preserved} protected pixels, {lowerPixels} lower samples, {donors.Length} bounded edge donors; full720896-pixel export audit. Not visual approval.");
   }
  }
 }
}
'@
foreach($e in $recipe.assets){
 if(!$upperHashes.ContainsKey($e.name)){throw 'UNKNOWN_ASSET'}
 if((Get-FileHash -LiteralPath (Join-Path $root $e.upper)).Hash -ne $upperHashes[$e.name] -or (Get-FileHash -LiteralPath (Join-Path $root $e.lower)).Hash -ne $lowerHashes[$e.name]){throw 'SOURCE_HASH'}
 [MidDepthAudit]::Check($root,$dir,($e|ConvertTo-Json -Compress),$Mutation)
}
