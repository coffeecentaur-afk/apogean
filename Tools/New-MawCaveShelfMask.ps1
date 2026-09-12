param([Parameter(Mandatory)][string]$OutputDirectory,[ValidateSet(128,255)][int]$MinimumAlpha=255)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/MawCaveBackground-v1/shelf-source-v1.png'
$pin='3E8BF03E2E1489147830E65CEC507C30C9AC5A58D6CBAC1A177CE417341ACFA0'
if((Get-FileHash -LiteralPath $source).Hash -ne $pin){throw 'SOURCE_CHANGED'}
$output=[IO.Path]::GetFullPath($OutputDirectory)
$prefix=[IO.Path]::GetFullPath((Join-Path $root 'Art/Candidates/MawCaveBackground-v1'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase) -or (Test-Path -LiteralPath $output)){throw 'NEW_PROPOSAL_DIRECTORY_REQUIRED'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;using System.Drawing;using System.Drawing.Imaging;using System.IO;
public static class MawShelfMask {
 public static long[] Run(string source,string output,int minimumAlpha){using var art=new Bitmap(source);
  if(art.Width!=1536||art.Height!=1024)throw new InvalidDataException("WRONG_CANVAS");
  using var mask=new Bitmap(art.Width,art.Height);using var colors=new Bitmap(art.Width,art.Height);using var dark=new Bitmap(art.Width,art.Height);using var light=new Bitmap(art.Width,art.Height);
  long zero=0,partial=0,opaque=0,nonzeroHidden=0,kept=0;int l=art.Width,t=art.Height,r=-1,b=-1;
  for(int y=0;y<art.Height;y++)for(int x=0;x<art.Width;x++){
   Color c=art.GetPixel(x,y);bool keep=c.A>=minimumAlpha;
   if(c.A==0){zero++;if(c.R!=0||c.G!=0||c.B!=0)nonzeroHidden++;}else if(c.A==255)opaque++;else partial++;
   if(keep){kept++;l=Math.Min(l,x);t=Math.Min(t,y);r=Math.Max(r,x);b=Math.Max(b,y);}
   Color solid=Color.FromArgb(255,c.R,c.G,c.B);colors.SetPixel(x,y,solid);
   mask.SetPixel(x,y,keep?Color.White:Color.Black);dark.SetPixel(x,y,keep?solid:Color.FromArgb(22,26,34));light.SetPixel(x,y,keep?solid:Color.FromArgb(207,224,221));
  }
  foreach(var item in new[]{(colors,"color-master.png"),(mask,"mask.png"),(dark,"preview-dark.png"),(light,"preview-light.png")})using(var f=new FileStream(Path.Combine(output,item.Item2),FileMode.CreateNew))item.Item1.Save(f,ImageFormat.Png);
  return new long[]{zero,partial,opaque,nonzeroHidden,l,t,r<0?0:r-l+1,b<0?0:b-t+1,kept};
 }
}
'@
New-Item -ItemType Directory -Path $output|Out-Null
$m=[MawShelfMask]::Run($source,$output,$MinimumAlpha)
$record=[ordered]@{schemaVersion=2;sourceSHA256=$pin;maskSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'mask.png')).Hash;colorSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'color-master.png')).Hash;sourceTransparent=$m[0];sourcePartial=$m[1];sourceOpaque=$m[2];hiddenRgbNonzero=$m[3];keptBounds=@($m[4],$m[5],$m[6],$m[7]);keptPixels=$m[8];minimumAlpha=$MinimumAlpha;nonempty=($m[8] -gt 0);review='proposal';rule='Pinned source alpha threshold only; RGB unchanged in color-master, alpha explicitly hardened to255 before reviewed mask export. No RGB key, dilation, or claim that every partial pixel is extraneous.';scope='Inspect thin fibers, enclosed gaps and complete underside on both backings before exact export. Art/pixel density/depth separate.'}
$json=$record|ConvertTo-Json -Depth 4
$stream=[IO.File]::Open((Join-Path $output 'proposal.json'),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$bytes=[Text.Encoding]::UTF8.GetBytes($json);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output $json
if($m[8] -eq 0){throw 'EMPTY_PROPOSAL_RETAINED_DO_NOT_EXPORT'}
