param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$output=[IO.Path]::GetFullPath($OutputDirectory)
$allowed=(Join-Path $root 'Art/Candidates/MawHangingFiber-v1')+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase) -or (Test-Path -LiteralPath $output)){throw 'NEW_CANDIDATE_REQUIRED'}
$source=Join-Path $root 'Art/Candidates/MawHangingFiber-v1/source.png'
New-Item -ItemType Directory -Path $output | Out-Null
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;using System.Drawing;using System.Drawing.Imaging;using System.IO;
public static class HangingFiberFit {
 static readonly Color[] palette={Color.FromArgb(30,23,16),Color.FromArgb(50,36,19),Color.FromArgb(73,47,20),Color.FromArgb(99,64,25),Color.FromArgb(126,80,29),Color.FromArgb(158,103,34),Color.FromArgb(190,137,58),Color.FromArgb(218,170,90)};
 static bool Keep(Color c)=>!(c.R>180&&c.B>150&&c.G<100);
 static Color Fit(Color c){int best=int.MaxValue;Color found=palette[0];foreach(Color p in palette){int d=(c.R-p.R)*(c.R-p.R)+(c.G-p.G)*(c.G-p.G)+(c.B-p.B)*(c.B-p.B);if(d<best){best=d;found=p;}}return found;}
 public static string Run(string path,string output){
  using var source=new Bitmap(path);int l=source.Width,r=-1,t=source.Height,b=-1;
  for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++)if(Keep(source.GetPixel(x,y))){l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);b=Math.Max(b,y);}
  if(r<l)throw new Exception("EMPTY_SOURCE");double scale=(b-t+1)/96.0;int width=(int)Math.Ceiling((r-l+1)/scale),offset=(16-width)/2;
  if(width>16)throw new Exception("TOO_WIDE_NO_WARP");
  using var art=new Bitmap(16,96);using var mask=new Bitmap(16,96);
  for(int y=0;y<96;y++)for(int x=0;x<16;x++){art.SetPixel(x,y,palette[0]);mask.SetPixel(x,y,Color.Black);}
  for(int y=0;y<96;y++)for(int x=0;x<width;x++){
   int sx=l+(int)Math.Floor((x+0.5)*scale),sy=t+(int)Math.Floor((y+0.5)*scale);
   if(sx>r||sy>b)continue;Color c=source.GetPixel(sx,sy);if(!Keep(c))continue;
   art.SetPixel(x+offset,y,Fit(c));mask.SetPixel(x+offset,y,Color.White);
  }
  art.Save(Path.Combine(output,"color-master.png"),ImageFormat.Png);mask.Save(Path.Combine(output,"mask.png"),ImageFormat.Png);
  return $"bbox={l},{t},{r-l+1},{b-t+1}; uniformScale={scale:R}; occupiedWidth={width}; offset={offset}; 8 colors; no silhouette warp";
 }
 public static void Sheet(string path,string output){using var art=new Bitmap(path);using var sheet=new Bitmap(18,108);
  for(int y=0;y<96;y++)for(int x=0;x<16;x++)sheet.SetPixel(x,y/16*18+y%16,art.GetPixel(x,y));sheet.Save(output,ImageFormat.Png);
 }
}
'@
$fit=[HangingFiberFit]::Run($source,$output)
& pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $output 'color-master.png') -MaskPath (Join-Path $output 'mask.png') -OutputPath (Join-Path $output 'fiber.png')
if($LASTEXITCODE -ne 0){throw 'HARD_MASK_FAILED'}
[HangingFiberFit]::Sheet((Join-Path $output 'fiber.png'),(Join-Path $output 'atlas.png'))
$record=@{schemaVersion=1;scope='Candidate only; six ordered 16px sections. Shorter growing strands end at a broken section, not an invented tip.';fitting=$fit;sourceSHA256=(Get-FileHash $source).Hash;atlasSHA256=(Get-FileHash (Join-Path $output 'atlas.png')).Hash}
[IO.File]::WriteAllText((Join-Path $output 'recipe.json'),($record|ConvertTo-Json))
Write-Output $fit
