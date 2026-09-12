param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$source=Join-Path $root 'Art/Candidates/MawCaveBackground-v1/master-v1.png'
$pin='57E162FB20A74AA5CB74A4F0A3730D32D939B13CBA09CC35FC5D7937EF5B26F3'
if((Get-FileHash -LiteralPath $source).Hash -ne $pin){throw 'SOURCE_CHANGED'}
$output=[IO.Path]::GetFullPath($OutputDirectory)
$prefix=[IO.Path]::GetFullPath((Join-Path $root 'Art/Candidates/MawCaveBackground-v1'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase) -or (Test-Path -LiteralPath $output)){throw 'NEW_CANDIDATE_DIRECTORY_REQUIRED'}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll'|Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;using System.Drawing;using System.Drawing.Imaging;using System.IO;using System.Collections.Generic;
public static class MawCavePixelStudy {
 public static readonly string[] Palette={"101211","191b1c","25272a","32353a","43454a","5b5b5c","202019","2e2c23","3e382c","514535","68563e","302619","4d3720","6c4928","926432","b38a49","d2ac65","4a4438","625848","7d7059","968873","303941","495361","626e7e","7d8996"};
 static Color Nearest(int r,int g,int b,Color[] palette){int distance=int.MaxValue;Color found=palette[0];foreach(Color p in palette){int d=(r-p.R)*(r-p.R)+(g-p.G)*(g-p.G)+(b-p.B)*(b-p.B);if(d<distance){distance=d;found=p;}}return found;}
 public static long[] Run(string path,string output){
  using var source=new Bitmap(path);
  if(source.Width!=1536||source.Height!=1024)throw new InvalidDataException("SOURCE_DIMENSIONS");
  Color[] palette=Array.ConvertAll(Palette,h=>ColorTranslator.FromHtml("#"+h));
  var inputColors=new HashSet<int>();var outputColors=new HashSet<int>();long changed=0;
  using var result=new Bitmap(source.Width,source.Height,PixelFormat.Format32bppArgb);
  for(int y=0;y<source.Height;y+=2)for(int x=0;x<source.Width;x+=2){
   int r=0,g=0,b=0;
   for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++){Color c=source.GetPixel(x+dx,y+dy);if(c.A!=255)throw new InvalidDataException("MASTER_MUST_BE_OPAQUE");inputColors.Add(c.ToArgb());r+=c.R;g+=c.G;b+=c.B;}
   Color p=Nearest((r+2)/4,(g+2)/4,(b+2)/4,palette);outputColors.Add(p.ToArgb());
   for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++){result.SetPixel(x+dx,y+dy,p);if(source.GetPixel(x+dx,y+dy).ToArgb()!=p.ToArgb())changed++;}
  }
  string file=Path.Combine(output,"master-pixel2.png");using(var f=new FileStream(file,FileMode.CreateNew))result.Save(f,ImageFormat.Png);
  using var decoded=new Bitmap(file);long checkedPixels=0;
  for(int y=0;y<decoded.Height;y++)for(int x=0;x<decoded.Width;x++){
   Color actual=decoded.GetPixel(x,y);if(actual.A!=255||actual.ToArgb()!=result.GetPixel(x,y).ToArgb()||actual.ToArgb()!=decoded.GetPixel(x/2*2,y/2*2).ToArgb())throw new InvalidDataException("OUTPUT_CHANGED_OR_GRID_MISMATCH");checkedPixels++;
  }
  return new long[]{source.Width,source.Height,inputColors.Count,outputColors.Count,changed,checkedPixels};
 }
}
'@
New-Item -ItemType Directory -Path $output|Out-Null
$metrics=[MawCavePixelStudy]::Run($source,$output)
$record=[ordered]@{schemaVersion=1;sourceSHA256=$pin;outputSHA256=(Get-FileHash -LiteralPath (Join-Path $output 'master-pixel2.png')).Hash;width=$metrics[0];height=$metrics[1];sourceColors=$metrics[2];outputColors=$metrics[3];changedColorPixels=$metrics[4];verifiedPixels=$metrics[5];grid=2;palette=[MawCavePixelStudy]::Palette;scope='Explicit whole-master color fitting and 2x2 block averaging. Same canvas, not invariant-preserving cutout or resolution improvement. Opaque art study, no runtime or mask approval.'}
$json=$record|ConvertTo-Json -Depth 4
$report=Join-Path $output 'recipe.json'
$stream=[IO.File]::Open($report,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$bytes=[Text.Encoding]::UTF8.GetBytes($json);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output $json
